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
    private readonly SignInManager<IdentityUser<Guid>> _signInManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly ITokenGenerator _tokenGenerator;

    public AuthenticationController(UserManager<IdentityUser<Guid>> userManager,
        RoleManager<IdentityRole<Guid>> roleManager, ITokenGenerator tokenGenerator,
        SignInManager<IdentityUser<Guid>> signInManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _tokenGenerator = tokenGenerator;
        _signInManager = signInManager;
    }


    [HttpPost("sign-up/patient")]
    public async Task<ActionResult<string>> RegisterPatient(PatientSignUpModel patientSignUpModel)
    {
        var userExists = await _userManager.FindByEmailAsync(patientSignUpModel.Email);
        if (userExists != null)
            return BadRequest();

        IdentityUser<Guid> user = new()
        {
            Email = patientSignUpModel.Email,
            SecurityStamp = Guid.NewGuid().ToString(),
            UserName = patientSignUpModel.Email
        };
        var signUpResult = await _userManager.CreateAsync(user, patientSignUpModel.Password);
        if (!signUpResult.Succeeded)
            return BadRequest();

        var signInResult =
            await _signInManager
                .PasswordSignInAsync(patientSignUpModel.Email, patientSignUpModel.Password, true,
                false);

        var token = _tokenGenerator.GenerateToken(User.Claims);
        var tokenJson = new JwtSecurityTokenHandler().WriteToken(token);

        return Ok(tokenJson);
    }
}