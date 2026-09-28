using Bth.Common.Application.Messaging;
using Bth.Common.Domain;
using Bth.Modules.Transfers.Domain.Transfers;

namespace Bth.Modules.Transfers.Application.Transfers.GetTransfers;

public sealed record GetTransfersQuery(Guid? AccountId) : IQuery<IReadOnlyList<TransferResponse>>;

internal sealed class GetTransfersQueryHandler(ITransferRepository transferRepository)
    : IQueryHandler<GetTransfersQuery, IReadOnlyList<TransferResponse>>
{
    public async Task<Result<IReadOnlyList<TransferResponse>>> Handle(
        GetTransfersQuery request,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<Transfer> transfers = await transferRepository.ListAsync(request.AccountId, cancellationToken);
        return transfers.Select(transfer => TransferMapper.ToResponse(transfer, replayed: false)).ToList();
    }
}
