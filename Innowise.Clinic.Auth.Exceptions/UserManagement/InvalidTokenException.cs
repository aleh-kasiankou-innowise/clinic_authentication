using Innowise.Clinic.Auth.Exceptions.UserManagement.Base;

namespace Innowise.Clinic.Auth.Exceptions.UserManagement;

public class InvalidTokenException : AuthenticationException
{
    private const string DefaultMessage = "Invalid JWT detected.";

    public InvalidTokenException() : base(DefaultMessage)
    {
        StatusCode = 401;
    }

    public InvalidTokenException(string message) : base(message)
    {
        StatusCode = 401;
    }
}