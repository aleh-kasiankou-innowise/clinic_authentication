using Innowise.Clinic.Auth.Dto;

namespace Innowise.Clinic.Auth.Services.MailService.Interfaces;

public interface IEmailHandler
{
    Task SendMessageAsync(string mailRecipient, string mailSubject, string mailBody);
    Task SendEmailConfirmationLinkAsync(string mailRecipient, string emailConfirmationLink);
    Task SendEmailWithCredentialsAsync(UserCredentialsDto credentials, string role);
}