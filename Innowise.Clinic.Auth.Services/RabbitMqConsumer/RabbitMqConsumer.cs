using System.Text;
using System.Text.Json;
using Innowise.Clinic.Auth.Dto.RabbitMq;
using Innowise.Clinic.Auth.Exceptions.RabbitMq;
using Innowise.Clinic.Auth.Exceptions.UserManagement;
using Innowise.Clinic.Auth.Services.AccountBlockingService.Interfaces;
using Innowise.Clinic.Auth.Services.RabbitMqConsumer.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Innowise.Clinic.Auth.Services.RabbitMqConsumer;

public class RabbitMqConsumer : BackgroundService
{
    private readonly IConnection _connection;
    private readonly IModel _doctorStatusUpdateChannel;
    private readonly RabbitOptions _rabbitOptions;
    private readonly IModel _receptionistRemoveChannel;
    private readonly IServiceProvider _services;

    private EventingBasicConsumer _doctorDeactivatedConsumer;
    private EventingBasicConsumer _receptionistRemovedConsumer;

    public RabbitMqConsumer(IOptions<RabbitOptions> rabbitConfig, IServiceProvider services)
    {
        _services = services;
        _rabbitOptions = rabbitConfig.Value;
        var factory = new ConnectionFactory
        {
            HostName = _rabbitOptions.HostName, UserName = _rabbitOptions.UserName, Password = _rabbitOptions.Password
        };
        _connection = factory.CreateConnection();
        _doctorStatusUpdateChannel = _connection.CreateModel();
        _receptionistRemoveChannel = _connection.CreateModel();
    }

    public override void Dispose()
    {
        _doctorStatusUpdateChannel.Close();
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
        _doctorDeactivatedConsumer = new EventingBasicConsumer(_doctorStatusUpdateChannel);
        _doctorDeactivatedConsumer.Received += HandleDoctorAccountStatusChange;
        _doctorStatusUpdateChannel.BasicConsume(queue: queue,
            autoAck: true,
            consumer: _doctorDeactivatedConsumer);
    }

    private void HandleDoctorAccountStatusChange(object? model, BasicDeliverEventArgs ea)
    {
        try
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var statusChangeDto = JsonSerializer.Deserialize<AccountStatusChangeDto>(message) ??
                                  throw new DeserializationException(
                                      "The object received is not of AccountStatusChangeDto type.");
            using var scope = _services.CreateScope();
            var accountBlockingService = scope.ServiceProvider.GetRequiredService<IAccountBlockingService>();
            accountBlockingService.SetAccountStatus(statusChangeDto.AccountId, statusChangeDto.IsActiveStatus).Wait();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private void SubscribeToReceptionistRemovalMessages()
    {
        DeclareProfileAuthenticationExchange();
        var queue = CreateAndBindAnonymousQueue(_rabbitOptions.ReceptionistRemovedRoutingKey);
        _receptionistRemovedConsumer = new EventingBasicConsumer(_receptionistRemoveChannel);
        _receptionistRemovedConsumer.Received += HandleReceptionistRemoval;
        _receptionistRemoveChannel.BasicConsume(queue: queue,
            autoAck: true,
            consumer: _receptionistRemovedConsumer);
    }

    private void HandleReceptionistRemoval(object? model, BasicDeliverEventArgs ea)
    {
        try
        {
            var body = ea.Body.ToArray();
            var accountId = new Guid(body);
            using var scope = _services.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser<Guid>>>();
            var accountToRemove = userManager.FindByIdAsync(accountId.ToString()).Result ??
                                  throw new UserNotFoundException();
            _ = userManager.DeleteAsync(accountToRemove).Result;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private void DeclareProfileAuthenticationExchange()
    {
        _doctorStatusUpdateChannel.ExchangeDeclare(exchange: _rabbitOptions.ProfilesAuthenticationExchangeName,
            type: ExchangeType.Topic);
    }

    private string CreateAndBindAnonymousQueue(string routingKey)
    {
        var queueName = _doctorStatusUpdateChannel.QueueDeclare().QueueName;
        _doctorStatusUpdateChannel.QueueBind(queue: queueName,
            exchange: _rabbitOptions.ProfilesAuthenticationExchangeName,
            routingKey: routingKey);
        return queueName;
    }
}