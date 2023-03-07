using Innowise.Clinic.Auth.Exceptions.UserManagement.Base;

namespace Innowise.Clinic.Auth.Exceptions.UserManagement;

public class UserNotFoundException : AuthenticationException
{
    private const string DefaultMessage = "The requested user is not registered.";

    public UserNotFoundException() : base(DefaultMessage)
    {
        StatusCode = 400;
    }

    public UserNotFoundException(string message) : base(message)
    {
        StatusCode = 400;
    }
}