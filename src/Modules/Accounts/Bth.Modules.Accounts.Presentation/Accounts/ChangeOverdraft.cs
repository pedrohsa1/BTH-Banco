using Bth.Common.Domain;
using Bth.Common.Presentation.Endpoints;
using Bth.Common.Presentation.Results;
using Bth.Modules.Accounts.Application.Accounts.ChangeOverdraft;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Bth.Modules.Accounts.Presentation.Accounts;

internal sealed class ChangeOverdraft : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch("api/accounts/{id:guid}/overdraft", async (Guid id, ChangeOverdraftRequest request, ISender sender) =>
        {
            Result result = await sender.Send(new ChangeOverdraftCommand(id, request.OverdraftLimit));
            return result.Match(Results.NoContent, ApiResults.Problem);
        })
        .WithTags("Accounts")
        .WithSummary("Define o limite de cheque especial de uma conta");
    }

    internal sealed record ChangeOverdraftRequest(decimal OverdraftLimit);
}
