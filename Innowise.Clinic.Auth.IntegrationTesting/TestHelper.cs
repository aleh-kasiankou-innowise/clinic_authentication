using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Innowise.Clinic.Auth.Constants;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Innowise.Clinic.Auth.IntegrationTesting;

public static class TestHelper
{
    
    internal const string SignUpEndpointUri =
        ControllerRoutes.AuthenticationControllerRoute + "/" + EndpointRoutes.SignUpEndpointRoute;

    internal const string RefreshTokenEndpointUri =
        ControllerRoutes.AuthenticationControllerRoute + "/" + EndpointRoutes.RefreshTokenEndpointRoute;

    internal const string SignInEndpointUri = ControllerRoutes.AuthenticationControllerRoute + "/" +
                                              EndpointRoutes.SignInEndpointRoute;

    internal const string SignOutEndpointUri = ControllerRoutes.AuthenticationControllerRoute + "/" +
                                               EndpointRoutes.SignOutEndpointRoute;

    
    internal static int _uniqueNumber = 0;

    internal static int UniqueNumber => _uniqueNumber++;
    

    internal static ClaimsPrincipal ValidateJwtToken(IntegrationTestingWebApplicationFactory factory, string token)
    {
        var jwtTokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = factory.UseConfiguration(x => x.GetValue<string>("JWT:ValidIssuer")),
            ValidateIssuerSigningKey = true,
            ValidateAudience = false,
            IssuerSigningKey =
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(factory.UseConfiguration(x => x.GetValue<string>("JWT:Key")))),
            ValidateLifetime = true
        };

        var tokenHandler = new JwtSecurityTokenHandler();

        return tokenHandler.ValidateToken(token, jwtTokenValidationParameters,
            out _);
    }

    internal static Guid ExtractUserIdFromJwtToken(string token)
    {
        var jwtToken = new JwtSecurityToken(token);
        var userId = jwtToken.Claims.First(x => x.Type == ClaimTypes.PrimarySid).Value;
        return Guid.Parse(userId);
    }

    internal static Guid ExtractRefreshTokenId(string refreshToken)
    {
        var refreshJwtToken = new JwtSecurityToken(refreshToken);
        var tokenId = refreshJwtToken.Claims.First(x => x.Type == "jti").Value;
        return Guid.Parse(tokenId);
    }
}