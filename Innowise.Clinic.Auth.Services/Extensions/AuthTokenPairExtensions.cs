using System.IdentityModel.Tokens.Jwt;
using Innowise.Clinic.Auth.Dto;
using Innowise.Clinic.Auth.Exceptions.Jwt;
using Innowise.Clinic.Auth.Services.Constants.Jwt;

namespace Innowise.Clinic.Auth.Services.Extensions;

public static class AuthTokenPairExtensions
{
    public static Guid GetRefreshTokenId(this AuthTokenPairDto tokens)
    {
        var refreshToken = new JwtSecurityToken(tokens.RefreshToken);

        var tokenIdClaim = refreshToken.Claims.SingleOrDefault(x => x.Type == JwtClaimTypes.TokenIdClaim);

        if (tokenIdClaim != null) return Guid.Parse(tokenIdClaim.Value);

        throw new PrincipalLacksTokenIdException();
    }

    public static Guid GetUserId(this AuthTokenPairDto tokens)
    {
        var refreshToken = new JwtSecurityToken(tokens.RefreshToken);

        var userIdClaim = refreshToken.Claims.SingleOrDefault(x => x.Type == JwtClaimTypes.UserIdClaim);

        if (userIdClaim != null) return Guid.Parse(userIdClaim.Value);

        throw new TokenLacksUserIdException();
    }
}