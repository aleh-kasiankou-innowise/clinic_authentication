using Microsoft.IdentityModel.Tokens;

namespace Innowise.Clinic.Auth.Exceptions.Jwt;

public class TokenLacksTokenIdException : SecurityTokenValidationException
{
    private const string DefaultMessage = "The provided token lacks token identifier";

    public TokenLacksTokenIdException() : base(DefaultMessage)
    {
    }

    public TokenLacksTokenIdException(string message) : base(message)
    {
    }
}