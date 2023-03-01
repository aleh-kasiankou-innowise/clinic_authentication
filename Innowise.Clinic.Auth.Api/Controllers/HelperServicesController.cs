using System.Security.Claims;
using Innowise.Clinic.Auth.Api.Controllers.Abstractions;
using Innowise.Clinic.Auth.Dto;
using Innowise.Clinic.Auth.Exceptions.UserManagement;
using Innowise.Clinic.Auth.Services.Constants.Jwt;
using Innowise.Clinic.Auth.Services.JwtService.Interfaces;
using Innowise.Clinic.Auth.Validators.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Innowise.Clinic.Auth.Api.Controllers;

public class HelperServicesController : ApiControllerBase
{
    [HttpPost("link-to-profile")]
    public async Task<IActionResult> LinkToProfile([FromBody] UserProfileLinkingDto profileLinkingDto,
        [FromServices] UserManager<IdentityUser<Guid>> userManager)
    {
        var user = await userManager.FindByIdAsync(profileLinkingDto.UserId.ToString()) ??
                   throw new UserNotFoundException();
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