namespace Bth.Modules.Accounts.Domain.Accounts;

public enum EntryType
{
    Debit = 1,
    Credit = 2
}

public sealed class AccountEntry
{
    private AccountEntry()
    {
    }

    public Guid Id { get; private set; }

    public Guid AccountId { get; private set; }

    public Guid TransferId { get; private set; }

    public EntryType Type { get; private set; }

    public decimal Amount { get; private set; }

    public decimal BalanceAfter { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public static AccountEntry Create(
        Guid accountId,
        Guid transferId,
        EntryType type,
        decimal amount,
        decimal balanceAfter,
        DateTime utcNow) =>
        new()
        {
            Id = Guid.NewGuid(),
            AccountId = accountId,
            TransferId = transferId,
            Type = type,
            Amount = amount,
            BalanceAfter = balanceAfter,
            CreatedAtUtc = utcNow
        };
}
