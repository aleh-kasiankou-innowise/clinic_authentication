namespace Innowise.Clinic.Auth.Exceptions.Testing;

public class TokensNotReturnedException : ApplicationException
{
    private const string DefaultMessage = "The tokens are not returned after login.";

    public TokensNotReturnedException() : base(DefaultMessage)
    {
    }
}