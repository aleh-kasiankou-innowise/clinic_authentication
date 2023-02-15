using Microsoft.IdentityModel.Tokens;

namespace Innowise.Clinic.Auth.Exceptions.Jwt;

public class PrincipalLacksTokenIdException : SecurityTokenValidationException
{
    private const string DefaultMessage = "The principal lacks TokenID";

    public PrincipalLacksTokenIdException() : base(DefaultMessage)
    {
    }

    public PrincipalLacksTokenIdException(string message) : base(message)
    {
    }
}