using Innowise.Clinic.Auth.Api.Controllers.Abstractions;
using Innowise.Clinic.Auth.Validators.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Innowise.Clinic.Auth.Api.Controllers;

public class HelperServicesController : ApiControllerBase
{
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