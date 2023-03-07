using Microsoft.IdentityModel.Tokens;

namespace Innowise.Clinic.Auth.Exceptions.Jwt;

public class JwtTokenNotExpiredException : SecurityTokenValidationException
{
    private const string DefaultMessage = "The provided token has not expired yet";

    public JwtTokenNotExpiredException() : base(DefaultMessage)
    {
    }

    public JwtTokenNotExpiredException(string message) : base(message)
    {
    }
}