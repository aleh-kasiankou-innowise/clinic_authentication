using System.Security.Claims;
using Innowise.Clinic.Auth.Dto;
using Microsoft.AspNetCore.Identity;

namespace Innowise.Clinic.Auth.Services.JwtService.Interfaces;

public interface ITokenService
{
    Task<AuthTokenPairDto> GenerateJwtAndRefreshTokenAsync(IdentityUser<Guid> user);
    string GenerateJwtToken(ClaimsPrincipal principal);
    Task<string> GenerateRefreshTokenAsync(Guid userId);
}