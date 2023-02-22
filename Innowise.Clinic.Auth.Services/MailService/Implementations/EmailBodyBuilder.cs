using System.Text;
using Innowise.Clinic.Auth.Dto;
using Innowise.Clinic.Auth.Services.Constants.Email;

namespace Innowise.Clinic.Auth.Services.MailService.Implementations;

public static class EmailBodyBuilder
{
    public static string BuildBodyForEmailConfirmation(string emailConfirmationLink)
    {
        var stringBuilder = new StringBuilder(EmailTemplates.EmailConfirmation);
        stringBuilder.Replace(EmailVariables.EmailConfirmationLink, emailConfirmationLink);

        return stringBuilder.ToString();
    }

    public static string BuildBodyWithCredentials(UserCredentialsDto userCredentials,
        string userRole)
    {
        var stringBuilder = new StringBuilder(EmailTemplates.EmailWithCredentials);
        stringBuilder.Replace(EmailVariables.EmailAddress, userCredentials.Email);
        stringBuilder.Replace(EmailVariables.Password, userCredentials.Password);
        stringBuilder.Replace(EmailVariables.Role, userRole);
        return stringBuilder.ToString();
    }
}