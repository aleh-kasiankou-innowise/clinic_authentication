using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
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
        _httpClient = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task TestRegisteredPatientSignsInWithValidCredentials_OK()
    {
        // Arrange

        var validUserRegistrationData = new UserCredentialsDto
        {
            Email = $"test{TestHelper.UniqueNumber}@test.com",
            Password = "12345678"
        };

        // Act

        var generatedTokens = await TestHelper.RegisterUserAndGetTokens(_httpClient, validUserRegistrationData);

        // Assert

        Assert.NotNull(generatedTokens.SecurityToken);
        Assert.NotNull(generatedTokens.RefreshToken);

        var userId = TestHelper.ExtractUserIdFromJwtToken(generatedTokens.SecurityToken);
        Assert.NotNull(_factory.UseDbContext(x =>
            x.Users.SingleOrDefaultAsync(u => u.Id == userId && u.Email == validUserRegistrationData.Email)));
    }

    [Fact]
    public async Task TestRegisteredPatientSignsInWithInvalidPassword_Fail()
    {
        // Arrange

        var validUserRegistrationData = new UserCredentialsDto
        {
            Email = $"test{TestHelper.UniqueNumber}@test.com",
            Password = "12345678"
        };

        var invalidUserCredentials = validUserRegistrationData with { Password = "87654321" };

        // Act

        await _httpClient.PostAsJsonAsync(TestHelper.SignUpEndpointUri, validUserRegistrationData);
        var response = await _httpClient.PostAsJsonAsync(TestHelper.SignInEndpointUri, invalidUserCredentials);
        var responseMessage = await response.Content.ReadFromJsonAsync<string>();

        // Assert

        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal("Either an email or a password is incorrect", responseMessage);
    }

    [Fact]
    public async Task TestUnregisteredPatientSignsIn_Fail()
    {
        // Arrange

        var unregisteredUserCredentials = new UserCredentialsDto
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