using Bth.Common.Domain;

namespace Bth.Modules.Transfers.Domain.Transfers;

public sealed class Transfer : Entity
{
    private Transfer()
    {
    }

    public Guid Id { get; private set; }

    public Guid SourceAccountId { get; private set; }

    public Guid DestinationAccountId { get; private set; }

    public decimal Amount { get; private set; }

    public TransferStatus Status { get; private set; }

    public DateTime? ScheduledAtUtc { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime? AttemptedAtUtc { get; private set; }

    public DateTime? ProcessedAtUtc { get; private set; }

    public string? FailureReason { get; private set; }

    public string? IdempotencyKey { get; private set; }

    public string? RequestHash { get; private set; }

    public static Transfer CreateImmediate(
        Guid sourceAccountId,
        Guid destinationAccountId,
        decimal amount,
        DateTime utcNow,
        string? idempotencyKey,
        string requestHash)
    {
        var transfer = new Transfer
        {
            Id = Guid.NewGuid(),
            SourceAccountId = sourceAccountId,
            DestinationAccountId = destinationAccountId,
            Amount = amount,
            Status = TransferStatus.Processing,
            CreatedAtUtc = utcNow,
            AttemptedAtUtc = utcNow,
            IdempotencyKey = idempotencyKey,
            RequestHash = requestHash
        };

        transfer.RaiseChanged();
        return transfer;
    }

    public static Result<Transfer> Schedule(
        Guid sourceAccountId,
        Guid destinationAccountId,
        decimal amount,
        DateTime scheduledAtUtc,
        DateTime utcNow,
        string? idempotencyKey,
        string requestHash)
    {
        if (scheduledAtUtc <= utcNow)
        {
            return Result.Failure<Transfer>(TransferErrors.ScheduleMustBeFuture);
        }

        var transfer = new Transfer
        {
            Id = Guid.NewGuid(),
            SourceAccountId = sourceAccountId,
            DestinationAccountId = destinationAccountId,
            Amount = amount,
            Status = TransferStatus.Scheduled,
            ScheduledAtUtc = scheduledAtUtc,
            CreatedAtUtc = utcNow,
            IdempotencyKey = idempotencyKey,
            RequestHash = requestHash
        };

        transfer.RaiseChanged();
        return transfer;
    }

    public Result BeginProcessing(DateTime utcNow)
    {
        if (Status != TransferStatus.Scheduled)
        {
            return Result.Failure(TransferErrors.InvalidTransition);
        }

        Status = TransferStatus.Processing;
        AttemptedAtUtc = utcNow;
        RaiseChanged();
        return Result.Success();
    }

    public Result Complete(DateTime utcNow)
    {
        if (Status != TransferStatus.Processing)
        {
            return Result.Failure(TransferErrors.InvalidTransition);
        }

        Status = TransferStatus.Completed;
        ProcessedAtUtc = utcNow;
        FailureReason = null;
        RaiseChanged();
        return Result.Success();
    }

    public Result Fail(string reason, DateTime utcNow)
    {
        if (Status is not (TransferStatus.Processing or TransferStatus.Scheduled))
        {
            return Result.Failure(TransferErrors.InvalidTransition);
        }

        Status = TransferStatus.Failed;
        FailureReason = reason;
        ProcessedAtUtc = utcNow;
        AttemptedAtUtc ??= utcNow;
        RaiseChanged();
        return Result.Success();
    }

    public Result Cancel()
    {
        if (Status != TransferStatus.Scheduled)
        {
            return Result.Failure(TransferErrors.CannotCancel);
        }

        Status = TransferStatus.Cancelled;
        RaiseChanged();
        return Result.Success();
    }

    private void RaiseChanged()
    {
        Raise(new TransferChangedDomainEvent
        {
            TransferId = Id,
            SourceAccountId = SourceAccountId,
            DestinationAccountId = DestinationAccountId,
            Amount = Amount,
            Status = Status.ToString(),
            ScheduledAtUtc = ScheduledAtUtc,
            CreatedAtUtc = CreatedAtUtc,
            ProcessedAtUtc = ProcessedAtUtc,
            FailureReason = FailureReason
        });
    }
}
