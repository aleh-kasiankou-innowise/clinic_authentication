using Innowise.Clinic.Auth.UserManagement.Exceptions.Base;

namespace Innowise.Clinic.Auth.UserManagement.Exceptions;

public class UserAlreadyRegisteredException : AuthenticationException
{
    private const string DefaultMessage = "The user with the provided email is already registered";

    public UserAlreadyRegisteredException() : base(DefaultMessage, StatusCode)
    {
    }

    public new static int StatusCode => 400;
}