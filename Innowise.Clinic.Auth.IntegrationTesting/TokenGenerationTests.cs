using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Innowise.Clinic.Auth.Dto;
using Innowise.Clinic.Auth.Jwt;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace Innowise.Clinic.Auth.IntegrationTesting;

public class TokenGenerationTests : IClassFixture<IntegrationTestingWebApplicationFactory>
{
    private readonly IntegrationTestingWebApplicationFactory _factory;
    private readonly HttpClient _httpClient;
    private readonly IOptions<JwtData> _jwtData;

    public TokenGenerationTests(IntegrationTestingWebApplicationFactory factory)
    {
        _factory = factory;
        _httpClient = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        _jwtData = _factory.Services.GetService<IOptions<JwtData>>() ?? throw new InvalidOperationException();
    }


    [Fact]
    public async Task TestRegisteredUserWithValidEmailAndPasswordGetsValidTokens_OK()
    {
        // Arrange

        var validUserRegistrationData = new PatientCredentialsDto
        {
            Email = $"test{TestHelper.UniqueNumber}@test.gmail.com",
            Password = "12345678"
        };

        // Act

        var response = await _httpClient.PostAsJsonAsync(TestHelper.SignUpEndpointUri, validUserRegistrationData);

        // Assert

        Assert.True(response.IsSuccessStatusCode);

        var generatedTokens = await response.Content.ReadFromJsonAsync<AuthTokenPairDto>();

        Assert.NotNull(generatedTokens);
        Assert.NotNull(generatedTokens.SecurityToken);
        Assert.NotNull(generatedTokens.RefreshToken);

        var userId = TestHelper.ExtractUserIdFromJwtToken(generatedTokens.SecurityToken);
        var refreshTokenId = TestHelper.ExtractRefreshTokenId(generatedTokens.RefreshToken);


        Assert.True(_factory.UseDbContext(x => x.RefreshTokens
            .Any(rt => rt.TokenId == refreshTokenId && rt.UserId == userId)));
    }

    [Fact]
    public async Task TestExpiredJwtTokenRefreshedWithValidRefreshToken_OK()
    {
        // Arrange

        var validEmail = $"test{TestHelper.UniqueNumber}@test.gmail.com";

        var userRegistrationDataWithRegisteredEmail = new PatientCredentialsDto
        {
            Email = validEmail,
            Password = "12345678"
        };

        // Act

        var response =
            await _httpClient.PostAsJsonAsync(TestHelper.SignUpEndpointUri, userRegistrationDataWithRegisteredEmail);
        var generatedTokens = await response.Content.ReadFromJsonAsync<AuthTokenPairDto>();

        await Task.Delay(_jwtData.Value.TokenValidityInSeconds * 1000);

        response = await _httpClient.PostAsJsonAsync(TestHelper.RefreshTokenEndpointUri, generatedTokens);
        var refreshedToken = await response.Content.ReadFromJsonAsync<string>();
        // Assert

        Assert.True(response.IsSuccessStatusCode);
        TestHelper.ValidateJwtToken(_factory, refreshedToken);
    }

    [Fact]
    public async Task TestValidActiveJwtTokenRefreshedWithValidRefreshToken_Fail()
    {
        // Arrange

        var validEmail = $"test{TestHelper.UniqueNumber}@test.gmail.com";

        var userRegistrationDataWithRegisteredEmail = new PatientCredentialsDto
        {
            Email = validEmail,
            Password = "12345678"
        };

        // Act

        var response =
            await _httpClient.PostAsJsonAsync(TestHelper.SignUpEndpointUri, userRegistrationDataWithRegisteredEmail);
        var generatedTokens = await response.Content.ReadFromJsonAsync<AuthTokenPairDto>();
        response = await _httpClient.PostAsJsonAsync(TestHelper.RefreshTokenEndpointUri, generatedTokens);
        // Assert

        Assert.False(response.IsSuccessStatusCode);
    }

