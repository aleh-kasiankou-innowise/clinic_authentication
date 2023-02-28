using System.Text;
using Innowise.Clinic.Auth.Services.RabbitMqConsumer.Options;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Innowise.Clinic.Auth.Services.RabbitMqConsumer;

public class RabbitMqConsumer
{
    private readonly IModel _channel;
    private readonly RabbitOptions _rabbitOptions;
    private EventingBasicConsumer _doctorDeactivatedConsumer;
    private EventingBasicConsumer _receptionistRemovedConsumer;

    public RabbitMqConsumer(IOptions<RabbitOptions> rabbitConfig)
    {
        _rabbitOptions = rabbitConfig.Value;
        var factory = new ConnectionFactory
            { HostName = _rabbitOptions.HostName, UserName = "guest", Password = "guest" };
        var connection = factory.CreateConnection();
        _channel = connection.CreateModel();
        SubscribeToReceptionistRemovalMessages();
        SubscribeToDoctorDeactivationMessages();
    }

    private void SubscribeToDoctorDeactivationMessages()
    {
        DeclareProfileAuthenticationExchange();
        var queue = CreateAndBindAnonymousQueue(_rabbitOptions.DoctorInactiveRoutingKey);
        _doctorDeactivatedConsumer = new EventingBasicConsumer(_channel);
        _doctorDeactivatedConsumer.Received += (model, ea) =>
        {
            byte[] body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            Console.WriteLine($" [x] {message}");
        };
        _channel.BasicConsume(queue: queue,
            autoAck: true,
            consumer: _doctorDeactivatedConsumer);
    }

    private void SubscribeToReceptionistRemovalMessages()
    {
        DeclareProfileAuthenticationExchange();
        var queue = CreateAndBindAnonymousQueue(_rabbitOptions.ReceptionistRemovedRoutingKey);
        _receptionistRemovedConsumer = new EventingBasicConsumer(_channel);
        _receptionistRemovedConsumer.Received += (model, ea) =>
        {
            byte[] body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            Console.WriteLine($" [x] {message}");
        };
        _channel.BasicConsume(queue: queue,
            autoAck: true,
            consumer: _receptionistRemovedConsumer);
    }

    private void DeclareProfileAuthenticationExchange()
    {
        _channel.ExchangeDeclare(exchange: _rabbitOptions.ProfilesAuthenticationExchangeName, type: ExchangeType.Topic);
    }

    private string CreateAndBindAnonymousQueue(string routingKey)
    {
        var queueName = _channel.QueueDeclare().QueueName;
        _channel.QueueBind(queue: queueName,
            exchange: _rabbitOptions.ProfilesAuthenticationExchangeName,
            routingKey: routingKey);
        return queueName;
    }
}