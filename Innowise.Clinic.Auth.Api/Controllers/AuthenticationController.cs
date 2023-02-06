using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text;
using Innowise.Clinic.Auth.Constants;
using Innowise.Clinic.Auth.Dto;
using Innowise.Clinic.Auth.Extensions;
using Innowise.Clinic.Auth.Jwt.Interfaces;
using Innowise.Clinic.Auth.Mail;
using Innowise.Clinic.Auth.Mail.Constants;
using Innowise.Clinic.Auth.Mail.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace Innowise.Clinic.Auth.Api.Controllers;

/// <summary>
///  Authentication controller.
///  It registers patients, returns security + refresh tokens for all users who successfully logged in
///  and revokes tokens for whose willing to log out.
/// </summary>
[ApiController]
[Route(ControllerRoutes.AuthenticationControllerRoute)]
[Produces("application/json")]
public class AuthenticationController : ControllerBase
{
    private readonly UserManager<IdentityUser<Guid>> _userManager;
    private readonly SignInManager<IdentityUser<Guid>> _signInManager;
    private readonly ITokenGenerator _tokenGenerator;
    private readonly ITokenRevoker _tokenRevoker;
    private readonly IEmailHandler _emailHandler;

    /// <inheritdoc />
    public AuthenticationController(UserManager<IdentityUser<Guid>> userManager, ITokenGenerator tokenGenerator,
        SignInManager<IdentityUser<Guid>> signInManager, ITokenRevoker tokenRevoker,
        IEmailHandler emailHandler)
    {
        _userManager = userManager;
        _tokenGenerator = tokenGenerator;
        _signInManager = signInManager;
        _tokenRevoker = tokenRevoker;
        _emailHandler = emailHandler;
    }

    /// <summary>Creates account for unregistered patient.</summary>
    /// <param name="patientCredentials">The email and the password that will be used for signing in.</param>
    /// <returns>
    /// A pair of tokens. A short-living security token and long-living refresh token.
    /// The latter is used to generate new security token.
    /// </returns>
    /// <response code="200">Success. The user has been registered. A pair of tokens is returned</response>
    /// <response code="400"> Fail. Email and/or password haven't passed validation.</response>
    [HttpPost(EndpointRoutes.SignUpEndpointRoute)]
    [ProducesResponseType(typeof(AuthTokenPairDto), 200)]
    [ProducesResponseType(typeof(void), 400)]
    public async Task<ActionResult<AuthTokenPairDto>> RegisterPatient(PatientCredentialsDto patientCredentials)
    {
        var userExists = await _userManager.FindByEmailAsync(patientCredentials.Email);
        if (userExists == null)
        {
            IdentityUser<Guid> user = new()
            {
                Email = patientCredentials.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = patientCredentials.Email
            };
            var signUpResult = await _userManager.CreateAsync(user, patientCredentials.Password);
            if (signUpResult.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, UserRoles.Patient);

                var authTokens = await GenerateJwtAndRefreshToken(user);

                var emailConfirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                byte[] tokenGeneratedBytes = Encoding.UTF8.GetBytes(emailConfirmationToken);
                var tokenEncoded = WebEncoders.Base64UrlEncode(tokenGeneratedBytes);
                var tokenConfirmationLink = Environment.GetEnvironmentVariable("AuthApiUrl") + "auth/email/confirm/" +
                                            tokenEncoded + $"?userid={user.Id}";

                var emailBody = EmailBodyBuilder.BuildBodyForEmailConfirmation(tokenConfirmationLink);

                _emailHandler.SendMessage(patientCredentials.Email, EmailSubjects.EmailConfirmation, emailBody);

                return Ok(authTokens);
            }

            else
            {
                foreach (var error in signUpResult.Errors)
                {
                    ModelState.TryAddModelError(error.Code, error.Description);
                }

                return BadRequest(ModelState);
            }
        }

