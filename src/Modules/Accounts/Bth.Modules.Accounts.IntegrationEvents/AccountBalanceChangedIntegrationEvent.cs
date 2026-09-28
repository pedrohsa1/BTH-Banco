using Bth.Common.Application.EventBus;

namespace Bth.Modules.Accounts.IntegrationEvents;

public sealed class AccountBalanceChangedIntegrationEvent : IntegrationEvent
{
    public AccountBalanceChangedIntegrationEvent(
        Guid id,
        DateTime occurredOnUtc,
        Guid accountId,
        string holderName,
        decimal balance,
        decimal overdraftLimit,
        string status)
        : base(id, occurredOnUtc)
    {
        AccountId = accountId;
        HolderName = holderName;
        Balance = balance;
        OverdraftLimit = overdraftLimit;
        Status = status;
    }

    public Guid AccountId { get; init; }

    public string HolderName { get; init; }

    public decimal Balance { get; init; }

    public decimal OverdraftLimit { get; init; }

    public string Status { get; init; }
}
