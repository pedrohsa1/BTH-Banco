using Bth.Common.Domain;
using Bth.Common.Presentation.Endpoints;
using Bth.Common.Presentation.Results;
using Bth.Modules.Transfers.Application.Transfers;
using Bth.Modules.Transfers.Application.Transfers.CancelTransfer;
using Bth.Modules.Transfers.Application.Transfers.GetTransfer;
using Bth.Modules.Transfers.Application.Transfers.GetTransfers;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Bth.Modules.Transfers.Presentation.Transfers;

internal sealed class GetTransfer : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/transfers/{id:guid}", async (Guid id, ISender sender) =>
        {
            Result<TransferResponse> result = await sender.Send(new GetTransferQuery(id));
            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .WithTags("Transfers")
        .WithSummary("Consulta o status e os dados de uma transferência");
    }
}

internal sealed class GetTransfers : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/transfers", async (Guid? accountId, ISender sender) =>
        {
            Result<IReadOnlyList<TransferResponse>> result = await sender.Send(new GetTransfersQuery(accountId));
            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .WithTags("Transfers")
        .WithSummary("Lista transferências, opcionalmente filtradas por conta");
    }
}

internal sealed class CancelTransfer : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/transfers/{id:guid}/cancel", async (Guid id, ISender sender) =>
        {
            Result<TransferResponse> result = await sender.Send(new CancelTransferCommand(id));
            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .WithTags("Transfers")
        .WithSummary("Cancela uma transferência agendada");
    }
}
