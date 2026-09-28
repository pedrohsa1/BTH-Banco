using Bth.Common.Domain;
using Bth.Common.Presentation.Endpoints;
using Bth.Common.Presentation.Results;
using Bth.Modules.Transfers.Application.Transfers;
using Bth.Modules.Transfers.Application.Transfers.CreateImmediateTransfer;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Bth.Modules.Transfers.Presentation.Transfers;

internal sealed class CreateImmediateTransfer : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/transfers", async (TransferRequest request, HttpContext httpContext, ISender sender) =>
        {
            Result<TransferResponse> result = await sender.Send(new CreateImmediateTransferCommand(
                request.SourceAccountId,
                request.DestinationAccountId,
                request.Amount,
                httpContext.Request.Headers["Idempotency-Key"].FirstOrDefault()));

            return ToHttpResult(result);
        })
        .WithTags("Transfers")
        .WithSummary("Realiza uma transferência imediata");
    }

    internal static IResult ToHttpResult(Result<TransferResponse> result)
    {
        if (result.IsFailure)
        {
            return ApiResults.Problem(result);
        }

        TransferResponse transfer = result.Value;

        if (transfer.Replayed)
        {
            return Results.Ok(transfer);
        }

        if (transfer.Status == "Failed")
        {
            return Results.UnprocessableEntity(transfer);
        }

        return Results.Created($"/api/transfers/{transfer.Id}", transfer);
    }
}

internal sealed record TransferRequest(Guid SourceAccountId, Guid DestinationAccountId, decimal Amount);
