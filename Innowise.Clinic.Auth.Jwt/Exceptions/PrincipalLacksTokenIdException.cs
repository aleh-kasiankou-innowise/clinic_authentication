using Microsoft.IdentityModel.Tokens;

namespace Innowise.Clinic.Auth.Jwt.Exceptions;

public class PrincipalLacksTokenIdException : SecurityTokenValidationException
{
    private const string DefaultMessage = "The principal lacks TokenID";

    public PrincipalLacksTokenIdException() : base(DefaultMessage)
    {
    }
}