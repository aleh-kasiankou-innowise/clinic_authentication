namespace Innowise.Clinic.Auth.Mail.Constants;

public static class EmailTemplates
{
    public const string EmailFooter = "<p>Your Clinic - To healthier future together!<p>";

    public const string EmailConfirmation =
        $"<p>Dear Customer, we've recieved your registration request." +
        $" To confirm your email address and proceed with profile creation, please follow the link below:<br>" +
        $"<a href='{EmailVariables.EmailConfirmationLink}'>Confirm Email!</a></p>" +
        $"<p>In case you haven't registered an account, please ignore this message.</p>" +
        $"{EmailFooter}";
}