using Bth.Common.Domain;

namespace Bth.Modules.Transfers.Domain.Transfers;

public sealed class TransferChangedDomainEvent : DomainEvent
{
    public Guid TransferId { get; init; }

    public Guid SourceAccountId { get; init; }

    public Guid DestinationAccountId { get; init; }

    public decimal Amount { get; init; }

    public string Status { get; init; } = string.Empty;

    public DateTime? ScheduledAtUtc { get; init; }

    public DateTime CreatedAtUtc { get; init; }

    public DateTime? ProcessedAtUtc { get; init; }

    public string? FailureReason { get; init; }
}
