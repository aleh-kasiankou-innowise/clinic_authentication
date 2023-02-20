using System.Security.Claims;
using Innowise.Clinic.Auth.Dto;
using Innowise.Clinic.Auth.Services.Constants.Jwt;
using Innowise.Clinic.Auth.Services.JwtService.Data;
using Innowise.Clinic.Auth.Services.JwtService.Interfaces;
using Innowise.Clinic.Auth.Services.UserCredentialsGenerationService.Interfaces;
using Innowise.Clinic.Auth.Services.UserManagementService.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Innowise.Clinic.Auth.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class HelperServicesController : ControllerBase
{
    private readonly IUserCredentialsGenerationService _passwordGenerator;
    private readonly IUserManagementService _userManagementService;
    private readonly JwtValidationSettings _validationSettings;

    // generate credentials, send confirmation mail, add claims for limiting access

    // doctors can only interact with their own profiles
    // patients can only interact with their own profiles 
    // receptionists can interact with any profiles

    public HelperServicesController(IUserManagementService userManagementService,
        IUserCredentialsGenerationService passwordGenerator, IOptions<JwtValidationSettings> validationSettings)
    {
        _userManagementService = userManagementService;
        _passwordGenerator = passwordGenerator;
        _validationSettings = validationSettings.Value;
    }

    [HttpPost("generate-credentials")]
    public async Task<IActionResult> CreateUser([FromBody] UserCreationRequestDto userCreationRequest)
    {
        var userCredentials =
            _passwordGenerator.GenerateCredentials(_validationSettings.MaximalPasswordLength,
                userCreationRequest.Email);

        await _userManagementService.RegisterConfirmedUserAsync(userCredentials, userCreationRequest);

        return Ok();
    }

    [HttpPost("link-to-profile")]
    public async Task<IActionResult> LinkToProfile([FromBody] UserProfileLinkingDto profileLinkingDto,
        [FromServices] UserManager<IdentityUser<Guid>> userManager)
    {
        var user = await userManager.FindByIdAsync(profileLinkingDto.UserId.ToString());
        if (user == null) throw new NotImplementedException();

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
}