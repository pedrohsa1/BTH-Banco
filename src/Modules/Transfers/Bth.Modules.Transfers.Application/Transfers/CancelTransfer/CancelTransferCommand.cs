using Bth.Common.Application.Messaging;
using Bth.Common.Domain;
using Bth.Modules.Transfers.Application.Abstractions;

namespace Bth.Modules.Transfers.Application.Transfers.CancelTransfer;

public sealed record CancelTransferCommand(Guid TransferId) : ICommand<TransferResponse>;

internal sealed class CancelTransferCommandHandler(ITransferExecutor executor)
    : ICommandHandler<CancelTransferCommand, TransferResponse>
{
    public Task<Result<TransferResponse>> Handle(
        CancelTransferCommand request,
        CancellationToken cancellationToken) =>
        executor.CancelAsync(request.TransferId, cancellationToken);
}
