using Bth.Common.Domain;
using Bth.Common.Presentation.Endpoints;
using Bth.Common.Presentation.Results;
using Bth.Modules.Transfers.Application.Transfers;
using Bth.Modules.Transfers.Application.Transfers.ScheduleTransfer;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Bth.Modules.Transfers.Presentation.Transfers;

internal sealed class ScheduleTransfer : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/transfers/scheduled", async (ScheduleRequest request, HttpContext httpContext, ISender sender) =>
        {
            Result<TransferResponse> result = await sender.Send(new ScheduleTransferCommand(
                request.SourceAccountId,
                request.DestinationAccountId,
                request.Amount,
                request.ScheduledAtUtc,
                httpContext.Request.Headers["Idempotency-Key"].FirstOrDefault()));

            return CreateImmediateTransfer.ToHttpResult(result);
        })
        .WithTags("Transfers")
        .WithSummary("Agenda uma transferência para uma data futura");
    }

    internal sealed record ScheduleRequest(
        Guid SourceAccountId,
        Guid DestinationAccountId,
        decimal Amount,
        DateTime ScheduledAtUtc);
}
