using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Innowise.Clinic.Auth.Dto;
using Innowise.Clinic.Auth.Exceptions.Testing;
using Innowise.Clinic.Auth.Services.Constants.Jwt;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Innowise.Clinic.Auth.IntegrationTesting;

internal static class TestHelper
{
    internal const string SignUpEndpointUri = "authentication/sign-up/patient";
    internal const string RefreshTokenEndpointUri = "authentication/token/refresh";
    internal const string SignInEndpointUri = "authentication/sign-in";
    internal const string SignOutEndpointUri = "authentication/sign-out";

    private static int _uniqueNumber;
    internal static int UniqueNumber => _uniqueNumber++;


    internal static ClaimsPrincipal ValidateJwtToken(IntegrationTestingWebApplicationFactory factory, string token)
    {
        var jwtTokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer =
                Convert.ToBoolean(factory.UseConfiguration(x =>
                    x.GetValue<string>("JwtValidationConfiguration:ValidateIssuer"))),
            ValidIssuer = factory.UseConfiguration(x => x.GetValue<string>("JWT:ValidIssuer")),
            ValidateIssuerSigningKey = Convert.ToBoolean(factory.UseConfiguration(x =>
                x.GetValue<string>("JwtValidationConfiguration:ValidateIssuerSigningKey"))),
            ValidateAudience = Convert.ToBoolean(factory.UseConfiguration(x =>
                x.GetValue<string>("JwtValidationConfiguration:ValidateAudience"))),
            ValidAudience = factory.UseConfiguration(x => x.GetValue<string>("JWT:ValidAudience")),
            IssuerSigningKey =
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(factory.UseConfiguration(x => x.GetValue<string>("JWT:Key")))),
            ValidateLifetime = Convert.ToBoolean(factory.UseConfiguration(x =>
                x.GetValue<string>("JwtValidationConfiguration:ValidateLifetime")))
        };
        var tokenHandler = new JwtSecurityTokenHandler();
        return tokenHandler.ValidateToken(token, jwtTokenValidationParameters,
            out _);
    }

    internal static Guid ExtractUserIdFromJwtToken(string token)
    {
        var jwtToken = new JwtSecurityToken(token);
        var userId = jwtToken.Claims.First(x => x.Type == JwtClaimTypes.UserIdClaim).Value;
        return Guid.Parse(userId);
    }

    internal static Guid ExtractRefreshTokenId(string refreshToken)
    {
        var refreshJwtToken = new JwtSecurityToken(refreshToken);
        var tokenId = refreshJwtToken.Claims.First(x => x.Type == JwtClaimTypes.TokenIdClaim).Value;
        return Guid.Parse(tokenId);
    }

    internal static async Task<AuthTokenPairDto> RegisterUserAndGetTokens(HttpClient httpClient,
        UserCredentialsDto userCredentials)
    {
        await httpClient.PostAsJsonAsync(SignUpEndpointUri, userCredentials);
        var loginResult = await httpClient.PostAsJsonAsync(SignInEndpointUri,
            userCredentials);
        var generatedTokens = await loginResult.Content.ReadFromJsonAsync<AuthTokenPairDto>();
        return generatedTokens ?? throw new TokensNotReturnedException();
    }
}