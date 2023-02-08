namespace Innowise.Clinic.Auth.UserManagement.Exceptions;

public class InvalidTokenException : ApplicationException
{
    private const string DefaultMessage = "Invalid JWT detected.";

    public InvalidTokenException() : base(DefaultMessage)
    {
    }

    public InvalidTokenException(string message) : base(message)
    {
    }
}