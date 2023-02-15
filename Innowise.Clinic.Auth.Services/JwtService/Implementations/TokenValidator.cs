using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Innowise.Clinic.Auth.Dto;
using Innowise.Clinic.Auth.Exceptions.Jwt;
using Innowise.Clinic.Auth.Persistence;
using Innowise.Clinic.Auth.Services.Constants.Jwt;
using Innowise.Clinic.Auth.Services.JwtService.Data;
using Innowise.Clinic.Auth.Services.JwtService.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Innowise.Clinic.Auth.Services.JwtService.Implementations;

public class TokenValidator : ITokenValidator
{
    private readonly ClinicAuthDbContext _dbContext;
    private readonly IOptions<JwtSettings> _jwtConfiguration;
    private readonly IOptions<JwtValidationSettings> _jwtValidationConfiguration;

    public TokenValidator(ClinicAuthDbContext dbContext, IOptions<JwtSettings> jwtConfiguration,
        IOptions<JwtValidationSettings> jwtValidationConfiguration)
    {
        _dbContext = dbContext;
        _jwtConfiguration = jwtConfiguration;
        _jwtValidationConfiguration = jwtValidationConfiguration;
    }

    public async Task<ClaimsPrincipal> ValidateTokenPairAndExtractPrincipal(AuthTokenPairDto authTokens,
        bool tokenShouldBeExpired)
    {
        var principal = ExtractPrincipalFromJwtToken(authTokens.SecurityToken, tokenShouldBeExpired);

        await ValidateRefreshTokenAsync(authTokens.RefreshToken);

        return principal;
    }

    private ClaimsPrincipal ExtractPrincipalFromJwtToken(string token, bool tokenShouldBeExpired)
    {
        try
        {
            var securityToken = new JwtSecurityToken(token);
            if (tokenShouldBeExpired && securityToken.ValidTo > DateTime.UtcNow)
                throw new JwtTokenNotExpiredException();

            var principal = ValidateTokenAndReturnPrinciple(token, false);

            return principal;
        }

        catch (ArgumentException)
        {
            throw new InvalidTokenException("The token signature is invalid");
        }
    }

    private async Task ValidateRefreshTokenAsync(string token)
    {
        var principal = ValidateTokenAndReturnPrinciple(token, true);

        var extractedTokenId = principal.FindFirstValue(JwtClaimTypes.TokenIdClaim);
        var associatedUserId = principal.FindFirstValue(JwtClaimTypes.UserIdClaim);

        if (extractedTokenId == null) throw new TokenLacksTokenIdException();
        if (associatedUserId == null) throw new TokenLacksUserIdException();

        var tokenIsRegisteredInDb = await _dbContext.RefreshTokens.AnyAsync(x =>
            x.TokenId == Guid.Parse(extractedTokenId) &&
            x.UserId == Guid.Parse(associatedUserId));


        if (!tokenIsRegisteredInDb) throw new UnknownRefreshTokenException();
    }

    private ClaimsPrincipal ValidateTokenAndReturnPrinciple(string token, bool validateExpirationDate)
    {
        var jwtTokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = _jwtValidationConfiguration.Value.ValidateIssuer,
            ValidIssuer = _jwtConfiguration.Value.ValidIssuer,
            ValidateIssuerSigningKey = _jwtValidationConfiguration.Value.ValidateIssuerSigningKey,
            ValidateAudience = _jwtValidationConfiguration.Value.ValidateAudience,
            ValidAudience = _jwtConfiguration.Value.ValidAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfiguration.Value.Key)),
            ValidateLifetime = validateExpirationDate
        };

        var tokenHandler = new JwtSecurityTokenHandler();

        var principal = tokenHandler.ValidateToken(token, jwtTokenValidationParameters,
            out _);

        return principal;
    }
}