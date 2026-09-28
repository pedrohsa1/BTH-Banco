using Bth.Common.Domain;
using Bth.Common.Presentation.Endpoints;
using Bth.Common.Presentation.Results;
using Bth.Modules.Accounts.Application.Accounts.ChangeAccountStatus;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Bth.Modules.Accounts.Presentation.Accounts;

internal sealed class ChangeAccountStatus : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch("api/accounts/{id:guid}/status", async (Guid id, ChangeStatusRequest request, ISender sender) =>
        {
            Result result = await sender.Send(new ChangeAccountStatusCommand(id, request.Status));
            return result.Match(Results.NoContent, ApiResults.Problem);
        })
        .WithTags("Accounts")
        .WithSummary("Ativa, bloqueia ou inativa uma conta");
    }

    internal sealed record ChangeStatusRequest(string Status);
}
