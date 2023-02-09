using System.Text;
using Innowise.Clinic.Auth.Mail.Constants;

namespace Innowise.Clinic.Auth.Mail;

public static class EmailBodyBuilder
{
    public static string BuildBodyForEmailConfirmation(string emailConfirmationLink)
    {
        var stringBuilder = new StringBuilder(EmailTemplates.EmailConfirmation);
        stringBuilder.Replace(EmailVariables.EmailConfirmationLink, emailConfirmationLink);

        return stringBuilder.ToString();
    }
}