using Bth.Modules.Transfers.Domain.Transfers;
using Bth.Modules.Transfers.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Bth.Modules.Transfers.Infrastructure.Transfers;

internal sealed class TransferRepository(TransfersDbContext dbContext) : ITransferRepository
{
    public Task<Transfer?> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Transfers.FirstOrDefaultAsync(transfer => transfer.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Transfer>> ListAsync(Guid? accountId, CancellationToken cancellationToken = default)
    {
        IQueryable<Transfer> query = dbContext.Transfers.AsNoTracking();

        if (accountId is not null)
        {
            query = query.Where(transfer =>
                transfer.SourceAccountId == accountId || transfer.DestinationAccountId == accountId);
        }

        return await query
            .OrderByDescending(transfer => transfer.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public void Insert(Transfer transfer) => dbContext.Transfers.Add(transfer);
}
