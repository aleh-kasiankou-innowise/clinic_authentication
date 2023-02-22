namespace Innowise.Clinic.Auth.Exceptions.CrossServiceCommunication;

public class ProfileNotLinkedException : ApplicationException
{
    public const string DefaultException = "The profile hasn't been linked to the created account.";

    public ProfileNotLinkedException() : base(DefaultException)
    {
    }
}