        else
        {
            return BadRequest();
        }
    }

    /// <summary>Signs in the registered patient.</summary>
    /// <param name="patientCredentials">The email and the password of the patient.</param>
    /// <returns>
    /// A pair of tokens. A short-living security token and long-living refresh token.
    /// The latter is used to generate new security token.
    /// </returns>
    /// <response code="200">Success. The user has been registered. A pair of tokens is returned</response>
    /// <response code="401"> Fail. The account with the provided credentials doesn't exist. </response>
    [HttpPost(EndpointRoutes.SignInEndpointRoute)]
    [ProducesResponseType(typeof(AuthTokenPairDto), 200)]
    [ProducesResponseType(typeof(void), 401)]
    public async Task<ActionResult<AuthTokenPairDto>> SignInAsPatient(PatientCredentialsDto patientCredentials)
    {
        var user = await _userManager.FindByEmailAsync(patientCredentials.Email);
        if (user != null)
        {
            var signInSucceeded =
                await _signInManager.UserManager.CheckPasswordAsync(user, patientCredentials.Password);

            if (signInSucceeded)
            {
                var authTokens = await GenerateJwtAndRefreshToken(user);
                return Ok(authTokens);
            }

            await _tokenRevoker.RevokeAllUserTokensAsync(user.Id);
        }

        return Unauthorized(ApiMessages.FailedLoginMessage);
    }

    /// <summary>Logs out the user. Revokes the refresh token.</summary>
    /// <param name="tokens">The refresh token and security token issued by the system.</param>
    /// <returns>
    /// Successful status code (200) if user is logged out.
    /// </returns>
    /// <response code="200">Success. The user is logged out.</response>
    /// <response code="401">Fail. Provided tokens haven't passed validation. It is necessary to sign in again.</response>
    [HttpPost(EndpointRoutes.SignOutEndpointRoute)]
    [ProducesResponseType(typeof(void), 200)]
    [ProducesResponseType(typeof(void), 401)]
    public async Task<IActionResult> SignOutAsPatient(AuthTokenPairDto tokens,
        [FromServices] ITokenValidator validator)
    {
        try
        {
            await validator.ValidateTokenPairAndExtractPrincipal(tokens, false);
            var tokenId = tokens.GetRefreshTokenId();
            await _tokenRevoker.RevokeTokenAsync(tokenId);
        }

        catch (ApplicationException e)
        {
            return Unauthorized(e.Message);
        }

        return Ok();
    }

    /// <summary>Generates a new security token for the user.</summary>
    /// <param name="tokens">The refresh token and security token issued by the system.</param>
    /// <returns>
    /// New valid security token.
    /// </returns>
    /// <response code="200">Success. A new security token is returned.</response>
    /// <response code="401"> Fail. Provided tokens haven't passed validation. It is necessary to sign in again. </response>
    [HttpPost(EndpointRoutes.RefreshTokenEndpointRoute)]
    [ProducesResponseType(typeof(string), 200)]
    [ProducesResponseType(typeof(void), 401)]
    public async Task<ActionResult<string>> RefreshToken([FromBody] AuthTokenPairDto tokens,
        [FromServices] ITokenValidator validator)
    {
        try
        {
            var principal = await validator.ValidateTokenPairAndExtractPrincipal(tokens, true);
            return _tokenGenerator.GenerateJwtToken(principal);
        }
        catch (Exception e)
        {
            var userId = tokens.GetUserId();
            await _tokenRevoker.RevokeAllUserTokensAsync(userId);
            return Unauthorized();
        }
    }

    /// <summary>
    /// Confirms user email.
    /// </summary>
    /// <param name="emailConfirmationToken">Token sent to the email specified by user and encoded in the email confirmation link.</param>
    /// <param name="userId">Id of the user whose email is being confirmed.</param>
    /// <response code="200"> Success. Email is confirmed.</response>
    /// <response code="400"> Fail. Email is not confirmed. Response contains explanation of the issue. </response>
    [ProducesResponseType(typeof(void), 200)]
    [ProducesResponseType(typeof(string), 400)]
    [HttpGet("email/confirm/{emailConfirmationToken:required}")]
    public async Task<IActionResult> ConfirmUserEmail([FromRoute] string emailConfirmationToken,
        [FromQuery] [Required] string userId)
    {
        var emailConfirmationTokenBytes = WebEncoders.Base64UrlDecode(emailConfirmationToken);
        emailConfirmationToken = Encoding.UTF8.GetString(emailConfirmationTokenBytes);

        var user = await _userManager.FindByIdAsync(userId);

        var confirmation = await _userManager.ConfirmEmailAsync(user, emailConfirmationToken);

        if (!confirmation.Succeeded)
        {
            foreach (var error in confirmation.Errors)
            {
                ModelState.TryAddModelError(error.Code, error.Description);
            }

            return BadRequest(ModelState);
        }

        return Ok();
    }

    private async Task<AuthTokenPairDto> GenerateJwtAndRefreshToken(IdentityUser<Guid> user)
    {
        var principal = await GetRegisteredUserPrincipalAsync(user);
        var jwtToken = _tokenGenerator.GenerateJwtToken(principal);
        var refreshToken = await _tokenGenerator.GenerateRefreshTokenAsync(user.Id);
        var authTokens = new AuthTokenPairDto(jwtToken, refreshToken);

        return authTokens;
    }

    private async Task<ClaimsPrincipal> GetRegisteredUserPrincipalAsync(IdentityUser<Guid> user)
    {
        var getUserRolesTask = _userManager.GetRolesAsync(user);

        var authClaims = new List<Claim>
        {
            new(ClaimTypes.PrimarySid, user.Id.ToString()),
        };

        var userRoles = await getUserRolesTask;
        foreach (var userRole in userRoles)
        {
            authClaims.Add(new Claim(ClaimTypes.Role, userRole));
        }

        var claimsIdentity = new ClaimsIdentity(authClaims);
        var principal = new ClaimsPrincipal(claimsIdentity);
        return principal;
    }
}