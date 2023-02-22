using Innowise.Clinic.Auth.Validators.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Innowise.Clinic.Auth.Api.Controllers;

/// <summary>
///     Validation controller.
///     It checks whether the provided email is already registered in the system.
/// </summary>
[ApiController]
[Route("[controller]")]
public class ValidationController : ControllerBase
{
    private const string EmailIsRegisteredMessage =
        "The account with the provided email is already registered in the system";

    private readonly IEmailValidator _emailValidator;
    private readonly ITokenStateValidator _tokenStateValidator;

    /// <inheritdoc />
    public ValidationController(IEmailValidator emailValidator, ITokenStateValidator tokenStateValidator)
    {
        _emailValidator = emailValidator;
        _tokenStateValidator = tokenStateValidator;
    }

    /// <summary>Checks whether the provided email is already registered in the system.</summary>
    /// <param name="email">The email to check for uniqueness.</param>
    /// <returns>
    ///     Successful status code (200) if the provided email is unique.
    /// </returns>
    /// <response code="200">Success. The email is not registered in the system.</response>
    /// <response code="400"> Fail. The account with the provided email is already registered in the system.</response>
    [HttpGet("email/{email:required}")]
    [ProducesResponseType(typeof(void), 200)]
    [ProducesResponseType(typeof(void), 400)]
    public async Task<IActionResult> CheckEmailUniqueness([FromRoute] string email)
    {
        var isValidationSucceeded = await _emailValidator.ValidateEmailAsync(email);

        return isValidationSucceeded
            ? Ok()
            : BadRequest(EmailIsRegisteredMessage);
    }

    /// <summary>Checks whether the user tokens are revoked.</summary>
    /// <param name="id">The id of the user who is to be checked.</param>
    /// <returns>
    ///     Successful status code (200) if the provided email is unique.
    /// </returns>
    /// <response code="200">Success. The validation result is returned.</response>
    [HttpGet("user-tokens/{id:guid}")]
    [ProducesResponseType(typeof(void), 200)]
    [ProducesResponseType(typeof(void), 400)]
    public async Task<bool> CheckIfTokenRevoked([FromRoute] Guid id)
    {
        return await _tokenStateValidator.IsTokenStateValid(id);
    }
}