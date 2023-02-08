using Innowise.Clinic.Auth.Dto;

namespace Innowise.Clinic.Auth.UserManagement.interfaces;

public interface IUserManagementService
{
    Task<AuthTokenPairDto> RegisterPatientAsync(PatientCredentialsDto patientCredentials);

    Task<AuthTokenPairDto> SignInUserAsync(PatientCredentialsDto patientCredentialsDto);

    Task LogOutUserAsync(AuthTokenPairDto userTokens);

    Task<string> RefreshTokenAsync(AuthTokenPairDto userTokens);

    Task ConfirmUserEmailAsync(string userId, string emailToken);
}