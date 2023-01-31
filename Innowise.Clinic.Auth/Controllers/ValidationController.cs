using Innowise.Clinic.Auth.Validators.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Innowise.Clinic.Auth.Controllers;

[ApiController]
[Route("auth/validation")]
public class ValidationController : ControllerBase
{
    private readonly IEmailValidator _emailValidator;

    public ValidationController(IEmailValidator emailValidator)
    {
        _emailValidator = emailValidator;
    }

    [HttpGet("email/{email:alpha}")]
    public async Task<IActionResult> CheckEmailUniqueness([FromRoute] string email)
    {
        var validationSucceeded = await _emailValidator.ValidateEmailAsync(email);

        return validationSucceeded? Ok() : BadRequest();
    }
}