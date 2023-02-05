using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Innowise.Clinic.Auth.Constants;
using Innowise.Clinic.Auth.Dto;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Innowise.Clinic.Auth.IntegrationTesting;

public class SignInTests : IClassFixture<IntegrationTestingWebApplicationFactory>
{
    private readonly IntegrationTestingWebApplicationFactory _factory;
    private readonly HttpClient _httpClient;

    public SignInTests(IntegrationTestingWebApplicationFactory factory)
    {
        _factory = factory;
        _httpClient = factory.CreateClient(new WebApplicationFactoryClientOptions()
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task TestRegisteredPatientSignsInWithValidCredentials_OK()
    {
        // Arrange

        var validUserRegistrationData = new PatientCredentialsDto()
        {
            Email = $"test{TestHelper.UniqueNumber}@test.com",
            Password = "12345678"
        };

        await _httpClient.PostAsJsonAsync(TestHelper.SignUpEndpointUri, validUserRegistrationData);

        // Act

        var response = await _httpClient.PostAsJsonAsync(TestHelper.SignInEndpointUri, validUserRegistrationData);


        // Assert

        Assert.True(response.IsSuccessStatusCode);
        var generatedTokens = await response.Content.ReadFromJsonAsync<AuthTokenPairDto>();


        Assert.NotNull(generatedTokens);
        Assert.NotNull(generatedTokens.JwtToken);
        Assert.NotNull(generatedTokens.RefreshToken);

        var userId = TestHelper.ExtractUserIdFromJwtToken(generatedTokens.JwtToken);
        Assert.NotNull(_factory.UseDbContext(x =>
            x.Users.SingleOrDefaultAsync(u => u.Id == userId && u.Email == validUserRegistrationData.Email)));
    }

    [Fact]
    public async Task TestRegisteredPatientSignsInWithInvalidPassword_Fail()
    {
        // Arrange

        var validUserRegistrationData = new PatientCredentialsDto()
        {
            Email = $"test{TestHelper.UniqueNumber}@test.com",
            Password = "12345678"
        };

        await _httpClient.PostAsJsonAsync(TestHelper.SignUpEndpointUri, validUserRegistrationData);

        var invalidUserCredentials = new PatientCredentialsDto()
        {
            Email = validUserRegistrationData.Email,
            Password = "87654321"
        };

        // Act

        var response = await _httpClient.PostAsJsonAsync(TestHelper.SignInEndpointUri, invalidUserCredentials);
        var responseMessage = await response.Content.ReadAsStringAsync();

        // Assert

        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(ApiMessages.FailedLoginMessage, responseMessage);
    }

    [Fact]
    public async Task TestUnregisteredPatientSignsIn_Fail()
    {
        // Arrange

        var unregisteredUserCredentials = new PatientCredentialsDto()
        {
            Email = $"test{TestHelper.UniqueNumber}@test.com",
            Password = "12345678"
        };

        // Act

        var response = await _httpClient.PostAsJsonAsync(TestHelper.SignInEndpointUri, unregisteredUserCredentials);


        // Assert

        Assert.False(response.IsSuccessStatusCode);
    }
}