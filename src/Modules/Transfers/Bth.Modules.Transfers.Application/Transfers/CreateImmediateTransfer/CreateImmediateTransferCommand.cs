using Bth.Common.Application.Messaging;
using Bth.Common.Domain;
using Bth.Modules.Transfers.Application.Abstractions;

namespace Bth.Modules.Transfers.Application.Transfers.CreateImmediateTransfer;

public sealed record CreateImmediateTransferCommand(
    Guid SourceAccountId,
    Guid DestinationAccountId,
    decimal Amount,
    string? IdempotencyKey) : ICommand<TransferResponse>;

internal sealed class CreateImmediateTransferCommandHandler(ITransferExecutor executor)
    : ICommandHandler<CreateImmediateTransferCommand, TransferResponse>
{
    public Task<Result<TransferResponse>> Handle(
        CreateImmediateTransferCommand request,
        CancellationToken cancellationToken) =>
        executor.ExecuteImmediateAsync(
            request.SourceAccountId,
            request.DestinationAccountId,
            request.Amount,
            request.IdempotencyKey,
            cancellationToken);
}
