using Bth.Modules.Accounts.Domain.Accounts;
using Bth.Modules.Accounts.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Bth.Modules.Accounts.Infrastructure.Accounts;

internal sealed class AccountRepository(AccountsDbContext dbContext) : IAccountRepository
{
    public Task<Account?> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Accounts.FirstOrDefaultAsync(account => account.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Account>> ListAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Accounts
            .OrderBy(account => account.HolderName)
            .ToListAsync(cancellationToken);

    public Task<bool> DocumentExistsAsync(string document, CancellationToken cancellationToken = default) =>
        dbContext.Accounts.AnyAsync(account => account.Document == document, cancellationToken);

    public void Insert(Account account) => dbContext.Accounts.Add(account);
}
