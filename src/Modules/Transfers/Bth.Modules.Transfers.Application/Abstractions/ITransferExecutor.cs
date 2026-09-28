using Bth.Common.Domain;
using Bth.Modules.Transfers.Application.Transfers;

namespace Bth.Modules.Transfers.Application.Abstractions;

public interface ITransferExecutor
{
    Task<Result<TransferResponse>> ExecuteImmediateAsync(
        Guid sourceAccountId,
        Guid destinationAccountId,
        decimal amount,
        string? idempotencyKey,
        CancellationToken cancellationToken);

    Task<Result<TransferResponse>> ScheduleAsync(
        Guid sourceAccountId,
        Guid destinationAccountId,
        decimal amount,
        DateTime scheduledAtUtc,
        string? idempotencyKey,
        CancellationToken cancellationToken);

    Task<Result<TransferResponse>> CancelAsync(Guid transferId, CancellationToken cancellationToken);

    Task ExecuteDueAsync(Guid transferId, CancellationToken cancellationToken);

    Task DispatchDueAsync(CancellationToken cancellationToken);
}

public interface ITransferScheduler
{
    void Schedule(Guid transferId, DateTime scheduledAtUtc);
}
