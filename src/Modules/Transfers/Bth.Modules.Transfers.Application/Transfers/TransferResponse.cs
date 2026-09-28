namespace Bth.Modules.Transfers.Application.Transfers;

public sealed record TransferResponse(
    Guid Id,
    Guid SourceAccountId,
    Guid DestinationAccountId,
    decimal Amount,
    string Status,
    DateTime? ScheduledAtUtc,
    DateTime CreatedAtUtc,
    DateTime? AttemptedAtUtc,
    DateTime? ProcessedAtUtc,
    string? FailureReason,
    bool Replayed);
