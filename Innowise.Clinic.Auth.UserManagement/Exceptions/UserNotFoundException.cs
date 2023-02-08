namespace Innowise.Clinic.Auth.UserManagement.Exceptions;

public class UserNotFoundException : ApplicationException
{
    private const string DefaultMessage = "The user with provided email is not registered.";

    public UserNotFoundException() : base(DefaultMessage)
    {
    }

    public UserNotFoundException(string message) : base(message)
    {
    }
}