using Microsoft.IdentityModel.Tokens;

namespace Innowise.Clinic.Auth.Jwt.Exceptions;

public class UnknownRefreshTokenException : SecurityTokenValidationException
{
    private const string DefaultMessage = "The provided token is not registered in the system";

    public UnknownRefreshTokenException() : base(DefaultMessage)
    {
    }
}