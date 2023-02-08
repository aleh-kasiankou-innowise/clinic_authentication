namespace Innowise.Clinic.Auth.UserManagement.Exceptions;

public class EmailConfirmationFailedException : ApplicationException
{
    private const string DefaultMessage = "The Email confirmation failed. Please try again later.";

    public EmailConfirmationFailedException() : base(DefaultMessage)
    {
    }
}