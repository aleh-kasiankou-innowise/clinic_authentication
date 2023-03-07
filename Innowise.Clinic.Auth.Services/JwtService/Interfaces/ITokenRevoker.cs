namespace Innowise.Clinic.Auth.Services.JwtService.Interfaces;

public interface ITokenRevoker
{
    Task RevokeTokenAsync(Guid tokenId);
    void RevokeToken(Guid tokenId);
    Task RevokeAllUserTokensAsync(Guid userId);
    void RevokeAllUserTokens(Guid userId);
    Task RevokeAllRefreshTokensAsync();
    void RevokeAllRefreshTokens();
}