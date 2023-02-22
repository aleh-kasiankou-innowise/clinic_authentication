using Innowise.Clinic.Auth.Persistence;
using Innowise.Clinic.Auth.Validators.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Innowise.Clinic.Auth.Validators.Custom;

public class TokenStateValidator : ITokenStateValidator
{
    private readonly ClinicAuthDbContext _dbContext;

    public TokenStateValidator(ClinicAuthDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> IsTokenStateValid(Guid id)
    {
        return await _dbContext.RefreshTokens.AnyAsync(x => x.UserId == id);
    }
}