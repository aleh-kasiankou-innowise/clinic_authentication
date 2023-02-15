using System.Security.Cryptography;
using Innowise.Clinic.Auth.Dto;
using Innowise.Clinic.Auth.Services.UserCredentialsGenerationService.Interfaces;

namespace Innowise.Clinic.Auth.Services.UserCredentialsGenerationService.Implementations;

public class UserCredentialsGenerationService : IUserCredentialsGenerationService
{
    public UserCredentialsDto GenerateCredentials(int maxPasswordLength, string emailAddress)
    {
        const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*()_+";

        var byteBuffer = RandomNumberGenerator.GetBytes(maxPasswordLength);
        {
            var charBuffer = new char[maxPasswordLength];

            for (var i = 0; i < maxPasswordLength; i++) charBuffer[i] = validChars[byteBuffer[i] % validChars.Length];

            return new UserCredentialsDto
            {
                Email = emailAddress,
                Password = new string(charBuffer)
            };
        }
    }
}