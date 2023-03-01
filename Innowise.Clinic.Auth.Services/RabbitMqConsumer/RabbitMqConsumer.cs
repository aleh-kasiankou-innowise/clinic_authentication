using System.Text;
using System.Text.Json;
using Innowise.Clinic.Auth.Dto.RabbitMq;
using Innowise.Clinic.Auth.Services.RabbitMqConsumer.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Innowise.Clinic.Auth.Services.RabbitMqConsumer;

public class RabbitMqConsumer : BackgroundService
{
    private readonly IModel _channel;
    private readonly IConnection _connection;
    private readonly RabbitOptions _rabbitOptions;

    private EventingBasicConsumer _doctorDeactivatedConsumer;
    private EventingBasicConsumer _receptionistRemovedConsumer;

    public RabbitMqConsumer(IOptions<RabbitOptions> rabbitConfig, IServiceProvider services)
    {
        _rabbitOptions = rabbitConfig.Value;
        var factory = new ConnectionFactory
        {
            HostName = _rabbitOptions.HostName, UserName = _rabbitOptions.UserName, Password = _rabbitOptions.Password
        };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
    }

    public override void Dispose()
    {
        _channel.Close();
        _connection.Close();
        base.Dispose();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.ThrowIfCancellationRequested();
        SubscribeToReceptionistRemovalMessages();
        SubscribeToDoctorDeactivationMessages();
    }

    private void SubscribeToDoctorDeactivationMessages()
    {
        DeclareProfileAuthenticationExchange();
        var queue = CreateAndBindAnonymousQueue(_rabbitOptions.DoctorInactiveRoutingKey);
        _doctorDeactivatedConsumer = new EventingBasicConsumer(_channel);
        _doctorDeactivatedConsumer.Received += HandleDoctorAccountStatusChange;
        _channel.BasicConsume(queue: queue,
            autoAck: true,
            consumer: _doctorDeactivatedConsumer);
    }

    private void HandleDoctorAccountStatusChange(object? model, BasicDeliverEventArgs ea)
    {
        var body = ea.Body.ToArray();
        var message = Encoding.UTF8.GetString(body);
        var obj = JsonSerializer.Deserialize<AccountStatusChangeDto>(message) ??
                  throw new NotImplementedException();
        Console.WriteLine($" [x] {obj.AccountId} - {obj.IsActiveStatus} ");
    }

    private void SubscribeToReceptionistRemovalMessages()
    {
        DeclareProfileAuthenticationExchange();
        var queue = CreateAndBindAnonymousQueue(_rabbitOptions.ReceptionistRemovedRoutingKey);
        _receptionistRemovedConsumer = new EventingBasicConsumer(_channel);
        _receptionistRemovedConsumer.Received += HandleReceptionistRemoval;
        _channel.BasicConsume(queue: queue,
            autoAck: true,
            consumer: _receptionistRemovedConsumer);
    }

    private void HandleReceptionistRemoval(object? model, BasicDeliverEventArgs ea)
    {
        var body = ea.Body.ToArray();
        var message = new Guid(body);
        Console.WriteLine($" [x] {message}");
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