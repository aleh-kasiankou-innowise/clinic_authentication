using Microsoft.AspNetCore.Mvc;

namespace Innowise.Clinic.Auth.Api.Controllers.Abstractions;

[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public abstract class ApiControllerBase : ControllerBase
{
}