using Bth.Modules.Accounts.Contracts;
using Bth.Modules.Accounts.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Bth.Modules.Accounts.Infrastructure.Accounts;

internal sealed class AccountReader(AccountsDbContext dbContext) : IAccountReader
{
    public async Task<AccountSnapshot?> GetAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        var account = await dbContext.Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == accountId, cancellationToken);

        if (account is null)
        {
            return null;
        }

        return new AccountSnapshot(
            account.Id,
            account.HolderName,
            account.Document,
            account.Balance,
            account.OverdraftLimit,
            account.AvailableFunds,
            account.Status.ToString(),
            account.DayMaxAmountPerHour,
            account.DayMaxAttemptsPerHour,
            account.NightMaxAmountPerHour,
            account.NightMaxAttemptsPerHour);
    }
}
