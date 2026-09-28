using Bth.Common.Domain;

namespace Bth.Modules.Accounts.Domain.Accounts;

public sealed class AccountOpenedDomainEvent : DomainEvent
{
    public Guid AccountId { get; init; }

    public decimal Balance { get; init; }
}

public sealed class AccountDebitedDomainEvent : DomainEvent
{
    public Guid AccountId { get; init; }

    public Guid TransferId { get; init; }

    public decimal Amount { get; init; }

    public decimal BalanceAfter { get; init; }

    public string HolderName { get; init; } = string.Empty;

    public decimal OverdraftLimit { get; init; }

    public string Status { get; init; } = string.Empty;
}

public sealed class AccountCreditedDomainEvent : DomainEvent
{
    public Guid AccountId { get; init; }

    public Guid TransferId { get; init; }

    public decimal Amount { get; init; }

    public decimal BalanceAfter { get; init; }

    public string HolderName { get; init; } = string.Empty;

    public decimal OverdraftLimit { get; init; }

    public string Status { get; init; } = string.Empty;
}

public sealed class AccountLimitsChangedDomainEvent : DomainEvent
{
    public Guid AccountId { get; init; }

    public string HolderName { get; init; } = string.Empty;

    public decimal Balance { get; init; }

    public decimal OverdraftLimit { get; init; }

    public string Status { get; init; } = string.Empty;
}

public sealed class AccountStatusChangedDomainEvent : DomainEvent
{
    public Guid AccountId { get; init; }

    public string Status { get; init; } = string.Empty;

    public string HolderName { get; init; } = string.Empty;

    public decimal Balance { get; init; }

    public decimal OverdraftLimit { get; init; }
}
