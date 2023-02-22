using Microsoft.AspNetCore.Identity;

namespace Innowise.Clinic.Auth.Exceptions.UserManagement.Base;

public class AuthenticationModelException : ApplicationException
{
    public AuthenticationModelException(string message, IEnumerable<IdentityError> errors) :
        base(message)
    {
        Errors = errors;
    }

    public int StatusCode { get; set; }
    public IEnumerable<IdentityError> Errors { get; }
}