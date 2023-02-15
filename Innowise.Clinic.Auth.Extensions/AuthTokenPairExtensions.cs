using System.IdentityModel.Tokens.Jwt;
using Innowise.Clinic.Auth.Dto;
using Innowise.Clinic.Auth.Exceptions.Jwt;

namespace Innowise.Clinic.Auth.Extensions;

public static class AuthTokenPairExtensions
{
    // TODO Use constants instead of literals
    public static Guid GetRefreshTokenId(this AuthTokenPairDto tokens)
    {
        var refreshToken = new JwtSecurityToken(tokens.RefreshToken);

        var tokenIdClaim = refreshToken.Claims.SingleOrDefault(x => x.Type == "jti");

        if (tokenIdClaim != null) return Guid.Parse(tokenIdClaim.Value);

        throw new PrincipalLacksTokenIdException();
    }
    // TODO Use constants instead of literals

    public static Guid GetUserId(this AuthTokenPairDto tokens)
    {
        var refreshToken = new JwtSecurityToken(tokens.RefreshToken);

        var userIdClaim = refreshToken.Claims.SingleOrDefault(x => x.Type == "user-id");

        if (userIdClaim != null) return Guid.Parse(userIdClaim.Value);

        throw new TokenLacksUserIdException();
    }
}