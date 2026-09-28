using Bth.Common.Application.EventBus;

namespace Bth.Modules.Transfers.IntegrationEvents;

public sealed class TransferStatusChangedIntegrationEvent : IntegrationEvent
{
    public TransferStatusChangedIntegrationEvent(
        Guid id,
        DateTime occurredOnUtc,
        Guid transferId,
        Guid sourceAccountId,
        Guid destinationAccountId,
        decimal amount,
        string status,
        DateTime? scheduledAtUtc,
        DateTime createdAtUtc,
        DateTime? processedAtUtc,
        string? failureReason)
        : base(id, occurredOnUtc)
    {
        TransferId = transferId;
        SourceAccountId = sourceAccountId;
        DestinationAccountId = destinationAccountId;
        Amount = amount;
        Status = status;
        ScheduledAtUtc = scheduledAtUtc;
        CreatedAtUtc = createdAtUtc;
        ProcessedAtUtc = processedAtUtc;
        FailureReason = failureReason;
    }

    public Guid TransferId { get; init; }

    public Guid SourceAccountId { get; init; }

    public Guid DestinationAccountId { get; init; }

    public decimal Amount { get; init; }

    public string Status { get; init; }

    public DateTime? ScheduledAtUtc { get; init; }

    public DateTime CreatedAtUtc { get; init; }

    public DateTime? ProcessedAtUtc { get; init; }

    public string? FailureReason { get; init; }
}
