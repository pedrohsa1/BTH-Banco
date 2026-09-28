using Bth.Common.Application.Messaging;
using Bth.Common.Domain;
using Bth.Modules.Transfers.Domain.Transfers;

namespace Bth.Modules.Transfers.Application.Transfers.GetTransfer;

public sealed record GetTransferQuery(Guid TransferId) : IQuery<TransferResponse>;

internal sealed class GetTransferQueryHandler(ITransferRepository transferRepository)
    : IQueryHandler<GetTransferQuery, TransferResponse>
{
    public async Task<Result<TransferResponse>> Handle(GetTransferQuery request, CancellationToken cancellationToken)
    {
        Transfer? transfer = await transferRepository.GetAsync(request.TransferId, cancellationToken);

        if (transfer is null)
        {
            return Result.Failure<TransferResponse>(TransferErrors.NotFound);
        }

        return TransferMapper.ToResponse(transfer, replayed: false);
    }
}
