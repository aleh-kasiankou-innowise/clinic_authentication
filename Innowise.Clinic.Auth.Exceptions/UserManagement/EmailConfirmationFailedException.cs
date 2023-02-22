using Innowise.Clinic.Auth.Exceptions.UserManagement.Base;
using Microsoft.AspNetCore.Identity;

namespace Innowise.Clinic.Auth.Exceptions.UserManagement;

public class EmailConfirmationFailedException : AuthenticationModelException
{
    public const string DefaultMessage = "The Email confirmation failed. Please try again later.";

    public EmailConfirmationFailedException(IEnumerable<IdentityError> errors) : base(DefaultMessage, errors)
    {
        StatusCode = 400;
    }
}