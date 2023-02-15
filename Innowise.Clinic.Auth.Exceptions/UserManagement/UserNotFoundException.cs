using Innowise.Clinic.Auth.Exceptions.UserManagement.Base;

namespace Innowise.Clinic.Auth.Exceptions.UserManagement;

public class UserNotFoundException : AuthenticationException
{
    private const string DefaultMessage = "The user with provided email is not registered.";

    public UserNotFoundException() : base(DefaultMessage, StatusCode)
    {
    }

    public UserNotFoundException(string message) : base(message, StatusCode)
    {
    }

    public new static int StatusCode => 400;
}