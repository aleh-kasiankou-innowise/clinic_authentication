using System.IdentityModel.Tokens.Jwt;
using Innowise.Clinic.Auth.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Innowise.Clinic.Auth.Controllers;

[ApiController]
[Route("auth")]
public class AuthenticationController : ControllerBase
{
    private readonly UserManager<IdentityUser<Guid>> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly ITokenGenerator _tokenGenerator;

    public AuthenticationController(UserManager<IdentityUser<Guid>> userManager,
        RoleManager<IdentityRole<Guid>> roleManager, ITokenGenerator tokenGenerator)
    {
        _userManager = userManager;
        _roleManager = roleManager;
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

        var signInResult = await _userManager.CheckPasswordAsync(user, patientCredentials.Password);

        if (!signInResult)
        {
            return Unauthorized();
        }

        var token = _tokenGenerator.GenerateToken(User.Claims);
        var tokenJson = new JwtSecurityTokenHandler().WriteToken(token);

        return Ok(tokenJson);
    }
}