using Innowise.Clinic.Auth.Dto;
using Swashbuckle.AspNetCore.Filters;

namespace Innowise.Clinic.Auth.Configuration.Swagger;

public class SignInCredentialsExample : IMultipleExamplesProvider<UserCredentialsDto>
{
    public IEnumerable<SwaggerExample<UserCredentialsDto>> GetExamples()
    {
        yield return SwaggerExample.Create("Patient", new UserCredentialsDto
        {
            Email = "patient@clinic.com",
            Password = "securePassword"
        });
        yield return SwaggerExample.Create("Doctor", new UserCredentialsDto
        {
            Email = "doctor@clinic.com",
            Password = "securePassword"
        });
        yield return SwaggerExample.Create("Receptionist", new UserCredentialsDto
        {
            Email = "receptionist@clinic.com",
            Password = "securePassword"
        });
    }
}