using Innowise.Clinic.Auth.Dto;
using Innowise.Clinic.Auth.Services.Constants.Email;
using Innowise.Clinic.Auth.Services.MailService.Data;
using Innowise.Clinic.Auth.Services.MailService.Interfaces;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Innowise.Clinic.Auth.Services.MailService.Implementations;

public class EmailHandler : IEmailHandler
{
    private readonly IOptions<SmtpSettings> _smtpSettings;

    public EmailHandler(IOptions<SmtpSettings> smtpSettings)
    {
        _smtpSettings = smtpSettings;
    }

    public async Task SendMessageAsync(string mailRecipientMailAddress, string mailSubject, string mailBody)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_smtpSettings.Value.AuthenticationEmailSenderName,
            _smtpSettings.Value.AuthenticationEmailSenderAddress));
        message.To.Add(new MailboxAddress(mailRecipientMailAddress, mailRecipientMailAddress));
        message.Subject = mailSubject;
        message.Body = new TextPart("html")
        {
            Text = mailBody
        };

        using (var client = new SmtpClient())
        {
            await client.ConnectAsync(_smtpSettings.Value.SmtpServerHost, _smtpSettings.Value.SmtpServerPort, false);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }

    public async Task SendEmailConfirmationLinkAsync(string mailRecipient, string emailConfirmationLink)
    {
        var emailBody = EmailBodyBuilder.BuildBodyForEmailConfirmation(emailConfirmationLink);
        await SendMessageAsync(mailRecipient, EmailSubjects.EmailConfirmation, emailBody);
    }

    public async Task SendEmailWithCredentialsAsync(UserCredentialsDto credentials, string role)
    {
        var emailBody = EmailBodyBuilder.BuildBodyWithCredentials(credentials, role);
        await SendMessageAsync(credentials.Email, EmailSubjects.AdminProfileRegistration, emailBody);
    }
}