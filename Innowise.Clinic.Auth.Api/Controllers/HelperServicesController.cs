using System.Security.Claims;
using Innowise.Clinic.Auth.Api.Controllers.Abstractions;
using Innowise.Clinic.Auth.Dto;
using Innowise.Clinic.Auth.Exceptions.UserManagement;
using Innowise.Clinic.Auth.Services.Constants.Jwt;
using Innowise.Clinic.Auth.Services.JwtService.Interfaces;
using Innowise.Clinic.Auth.Services.UserCredentialsGenerationService.Interfaces;
using Innowise.Clinic.Auth.Services.UserManagementService.Data;
using Innowise.Clinic.Auth.Services.UserManagementService.Interfaces;
using Innowise.Clinic.Auth.Validators.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Innowise.Clinic.Auth.Api.Controllers;

public class HelperServicesController : ApiControllerBase
{
    private readonly AuthenticationRequirementsSettings _authenticationRequirements;
    private readonly IUserCredentialsGenerationService _passwordGenerator;
    private readonly IUserManagementService _userManagementService;

    public HelperServicesController(IUserManagementService userManagementService,
        IUserCredentialsGenerationService passwordGenerator,
        IOptions<AuthenticationRequirementsSettings> authRequirementsSettings)
    {
        _userManagementService = userManagementService;
        _passwordGenerator = passwordGenerator;
        _authenticationRequirements = authRequirementsSettings.Value;
    }

    [HttpPost("generate-credentials")]
    public async Task<IActionResult> CreateUser([FromBody] UserCreationRequestDto userCreationRequest)
    {
        var userCredentials =
            _passwordGenerator.GenerateCredentials(_authenticationRequirements.MaximalPasswordLength,
                userCreationRequest.Email);

        await _userManagementService.RegisterConfirmedUserAsync(userCredentials, userCreationRequest);
        return Ok();
    }

    [HttpPost("link-to-profile")]
    public async Task<IActionResult> LinkToProfile([FromBody] UserProfileLinkingDto profileLinkingDto,
        [FromServices] UserManager<IdentityUser<Guid>> userManager)
    {
        var user = await userManager.FindByIdAsync(profileLinkingDto.UserId.ToString()) ??
                   throw new UserNotFoundException();
        ;
        await userManager.AddClaimAsync(user,
            new Claim(JwtClaimTypes.CanInteractWithProfileClaim, profileLinkingDto.ProfileId.ToString()));
        return Ok();
    }

    [HttpPost("force-log-out")]
    public async Task<IActionResult> TerminateAllUserSessions([FromBody] Guid userId,
        [FromServices] ITokenRevoker tokenRevoker)
    {
        await tokenRevoker.RevokeAllUserTokensAsync(userId);
        return Ok();
    }

    /// <summary>Checks whether the user tokens are revoked.</summary>
    /// <param name="id">The id of the user who is to be checked.</param>
    /// <returns>
    ///     Successful status code (200) if the provided email is unique.
    /// </returns>
    /// <response code="200">Success. The validation result is returned.</response>
    [HttpPost("user-tokens")]
    [ProducesResponseType(typeof(void), 200)]
    [ProducesResponseType(typeof(void), 400)]
    public async Task<bool> CheckIfTokenRevoked([FromBody] Guid id,
        [FromServices] ITokenStateValidator tokenStateValidator)
    {
        return await tokenStateValidator.IsTokenStateValid(id);
    }
}