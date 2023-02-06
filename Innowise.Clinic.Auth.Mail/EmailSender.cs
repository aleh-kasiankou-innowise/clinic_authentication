using Innowise.Clinic.Auth.Mail.Interfaces;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Innowise.Clinic.Auth.Mail;

public class EmailSender : IEmailHandler
{
    private readonly IOptions<SmtpData> _smtpData;

    public EmailSender(IOptions<SmtpData> emailData)
    {
        _smtpData = emailData;
    }

    public void SendMessage(string mailRecipientMailAddress, string mailSubject, string mailBody)
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
            client.Connect(_smtpData.Value.SmtpServerHost, _smtpData.Value.SmtpServerPort, false);

            /*// Note: only needed if the SMTP server requires authentication
            client.Authenticate ("joey", "password");*/

            client.Send(message);
            client.Disconnect(true);
        }
    }
}