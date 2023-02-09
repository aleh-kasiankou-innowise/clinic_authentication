using Innowise.Clinic.Auth.UserManagement.Exceptions.Base;

namespace Innowise.Clinic.Auth.UserManagement.Exceptions;

public class InvalidTokenException : AuthenticationException
{
    private const string DefaultMessage = "Invalid JWT detected.";

    public InvalidTokenException() : base(DefaultMessage, StatusCode)
    {
    }

    public InvalidTokenException(string message) : base(message, StatusCode)
    {
    }

    public new static int StatusCode => 401;
}