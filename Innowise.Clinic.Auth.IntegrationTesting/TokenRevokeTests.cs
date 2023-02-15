using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Innowise.Clinic.Auth.Dto;
using Innowise.Clinic.Auth.Extensions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Innowise.Clinic.Auth.IntegrationTesting;

public class TokenRevokeTests : IClassFixture<IntegrationTestingWebApplicationFactory>
{
    private readonly IntegrationTestingWebApplicationFactory _factory;
    private readonly HttpClient _httpClient;


    public TokenRevokeTests(IntegrationTestingWebApplicationFactory factory)
    {
        _factory = factory;
        _httpClient = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task TestRefreshTokenRevokedWhenPatientSignOutSuccessful_OK()
    {
        // Arrange

        var validUserRegistrationData = new UserCredentialsDto
        {
            Email = $"test{TestHelper.UniqueNumber}@test.com",
            Password = "12345678"
        };

        // Act

        var response = await _httpClient.PostAsJsonAsync(TestHelper.SignUpEndpointUri, validUserRegistrationData);
        var generatedTokens = await response.Content.ReadFromJsonAsync<AuthTokenPairDto>();
        response = await _httpClient.PostAsJsonAsync(TestHelper.SignOutEndpointUri, generatedTokens);

        // Assert

        Assert.True(response.IsSuccessStatusCode);
        Assert.False(_factory.UseDbContext(x =>
            x.RefreshTokens.Any(t => t.TokenId == generatedTokens.GetRefreshTokenId())));
    }

    [Fact]
    public async Task TestAllUserTokensRevokedOnTokenRefreshFail_Ok()
    {
        // Arrange

        var validUserRegistrationData = new UserCredentialsDto
        {
            Email = $"test{TestHelper.UniqueNumber}@test.com",
            Password = "12345678"
        };

        // Act

        var response = await _httpClient.PostAsJsonAsync(TestHelper.SignUpEndpointUri, validUserRegistrationData);
        var generatedTokens = await response.Content.ReadFromJsonAsync<AuthTokenPairDto>();
        response = await _httpClient.PostAsJsonAsync(TestHelper.RefreshTokenEndpointUri, generatedTokens);

        // Assert
        Assert.False(response.IsSuccessStatusCode);
        Assert.False(_factory.UseDbContext(x => x.RefreshTokens.Any(t => t.UserId == generatedTokens.GetUserId())));
    }

    [Fact]
    public async Task TestAllUserTokensRevokedOnLoginFail_Ok()
    {
        // Arrange

        var validUserRegistrationData = new UserCredentialsDto
        {
            Email = $"test{TestHelper.UniqueNumber}@test.com",
            Password = "12345678"
        };

        var invalidUserCredentials = new UserCredentialsDto
        {
            Email = validUserRegistrationData.Email,
            Password = "87654321"
        };

        // Act

        var response = await _httpClient.PostAsJsonAsync(TestHelper.SignUpEndpointUri, validUserRegistrationData);
        var generatedTokens = await response.Content.ReadFromJsonAsync<AuthTokenPairDto>();
        response = await _httpClient.PostAsJsonAsync(TestHelper.SignInEndpointUri, invalidUserCredentials);

        // Assert
        Assert.False(response.IsSuccessStatusCode);
        Assert.False(_factory.UseDbContext(x => x.RefreshTokens.Any(t => t.UserId == generatedTokens.GetUserId())));
    }
}