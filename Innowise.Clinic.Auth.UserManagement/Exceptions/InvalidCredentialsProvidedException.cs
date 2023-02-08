using Microsoft.AspNetCore.Identity;

namespace Innowise.Clinic.Auth.UserManagement.Exceptions;

public class InvalidCredentialsProvidedException : ApplicationException
{
    private const string DefaultMessage = "The provided credentials are not valid";

    public InvalidCredentialsProvidedException(IEnumerable<IdentityError> modelErrors) : base(DefaultMessage)
    {
        ModelErrors = modelErrors;
    }

    public IEnumerable<IdentityError> ModelErrors { get; }
}