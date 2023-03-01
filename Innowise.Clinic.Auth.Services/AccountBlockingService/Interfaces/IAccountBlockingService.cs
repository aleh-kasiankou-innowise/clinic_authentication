namespace Innowise.Clinic.Auth.Services.AccountBlockingService.Interfaces;

public interface IAccountBlockingService
{
    Task SetAccountStatus(Guid accountId, bool shouldAccountBeActive);
    Task<bool> IsAccountActive(Guid accountId);
}