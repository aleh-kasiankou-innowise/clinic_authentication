using Microsoft.AspNetCore.Identity;

namespace Innowise.Clinic.Auth.UserManagement.Exceptions.Base;

public class AuthenticationModelException : ApplicationException
{
    public AuthenticationModelException(string message, int statusCode, IEnumerable<IdentityError> errors) :
        base(message)
    {
        StatusCode = statusCode;
        Errors = errors;
    }

    public int StatusCode { get; }
    public IEnumerable<IdentityError> Errors { get; }
}