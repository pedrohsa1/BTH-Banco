using Bth.Common.Application.Messaging;
using Bth.Common.Domain;
using Bth.Modules.Transfers.Application.Abstractions;

namespace Bth.Modules.Transfers.Application.Transfers.ScheduleTransfer;

public sealed record ScheduleTransferCommand(
    Guid SourceAccountId,
    Guid DestinationAccountId,
    decimal Amount,
    DateTime ScheduledAtUtc,
    string? IdempotencyKey) : ICommand<TransferResponse>;

internal sealed class ScheduleTransferCommandHandler(ITransferExecutor executor)
    : ICommandHandler<ScheduleTransferCommand, TransferResponse>
{
    public Task<Result<TransferResponse>> Handle(
        ScheduleTransferCommand request,
        CancellationToken cancellationToken) =>
        executor.ScheduleAsync(
            request.SourceAccountId,
            request.DestinationAccountId,
            request.Amount,
            request.ScheduledAtUtc,
            request.IdempotencyKey,
            cancellationToken);
}
