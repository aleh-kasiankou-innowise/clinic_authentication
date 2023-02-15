using Innowise.Clinic.Auth.Dto;
using Innowise.Clinic.Auth.Services.MailService.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Innowise.Clinic.Auth.Api.Controllers;

[ApiController]
[Route("{controller}")]
public class HelperServicesController : ControllerBase
{
    private readonly IEmailHandler _emailHandler;

    // generate credentials, send confirmation mail, add claims for limiting access

    // doctors can only interact with their own profiles
    // patients can only interact with their own profiles 
    // receptionists can interact with any profiles

    public HelperServicesController(IEmailHandler emailHandler)
    {
        _emailHandler = emailHandler;
    }

    [HttpPost("generate-credentials")]
    public Guid CreateUser([FromBody] UserCreationRequestDto userCreationRequest)
    {
        throw new NotImplementedException();
    }
}