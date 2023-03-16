using Innowise.Clinic.Auth.Dto;
using Innowise.Clinic.Shared.Dto;
using Innowise.Clinic.Shared.MassTransit.MessageTypes.Events;

namespace Innowise.Clinic.Auth.Services.UserManagementService.Interfaces;

public interface IUserManagementService
{
    Task RegisterPatientAsync(UserCredentialsDto patientCredentials);
    Task RegisterConfirmedUserAsync(UserCredentialsDto userCredentials, AccountGenerationDto role);
    Task<AuthTokenPairDto> SignInUserAsync(UserCredentialsDto patientCredentialsDto);
    Task LogOutUserAsync(AuthTokenPairDto userTokens);
    Task<string> RefreshTokenAsync(AuthTokenPairDto userTokens);
    Task ConfirmUserEmailAsync(string userId, string emailToken);
    Task LinkAccountToProfile(PatientCreatedProfileMessage profileLinkingDto);
}