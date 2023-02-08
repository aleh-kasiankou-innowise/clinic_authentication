using Innowise.Clinic.Auth.Mail.Constants;
using Innowise.Clinic.Auth.Mail.Interfaces;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Innowise.Clinic.Auth.Mail;

public class EmailHandler : IEmailHandler
{
    private readonly IOptions<SmtpData> _smtpData;

    public EmailHandler(IOptions<SmtpData> emailData)
    {
        _smtpData = emailData;
    }

    public async Task SendMessageAsync(string mailRecipientMailAddress, string mailSubject, string mailBody)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_smtpData.Value.AuthenticationEmailSenderName,
            _smtpData.Value.AuthenticationEmailSenderAddress));

        message.To.Add(new MailboxAddress(mailRecipientMailAddress, mailRecipientMailAddress));
        message.Subject = mailSubject;

        message.Body = new TextPart("html")
        {
            Text = mailBody
        };

        using (var client = new SmtpClient())
        {
            await client.ConnectAsync(_smtpData.Value.SmtpServerHost, _smtpData.Value.SmtpServerPort, false);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }

    public async Task SendEmailConfirmationLinkAsync(string mailRecipient, string emailConfirmationLink)
    {
        var emailBody = EmailBodyBuilder.BuildBodyForEmailConfirmation(emailConfirmationLink);
        await SendMessageAsync(mailRecipient, EmailSubjects.EmailConfirmation, emailConfirmationLink);
    }
}