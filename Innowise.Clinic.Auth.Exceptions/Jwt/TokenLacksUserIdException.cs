using Microsoft.IdentityModel.Tokens;

namespace Innowise.Clinic.Auth.Exceptions.Jwt;

public class TokenLacksUserIdException : SecurityTokenValidationException
{
    private const string DefaultMessage = "The provided token lacks user identifier";

    public TokenLacksUserIdException() : base(DefaultMessage)
    {
    }

    public TokenLacksUserIdException(string message) : base(message)
    {
    }
}