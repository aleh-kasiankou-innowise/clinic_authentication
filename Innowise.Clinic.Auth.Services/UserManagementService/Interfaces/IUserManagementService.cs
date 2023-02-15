using Innowise.Clinic.Auth.Dto;

namespace Innowise.Clinic.Auth.Services.UserManagementService.Interfaces;

public interface IUserManagementService
{
    Task<AuthTokenPairDto> RegisterPatientAsync(UserCredentialsDto patientCredentials);

    Task<AuthTokenPairDto> SignInUserAsync(UserCredentialsDto patientCredentialsDto);

    Task LogOutUserAsync(AuthTokenPairDto userTokens);

    Task<string> RefreshTokenAsync(AuthTokenPairDto userTokens);

    Task ConfirmUserEmailAsync(string userId, string emailToken);
}