namespace Innowise.Clinic.Auth.Exceptions.UserManagement;

public class EmailNotConfirmedException : ApplicationException
{
    public EmailNotConfirmedException(string message) : base(message)
    {
    }
}