using Microsoft.IdentityModel.Tokens;

namespace Innowise.Clinic.Auth.Jwt.Exceptions;

public class InvalidTokenException : SecurityTokenValidationException
{
    public InvalidTokenException(string message) : base(message)
    {
    }
}