    [Fact]
    public async Task TestExpiredJwtTokenWithWrongSecurityKeyRefreshedWithValidRefreshToken_Fail()
    {
        // Arrange

        var validEmail = $"test{TestHelper.UniqueNumber}@test.gmail.com";

        var userRegistrationDataWithRegisteredEmail = new PatientCredentialsDto
        {
            Email = validEmail,
            Password = "12345678"
        };

        // Act

        var response =
            await _httpClient.PostAsJsonAsync(TestHelper.SignUpEndpointUri, userRegistrationDataWithRegisteredEmail);
        var generatedTokens = await response.Content.ReadFromJsonAsync<AuthTokenPairDto>();

        var invalidJwtToken = new JwtSecurityToken(
            _factory.UseConfiguration(x => x.GetValue<string>("JWT:ValidIssuer")),
            expires: DateTime.UtcNow - TimeSpan.FromHours(1),
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes("123456791011121314151617181920")),
                SecurityAlgorithms.HmacSha256),
            claims: TestHelper.ValidateJwtToken(_factory, generatedTokens.SecurityToken).Claims
        );

        var invalidTokenJson = new JwtSecurityTokenHandler().WriteToken(invalidJwtToken);

        response = await _httpClient.PostAsJsonAsync(TestHelper.RefreshTokenEndpointUri,
            new AuthTokenPairDto(invalidTokenJson, generatedTokens.RefreshToken));

        // Assert

        Assert.False(response.IsSuccessStatusCode);
        Assert.ThrowsAny<Exception>(delegate { TestHelper.ValidateJwtToken(_factory, invalidTokenJson); });
    }

    [Fact]
    public async Task TestExpiredJwtTokenWithWrongIssuerRefreshedWithValidRefreshToken_Fail()
    {
        // Arrange

        var validEmail = $"test{TestHelper.UniqueNumber}@test.gmail.com";

        var userRegistrationDataWithRegisteredEmail = new PatientCredentialsDto
        {
            Email = validEmail,
            Password = "12345678"
        };

        // Act

        var response =
            await _httpClient.PostAsJsonAsync(TestHelper.SignUpEndpointUri, userRegistrationDataWithRegisteredEmail);
        var generatedTokens = await response.Content.ReadFromJsonAsync<AuthTokenPairDto>();

        var invalidJwtToken = new JwtSecurityToken(
            "WrongIssuer",
            expires: DateTime.UtcNow - TimeSpan.FromHours(1),
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(_factory.UseConfiguration(x => x.GetValue<string>("JWT:Key")))),
                SecurityAlgorithms.HmacSha256),
            claims: TestHelper.ValidateJwtToken(_factory, generatedTokens.SecurityToken).Claims
        );

        var invalidTokenJson = new JwtSecurityTokenHandler().WriteToken(invalidJwtToken);

        response = await _httpClient.PostAsJsonAsync(TestHelper.RefreshTokenEndpointUri,
            new AuthTokenPairDto(invalidTokenJson, generatedTokens.RefreshToken));

        // Assert

        Assert.False(response.IsSuccessStatusCode);
        Assert.ThrowsAny<Exception>(delegate { TestHelper.ValidateJwtToken(_factory, invalidTokenJson); });
    }

    [Fact]
    public async Task TestValidExpiredJwtTokenRefreshedWithRefreshTokenWithWrongSecurityKey_Fail()
    {
        // Arrange

        var validEmail = $"test{TestHelper.UniqueNumber}@test.gmail.com";

        var userRegistrationDataWithRegisteredEmail = new PatientCredentialsDto
        {
            Email = validEmail,
            Password = "12345678"
        };

        // Act

        var response =
            await _httpClient.PostAsJsonAsync(TestHelper.SignUpEndpointUri, userRegistrationDataWithRegisteredEmail);
        var generatedTokens = await response.Content.ReadFromJsonAsync<AuthTokenPairDto>();


        var invalidRefreshToken = new JwtSecurityToken(
            _factory.UseConfiguration(x => x.GetValue<string>("JWT:ValidIssuer")),
            expires: DateTime.UtcNow - TimeSpan.FromHours(1),
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes("123456791011121314151617181920")),
                SecurityAlgorithms.HmacSha256),
            claims: TestHelper.ValidateJwtToken(_factory, generatedTokens.RefreshToken).Claims
        );

        var invalidTokenJson = new JwtSecurityTokenHandler().WriteToken(invalidRefreshToken);

        await Task.Delay(_jwtData.Value.TokenValidityInSeconds * 1000);


        response = await _httpClient.PostAsJsonAsync(TestHelper.RefreshTokenEndpointUri,
            new AuthTokenPairDto(generatedTokens.SecurityToken, invalidTokenJson));

        // Assert

        Assert.False(response.IsSuccessStatusCode);
        Assert.ThrowsAny<Exception>(delegate { TestHelper.ValidateJwtToken(_factory, invalidTokenJson); });
    }

    [Fact]
    public async Task TestValidExpiredJwtTokenRefreshedWithRefreshTokenWithWrongIssuer_Fail()
    {
        // Arrange

        var validEmail = $"test{TestHelper.UniqueNumber}@test.gmail.com";

        var userRegistrationDataWithRegisteredEmail = new PatientCredentialsDto
        {
            Email = validEmail,
            Password = "12345678"
        };

        // Act

        var response =
            await _httpClient.PostAsJsonAsync(TestHelper.SignUpEndpointUri, userRegistrationDataWithRegisteredEmail);
        var generatedTokens = await response.Content.ReadFromJsonAsync<AuthTokenPairDto>();


        var invalidRefreshToken = new JwtSecurityToken(
            "WrongIssuee",
            expires: DateTime.UtcNow - TimeSpan.FromHours(1),
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(_factory.UseConfiguration(x => x.GetValue<string>("JWT:Key")))),
                SecurityAlgorithms.HmacSha256),
            claims: TestHelper.ValidateJwtToken(_factory, generatedTokens.RefreshToken).Claims
        );

        var invalidTokenJson = new JwtSecurityTokenHandler().WriteToken(invalidRefreshToken);

        await Task.Delay(_jwtData.Value.TokenValidityInSeconds * 1000);


        response = await _httpClient.PostAsJsonAsync(TestHelper.RefreshTokenEndpointUri,
            new AuthTokenPairDto(generatedTokens.SecurityToken, invalidTokenJson));

        // Assert

        Assert.False(response.IsSuccessStatusCode);
        Assert.ThrowsAny<Exception>(delegate { TestHelper.ValidateJwtToken(_factory, invalidTokenJson); });
    }
}