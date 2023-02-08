using System.ComponentModel.DataAnnotations;
using Innowise.Clinic.Auth.Constants;
using Innowise.Clinic.Auth.Dto;
using Innowise.Clinic.Auth.UserManagement.interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Innowise.Clinic.Auth.Api.Controllers;

/// <summary>
///     Authentication controller.
///     It registers patients, returns security + refresh tokens for all users who successfully logged in
///     and revokes tokens for whose willing to log out.
/// </summary>
[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public class AuthenticationController : ControllerBase
{
    private readonly IUserManagementService _userManagementService;
    private readonly UserManager<IdentityUser<Guid>> _userManager;


    /// <inheritdoc />
    public AuthenticationController(UserManager<IdentityUser<Guid>> userManager,
        IUserManagementService userManagementService)
    {
        _userManager = userManager;
        _userManagementService = userManagementService;
    }

    /// <summary>Creates account for unregistered patient.</summary>
    /// <param name="patientCredentials">The email and the password that will be used for signing in.</param>
    /// <returns>
    ///     A pair of tokens. A short-living security token and long-living refresh token.
    ///     The latter is used to generate new security token.
    /// </returns>
    /// <response code="200">Success. The user has been registered. A pair of tokens is returned</response>
    /// <response code="400"> Fail. Email and/or password haven't passed validation.</response>
    [HttpPost(EndpointRoutes.SignUpEndpointRoute)]
    [ProducesResponseType(typeof(AuthTokenPairDto), 200)]
    [ProducesResponseType(typeof(void), 400)]
    public async Task<ActionResult<AuthTokenPairDto>> RegisterPatient(PatientCredentialsDto patientCredentials)
    {
        // TODO Move ModelState error reporting to exception handling middleware

        /*if (userRegistrationResult.modelErrors.Any())
        {
            foreach (var error in userRegistrationResult.modelErrors)
                ModelState.TryAddModelError(error.Code, error.Description);

            return BadRequest(ModelState);
        }*/

        return Ok(await _userManagementService.RegisterPatientAsync(patientCredentials));
    }


    /// <summary>Signs in the registered patient.</summary>
    /// <param name="patientCredentials">The email and the password of the patient.</param>
    /// <returns>
    ///     A pair of tokens. A short-living security token and long-living refresh token.
    ///     The latter is used to generate new security token.
    /// </returns>
    /// <response code="200">Success. The user has been registered. A pair of tokens is returned</response>
    /// <response code="401"> Fail. The account with the provided credentials doesn't exist. </response>
    [HttpPost(EndpointRoutes.SignInEndpointRoute)]
    [ProducesResponseType(typeof(AuthTokenPairDto), 200)]
    [ProducesResponseType(typeof(void), 401)]
    public async Task<ActionResult<AuthTokenPairDto>> SignInAsPatient(PatientCredentialsDto patientCredentials)
    {
        return Ok(await _userManagementService.SignInUserAsync(patientCredentials));
    }

    /// <summary>Logs out the user. Revokes the refresh token.</summary>
    /// <param name="tokens">The refresh token and security token issued by the system.</param>
    /// <returns>
    ///     Successful status code (200) if user is logged out.
    /// </returns>
    /// <response code="200">Success. The user is logged out.</response>
    /// <response code="401">Fail. Provided tokens haven't passed validation. It is necessary to sign in again.</response>
    [HttpPost(EndpointRoutes.SignOutEndpointRoute)]
    [ProducesResponseType(typeof(void), 200)]
    [ProducesResponseType(typeof(void), 401)]
    public async Task<IActionResult> SignOutAsPatient(AuthTokenPairDto tokens)
    {
        await _userManagementService.LogOutUserAsync(tokens);

        return Ok();
    }

    /// <summary>Generates a new security token for the user.</summary>
    /// <param name="tokens">The refresh token and security token issued by the system.</param>
    /// <returns>
    ///     New valid security token.
    /// </returns>
    /// <response code="200">Success. A new security token is returned.</response>
    /// <response code="401"> Fail. Provided tokens haven't passed validation. It is necessary to sign in again. </response>
    [HttpPost(EndpointRoutes.RefreshTokenEndpointRoute)]
    [ProducesResponseType(typeof(string), 200)]
    [ProducesResponseType(typeof(void), 401)]
    public async Task<ActionResult<string>> RefreshToken([FromBody] AuthTokenPairDto tokens)
    {
        return Ok(await _userManagementService.RefreshTokenAsync(tokens));
    }

    /// <summary>
    ///     Confirms user email.
    /// </summary>
    /// <param name="emailConfirmationToken">
    ///     Token sent to the email specified by user and encoded in the email confirmation
    ///     link.
    /// </param>
    /// <param name="userId">Id of the user whose email is being confirmed.</param>
    /// <response code="200"> Success. Email is confirmed.</response>
    /// <response code="400"> Fail. Email is not confirmed. Response contains explanation of the issue. </response>
    [ProducesResponseType(typeof(void), 200)]
    [ProducesResponseType(typeof(string), 400)]
    [HttpGet("email/confirm/{emailConfirmationToken:required}")]
    public async Task<IActionResult> ConfirmUserEmail([FromRoute] string emailConfirmationToken,
        [FromQuery] [Required] string userId)
    {
        await _userManagementService.ConfirmUserEmailAsync(userId, emailConfirmationToken);
        return Ok();
    }
}