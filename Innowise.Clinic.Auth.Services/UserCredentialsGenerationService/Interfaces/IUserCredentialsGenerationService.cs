using Innowise.Clinic.Auth.Dto;

namespace Innowise.Clinic.Auth.Services.UserCredentialsGenerationService.Interfaces;

public interface IUserCredentialsGenerationService
{
    UserCredentialsDto GenerateCredentials(int maxPasswordLength, string emailAddress);
}