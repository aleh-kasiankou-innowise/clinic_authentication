using Microsoft.AspNetCore.Identity;

namespace Innowise.Clinic.Auth.Configuration;

public class DataSeeder
{
    private readonly UserManager<IdentityUser<Guid>> _userManager;

    private readonly IEnumerable<IdentityUser<Guid>> _users = new List<IdentityUser<Guid>>
    {
        new()
        {
            UserName = "patient@clinic.com",
            Email = "patient@clinic.com",
            EmailConfirmed = true
        },
        new()
        {
            UserName = "doctor@clinic.com",
            Email = "doctor@clinic.com",
            EmailConfirmed = true
        },
        new()
        {
            UserName = "receptionist@clinic.com",
            Email = "receptionist@clinic.com",
            EmailConfirmed = true
        }
    };

    public DataSeeder(UserManager<IdentityUser<Guid>> userManager)
    {
        _userManager = userManager;
    }

    public async Task SeedUsers()
    {
        foreach (var user in _users)
            if (await _userManager.FindByEmailAsync(user.Email) == null)
            {
                await _userManager.CreateAsync(user, "securePassword");

                var lowerCaseRole = user.Email.Split("@")[0];
                var capitalizedRole = lowerCaseRole[0]
                    .ToString()
                    .ToUpper() + lowerCaseRole.Substring(1);

                var createdUser = await _userManager.FindByEmailAsync(user.Email);
                await _userManager.AddToRoleAsync(createdUser, capitalizedRole);
            }
    }
}