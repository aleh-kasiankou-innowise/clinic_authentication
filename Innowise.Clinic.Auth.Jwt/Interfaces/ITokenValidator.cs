using System.Security.Claims;
using Innowise.Clinic.Auth.Dto;

namespace Innowise.Clinic.Auth.Jwt.Interfaces;

public interface ITokenValidator
{
    Task<ClaimsPrincipal> ValidateTokenPairAndExtractPrincipal(AuthTokenPairDto authTokens,
        bool securityTokenShouldBeExpired = true);
}