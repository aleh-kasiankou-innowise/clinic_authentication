using Innowise.Clinic.Auth.Exceptions.UserManagement.Base;

namespace Innowise.Clinic.Auth.Exceptions.AccountBlockingService;

public class AccountBlockedException : AuthenticationException
{
    private const string DefaultMessage = "The account is blocked. Please contact any receptionist to activate it.";

    public AccountBlockedException() : base(DefaultMessage)
    {
    }

    public AccountBlockedException(string message) : base(message)
    {
    }
}