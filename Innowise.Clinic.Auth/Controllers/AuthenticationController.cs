using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text;
using Innowise.Clinic.Auth.Constants;
using Innowise.Clinic.Auth.DTO;
using Innowise.Clinic.Auth.Extensions;
using Innowise.Clinic.Auth.Jwt.Interfaces;
using Innowise.Clinic.Auth.Mail;
using Innowise.Clinic.Auth.Mail.Constants;
using Innowise.Clinic.Auth.Mail.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace Innowise.Clinic.Auth.Controllers;

[ApiController]
[Route(ControllerRoutes.AuthenticationControllerRoute)]
public class AuthenticationController : ControllerBase
{
    private readonly UserManager<IdentityUser<Guid>> _userManager;
    private readonly SignInManager<IdentityUser<Guid>> _signInManager;
    private readonly ITokenGenerator _tokenGenerator;
    private readonly ITokenRevoker _tokenRevoker;

    private readonly IEmailHandler _emailHandler;

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


    [HttpPost(EndpointRoutes.SignUpEndpointRoute)]
    public async Task<ActionResult<AuthTokenPairDto>> RegisterPatient(PatientCredentialsDto patientCredentials)
    {
        var userExists = await _userManager.FindByEmailAsync(patientCredentials.Email);
        if (userExists != null)
            return BadRequest();

        IdentityUser<Guid> user = new()
        {
            Email = patientCredentials.Email,
            SecurityStamp = Guid.NewGuid().ToString(),
            UserName = patientCredentials.Email
        };

        var signUpResult = await _userManager.CreateAsync(user, patientCredentials.Password);
        if (!signUpResult.Succeeded)
        {
            foreach (var error in signUpResult.Errors)
            {
                ModelState.TryAddModelError(error.Code, error.Description);
            }

            return BadRequest(ModelState);
        }

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

    [HttpPost(EndpointRoutes.SignInEndpointRoute)]
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

        return BadRequest(ApiMessages.FailedLoginMessage);
    }

    [HttpPost(EndpointRoutes.SignOutEndpointRoute)]
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
            return BadRequest(e.Message);
        }
        catch (Exception)
        {
            return BadRequest();
        }

        return Ok();
    }


    [HttpPost(EndpointRoutes.RefreshTokenEndpointRoute)]
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
            _tokenRevoker.RevokeAllUserTokens(userId);
            return BadRequest();
        }
    }

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