using Bth.Modules.Transfers.Domain.Transfers;

namespace Bth.Modules.Transfers.Application.Transfers;

public static class TransferMapper
{
    public static TransferResponse ToResponse(Transfer transfer, bool replayed) =>
        new(
            transfer.Id,
            transfer.SourceAccountId,
            transfer.DestinationAccountId,
            transfer.Amount,
            transfer.Status.ToString(),
            transfer.ScheduledAtUtc,
            transfer.CreatedAtUtc,
            transfer.AttemptedAtUtc,
            transfer.ProcessedAtUtc,
            transfer.FailureReason,
            replayed);
}
