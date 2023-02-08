namespace Innowise.Clinic.Auth.UserManagement.Exceptions;

public class UserAlreadyRegisteredException : ApplicationException
{
    private const string DefaultMessage = "The user with the provided email is alredy registered";

    public UserAlreadyRegisteredException() : base(DefaultMessage)
    {
    }
}