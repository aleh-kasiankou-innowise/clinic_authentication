using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Innowise.Clinic.Auth.Dto;
using Innowise.Clinic.Auth.Jwt.Interfaces;
using Innowise.Clinic.Auth.Persistence;
using Innowise.Clinic.Auth.Persistence.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Innowise.Clinic.Auth.Jwt;

public class TokenService : ITokenService
{
    private readonly ClinicAuthDbContext _dbContext;
    private readonly IOptions<JwtSettings> _jwtOptions;
    private readonly SymmetricSecurityKey _securityKey;
    private readonly UserManager<IdentityUser<Guid>?> _userManager;

    public TokenService(IOptions<JwtSettings> jwtOptions, ClinicAuthDbContext dbContext,
        UserManager<IdentityUser<Guid>?> userManager)
    {
        _jwtOptions = jwtOptions;
        _dbContext = dbContext;
        _userManager = userManager;
        _securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Value.Key));
    }

    public string GenerateJwtToken(ClaimsPrincipal principal)
    {
        var expirationDate = DateTime.UtcNow.AddSeconds(_jwtOptions.Value.TokenValidityInSeconds);

        var token = IssueToken(expirationDate, principal.Claims);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<string> GenerateRefreshTokenAsync(Guid userId)
    {
        var expirationDate = DateTime.UtcNow.AddDays(_jwtOptions.Value.RefreshTokenValidityInDays);

        var tokenId = await RegisterIssuedRefreshTokenInDbAsync(userId);

        var refreshTokenClaimsCollection = new List<Claim>
        {
            new("jti", tokenId.ToString()),
            new(ClaimTypes.PrimarySid, userId.ToString())
        };

        var token = IssueToken(expirationDate, refreshTokenClaimsCollection);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<AuthTokenPairDto> GenerateJwtAndRefreshTokenAsync(IdentityUser<Guid> user)
    {
        var principal = await GetRegisteredUserPrincipalAsync(user);
        var jwtToken = GenerateJwtToken(principal);
        var refreshToken = await GenerateRefreshTokenAsync(user.Id);
        var authTokens = new AuthTokenPairDto(jwtToken, refreshToken);

        return authTokens;
    }

    private JwtSecurityToken IssueToken(DateTime expirationDate, IEnumerable<Claim> claims)
    {
        return new JwtSecurityToken(
            _jwtOptions.Value.ValidIssuer,
            expires: expirationDate,
            signingCredentials: new SigningCredentials(_securityKey, SecurityAlgorithms.HmacSha256),
            claims: claims
        );
    }

    private async Task<Guid> RegisterIssuedRefreshTokenInDbAsync(Guid userId)
    {
        var refreshToken = new RefreshToken
        {
            UserId = userId
        };

        await _dbContext.RefreshTokens.AddAsync(refreshToken);
        await _dbContext.SaveChangesAsync();

        return refreshToken.TokenId;
    }

    private async Task<ClaimsPrincipal> GetRegisteredUserPrincipalAsync(IdentityUser<Guid> user)
    {
        var getUserRolesTask = _userManager.GetRolesAsync(user);

        var authClaims = new List<Claim>
        {
            new(ClaimTypes.PrimarySid, user.Id.ToString())
        };

        var userRoles = await getUserRolesTask;
        foreach (var userRole in userRoles) authClaims.Add(new Claim(ClaimTypes.Role, userRole));

        var claimsIdentity = new ClaimsIdentity(authClaims);
        var principal = new ClaimsPrincipal(claimsIdentity);
        return principal;
    }
}