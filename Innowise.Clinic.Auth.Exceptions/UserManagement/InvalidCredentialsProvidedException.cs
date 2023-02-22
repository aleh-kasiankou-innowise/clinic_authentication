using Innowise.Clinic.Auth.Exceptions.UserManagement.Base;

namespace Innowise.Clinic.Auth.Exceptions.UserManagement;

public class InvalidCredentialsProvidedException : AuthenticationException
{
    public const string DefaultMessage = "Either an email or a password is incorrect";


    public InvalidCredentialsProvidedException(string message) : base(message, StatusCode)
    {
    }

    public InvalidCredentialsProvidedException() : base(DefaultMessage, StatusCode)
    {
    }

    public new static int StatusCode => 401;
}