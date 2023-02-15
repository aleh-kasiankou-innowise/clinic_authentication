using System.Text;
using Innowise.Clinic.Auth.Services.Constants;

namespace Innowise.Clinic.Auth.Services.MailService.Implementations;

public static class EmailBodyBuilder
{
    public static string BuildBodyForEmailConfirmation(string emailConfirmationLink)
    {
        var stringBuilder = new StringBuilder(EmailTemplates.EmailConfirmation);
        stringBuilder.Replace(EmailVariables.EmailConfirmationLink, emailConfirmationLink);

        return stringBuilder.ToString();
    }
}