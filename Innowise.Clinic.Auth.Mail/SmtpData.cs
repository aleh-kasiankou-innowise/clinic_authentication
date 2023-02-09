namespace Innowise.Clinic.Auth.Mail;

public record SmtpSettings
{
    public string SmtpServerHost { get; set; }
    public int SmtpServerPort { get; set; }
    public string AuthenticationEmailSenderName { get; set; }
    public string AuthenticationEmailSenderAddress { get; set; }
}