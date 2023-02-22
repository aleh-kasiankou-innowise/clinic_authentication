using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace Innowise.Clinic.Auth.Validators.Identity;

public class MaximalPasswordLengthValidator<TUser> : IPasswordValidator<TUser>
    where TUser : IdentityUser<Guid>
{
    private readonly int _maximalPasswordLength;

    public MaximalPasswordLengthValidator(IConfiguration configuration)
    {
        var validationParams = configuration.GetSection("AuthenticationRequirements");
        _maximalPasswordLength = Convert.ToInt32(validationParams["MaximalPasswordLength"]);
    }

    public async Task<IdentityResult> ValidateAsync(UserManager<TUser> manager, TUser user, string password)
    {
        var passLength = password.Length;
        if (passLength > _maximalPasswordLength)
            return await Task.FromResult(IdentityResult.Failed(new IdentityError
            {
                Code = "PasswordLength",
                Description = $"The password should contain 6 to 15 symbols. Current length: {passLength}"
            }));

        return await Task.FromResult(IdentityResult.Success);
    }
}