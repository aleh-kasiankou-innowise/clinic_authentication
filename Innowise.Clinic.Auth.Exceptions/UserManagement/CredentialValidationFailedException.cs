using Innowise.Clinic.Auth.Exceptions.UserManagement.Base;
using Microsoft.AspNetCore.Identity;

namespace Innowise.Clinic.Auth.Exceptions.UserManagement;

public class CredentialValidationFailedException : AuthenticationModelException
{
    private const string DefaultMessage = "The provided credentials are not valid";

    public CredentialValidationFailedException(IEnumerable<IdentityError> modelErrors) : base(
        DefaultMessage, StatusCode, modelErrors)
    {
    }

    public new static int StatusCode => 400;
}