using Microsoft.AspNetCore.Mvc;

namespace Innowise.Clinic.Auth.Controllers;

[ApiController]
[Route("auth")]
public class AuthenticationController : ControllerBase
{
   

    private readonly ILogger<AuthenticationController> _logger;

    public AuthenticationController(ILogger<AuthenticationController> logger)
    {
        _logger = logger;
    }
}