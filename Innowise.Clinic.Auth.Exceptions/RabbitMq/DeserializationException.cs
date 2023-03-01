namespace Innowise.Clinic.Auth.Exceptions.RabbitMq;

public class DeserializationException : ApplicationException
{
    public DeserializationException(string message) : base(message)
    {
    }
}