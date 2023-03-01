using Innowise.Clinic.Auth.Persistence;
using Innowise.Clinic.Auth.Persistence.Models;
using Innowise.Clinic.Auth.Services.AccountBlockingService.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Innowise.Clinic.Auth.Services.AccountBlockingService.Implementations;

public class AccountBlockingService : IAccountBlockingService
{
    private readonly ClinicAuthDbContext _dbContext;

    public AccountBlockingService(ClinicAuthDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SetAccountStatus(Guid accountId, bool shouldAccountBeActive)
    {
        if (shouldAccountBeActive)
        {
            var blockEntries = await _dbContext.AccountBlocks
                .Where(x => x.UserId == accountId)
                .ToListAsync();
            _dbContext.RemoveRange(blockEntries);
        }
        else
        {
            var newBlockEntry = new AccountBlock
            {
                UserId = accountId,
            };

            await _dbContext.AccountBlocks.AddAsync(newBlockEntry);
        }

        await _dbContext.SaveChangesAsync();
    }

    public async Task<bool> IsAccountActive(Guid accountId)
    {
        return await _dbContext.AccountBlocks.AllAsync(x => x.UserId != accountId);
    }
}