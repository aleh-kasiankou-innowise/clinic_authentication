namespace Innowise.Clinic.Auth.Exceptions.UserManagement.Base;

public class AuthenticationException : ApplicationException
{
    public AuthenticationException(string message) : base(message)
    {
    }

    public int StatusCode { get; set; }
}