namespace Innowise.Clinic.Auth.Mail.Interfaces;

public interface IEmailHandler
{
    Task SendMessageAsync(string mailRecipient, string mailSubject, string mailBody);

    Task SendEmailConfirmationLinkAsync(string mailRecipient, string emailConfirmationLink);
}