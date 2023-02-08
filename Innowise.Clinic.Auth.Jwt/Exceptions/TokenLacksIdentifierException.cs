using Microsoft.IdentityModel.Tokens;

namespace Innowise.Clinic.Auth.Jwt.Exceptions;

public class TokenLacksTokenIdException : SecurityTokenValidationException
{
    private const string DefaultMessage = "The provided token lacks token identifier";

    public TokenLacksTokenIdException() : base(DefaultMessage)
    {
    }
}