using Innowise.Clinic.Auth.Services.UserManagementService.Interfaces;
using Innowise.Clinic.Shared.MassTransit.MessageTypes.Events;
using MassTransit;

namespace Innowise.Clinic.Auth.Services.MassTransitService.Consumers;

public class PatientProfileCreatedMessageConsumer : IConsumer<PatientCreatedProfileMessage>
{
    private readonly IUserManagementService _userManagementService;

    public PatientProfileCreatedMessageConsumer(IUserManagementService userManagementService)
    {
        _userManagementService = userManagementService;
    }

    public async Task Consume(ConsumeContext<PatientCreatedProfileMessage> context)
    {
        await _userManagementService.LinkAccountToProfile(context.Message);
        // TODO SEND RESPONSE BECAUSE IF LINKING FAILS THE USER WON'T BE ABLE TO LOG IN 
    }
}