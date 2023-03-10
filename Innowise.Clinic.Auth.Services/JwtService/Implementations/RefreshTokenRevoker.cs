using Innowise.Clinic.Auth.Exceptions.Jwt;
using Innowise.Clinic.Auth.Persistence;
using Innowise.Clinic.Auth.Persistence.Models;
using Innowise.Clinic.Auth.Services.JwtService.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Innowise.Clinic.Auth.Services.JwtService.Implementations;

public class RefreshTokenRevoker : ITokenRevoker
{
    private readonly ClinicAuthDbContext _dbContext;

    public RefreshTokenRevoker(ClinicAuthDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task RevokeTokenAsync(Guid tokenId)
    {
        var token = await _dbContext.RefreshTokens.FirstOrDefaultAsync(x => x.TokenId == tokenId);
        if (token == null)
        {
            throw new UnknownRefreshTokenException();
        }

        _dbContext.RefreshTokens.Remove(token);
        await _dbContext.SaveChangesAsync();
    }

    public void RevokeToken(Guid tokenId)
    {
        var token = _dbContext.RefreshTokens.FirstOrDefault(x => x.TokenId == tokenId);
        if (token == null)
        {
            throw new UnknownRefreshTokenException();
        }

        _dbContext.RefreshTokens.Remove(token);
        _dbContext.SaveChanges();
    }

    public async Task RevokeAllUserTokensAsync(Guid userId)
    {
        var userTokens = _dbContext.RefreshTokens.Where(x => x.UserId == userId);
        _dbContext.RefreshTokens.RemoveRange(userTokens);
        await _dbContext.SaveChangesAsync();
    }

    public void RevokeAllUserTokens(Guid userId)
    {
        var userTokens = _dbContext.RefreshTokens.Where(x => x.UserId == userId);
        _dbContext.RefreshTokens.RemoveRange(userTokens);
        _dbContext.SaveChanges();
    }

    public async Task RevokeAllRefreshTokensAsync()
    {
        var modelEntityType = _dbContext.Model.FindEntityType(typeof(RefreshToken));
        var tableName = modelEntityType?.GetSchemaQualifiedTableName() ??
                        throw new InvalidOperationException("The required table doesn't exist");

        await _dbContext.Database.ExecuteSqlRawAsync($"TRUNCATE TABLE {tableName};");
    }

    public void RevokeAllRefreshTokens()
    {
        var modelEntityType = _dbContext.Model.FindEntityType(typeof(RefreshToken));
        var tableName = modelEntityType?.GetSchemaQualifiedTableName() ??
                        throw new InvalidOperationException("The required table doesn't exist");

        _dbContext.Database.ExecuteSqlRaw($"TRUNCATE TABLE {tableName};");
    }
}