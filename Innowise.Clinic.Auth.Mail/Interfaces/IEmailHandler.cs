namespace Innowise.Clinic.Auth.Mail.Interfaces;

public interface IEmailHandler
{
    void SendMessage(string mailRecipient, string mailSubject, string mailBody);
}