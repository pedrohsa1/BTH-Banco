namespace Bth.Modules.Accounts.Contracts;

public interface IAccountReader
{
    Task<AccountSnapshot?> GetAsync(Guid accountId, CancellationToken cancellationToken = default);
}
