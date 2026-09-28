namespace Bth.Modules.Accounts.Domain.Accounts;

public interface IAccountRepository
{
    Task<Account?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Account>> ListAsync(CancellationToken cancellationToken = default);

    Task<bool> DocumentExistsAsync(string document, CancellationToken cancellationToken = default);

    void Insert(Account account);
}
