using System.ComponentModel.DataAnnotations;
using Innowise.Clinic.Auth.Api.Controllers.Abstractions;
using Innowise.Clinic.Auth.Validators.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Innowise.Clinic.Auth.Api.Controllers;

/// <summary>
///     Validation controller.
///     It checks whether the provided email is already registered in the system.
/// </summary>
public class ValidationController : ApiControllerBase
{
    private const string EmailIsRegisteredMessage =
        "The account with the provided email is already registered in the system";

    private readonly IEmailValidator _emailValidator;

    /// <inheritdoc />
    public ValidationController(IEmailValidator emailValidator)
    {
        _emailValidator = emailValidator;
    }

    /// <summary>Checks whether the provided email is already registered in the system.</summary>
    /// <param name="email">The email to check for uniqueness.</param>
    /// <returns>
    ///     Successful status code (200) if the provided email is unique.
    /// </returns>
    /// <response code="200">Success. The email is not registered in the system.</response>
    /// <response code="400"> Fail. The account with the provided email is already registered in the system.</response>
    [HttpPost("email")]
    [ProducesResponseType(typeof(void), 200)]
    [ProducesResponseType(typeof(void), 400)]
    public async Task<IActionResult> CheckEmailUniqueness([FromBody] [EmailAddress] string email)
    {
        var isValidationSucceeded = await _emailValidator.ValidateEmailAsync(email);

        return isValidationSucceeded
            ? Ok()
            : BadRequest(EmailIsRegisteredMessage);
    }
}