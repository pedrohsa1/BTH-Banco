namespace Bth.Modules.Transfers.Domain.Transfers;

public interface ITransferRepository
{
    Task<Transfer?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Transfer>> ListAsync(Guid? accountId, CancellationToken cancellationToken = default);

    void Insert(Transfer transfer);
}
