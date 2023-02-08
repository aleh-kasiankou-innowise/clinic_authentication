using Innowise.Clinic.Auth.UserManagement.Exceptions.Base;
using Microsoft.AspNetCore.Identity;

namespace Innowise.Clinic.Auth.UserManagement.Exceptions;

public class EmailConfirmationFailedException : AuthenticationModelException
{
    public const string DefaultMessage = "The Email confirmation failed. Please try again later.";

    public EmailConfirmationFailedException(IEnumerable<IdentityError> errors) : base(DefaultMessage, StatusCode,
        errors)
    {
    }

    public new static int StatusCode => 400;
}