using Microsoft.AspNetCore.Mvc;

namespace Innowise.Clinic.Auth.Controllers;

[ApiController]
[Route("auth/validation")]
public class ValidationController : ControllerBase
{
    [HttpGet("email/{email:alpha}")]
    public IActionResult CheckEmailUniqueness([FromRoute] string email)
    {
        // if valid return Ok()

        return BadRequest();
    }
}