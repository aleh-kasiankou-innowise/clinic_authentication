namespace Innowise.Clinic.Auth.Services.Constants;

public static class ServicesRoutes
{
    public static readonly string GatewayUrl = Environment.GetEnvironmentVariable("GATEWAY_URL") ?? throw new
        InvalidOperationException();

    private static readonly string EmailConfirmationPublicUrl = "authentication/email/confirm/";

    public static string BuildEmailConfirmationLink(string tokenEncoded, string userId)
    {
        return $"{GatewayUrl}{EmailConfirmationPublicUrl}{tokenEncoded}?userid={userId}";
    }
}