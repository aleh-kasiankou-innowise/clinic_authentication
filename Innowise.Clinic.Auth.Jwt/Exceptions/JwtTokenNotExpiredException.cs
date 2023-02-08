using Microsoft.IdentityModel.Tokens;

namespace Innowise.Clinic.Auth.Jwt.Exceptions;

public class JwtTokenNotExpiredException : SecurityTokenValidationException
{
    private const string DefaultMessage = "The provided token has not expired yet";

    public JwtTokenNotExpiredException() : base(DefaultMessage)
    {
    }
}