using Microsoft.IdentityModel.Tokens;

namespace Innowise.Clinic.Auth.Exceptions.Jwt;

public class InvalidTokenException : SecurityTokenValidationException
{
    public InvalidTokenException(string message) : base(message)
    {
    }
}