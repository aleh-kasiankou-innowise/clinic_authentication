namespace Innowise.Clinic.Auth.Services.Constants.Email;

public static class EmailTemplates
{
    public const string EmailFooter = "<p>Your Clinic - To healthier future together!<p>";
    public const string SiteName = "clinic.com";

    public const string EmailConfirmation =
        "<p>Dear Customer, we've recieved your registration request." +
        " To confirm your email address and proceed with profile creation, please follow the link below:<br>" +
        $"<a href='{EmailVariables.EmailConfirmationLink}'>Confirm Email!</a></p>" +
        "<p>In case you haven't registered an account, please ignore this message.</p>" +
        $"{EmailFooter}";

    public const string EmailWithCredentials =
        $"<p>Dear Customer, the {EmailVariables.Role} account has been created for you at {SiteName}</p>" +
        $"<p>To log in, please use the following credentials:<br><em>Email: {EmailVariables.EmailAddress}</em>" +
        $"<br><em>Password: {EmailVariables.Password}</em></p>" +
        $"{EmailFooter}";
}