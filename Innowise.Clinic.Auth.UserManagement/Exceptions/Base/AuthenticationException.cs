namespace Innowise.Clinic.Auth.UserManagement.Exceptions.Base;

public class AuthenticationException : ApplicationException
{
    public AuthenticationException(string message, int statusCode) : base(message)
    {
        StatusCode = statusCode;
    }

    public int StatusCode { get; }
}