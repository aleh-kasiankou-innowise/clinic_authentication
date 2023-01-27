using System.Security.Claims;
using Innowise.Clinic.Auth.Constants;
using Innowise.Clinic.Auth.Jwt.Interfaces;
using Innowise.Clinic.Auth.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Innowise.Clinic.Auth.Controllers;

[ApiController]
[Route("auth")]
public class AuthenticationController : ControllerBase
{
    private readonly UserManager<IdentityUser<Guid>> _userManager;
    private readonly ITokenGenerator _tokenGenerator;

    public AuthenticationController(UserManager<IdentityUser<Guid>> userManager, ITokenGenerator tokenGenerator)
    {
        _userManager = userManager;
        _tokenGenerator = tokenGenerator;
    }


    [HttpPost("sign-up/patient")]
    public async Task<ActionResult<string>> RegisterPatient(PatientSignUpModel patientCredentials)
    {
        var userExists = await _userManager.FindByEmailAsync(patientCredentials.Email);
        if (userExists != null)
            return BadRequest();

        IdentityUser<Guid> user = new()
        {
            Email = patientCredentials.Email,
            SecurityStamp = Guid.NewGuid().ToString(),
            UserName = patientCredentials.Email
        };
        var signUpResult = await _userManager.CreateAsync(user, patientCredentials.Password);
        if (!signUpResult.Succeeded)
            return BadRequest();

        await _userManager.AddToRoleAsync(user, UserRoles.Patient.ToString());

        var userRoles = await _userManager.GetRolesAsync(user);

        var authClaims = new List<Claim>
        {
            new Claim(ClaimTypes.PrimarySid, user.Id.ToString()),
        };

        foreach (var userRole in userRoles)
        {
            authClaims.Add(new Claim(ClaimTypes.Role, userRole));
        }

        var token = _tokenGenerator.GenerateToken(authClaims);

        return Ok(token);
    }
}