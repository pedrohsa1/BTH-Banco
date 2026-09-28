using Bth.Common.Domain;
using Bth.Common.Presentation.Endpoints;
using Bth.Common.Presentation.Results;
using Bth.Modules.Accounts.Application.Accounts;
using Bth.Modules.Accounts.Application.Accounts.GetAccount;
using Bth.Modules.Accounts.Application.Accounts.GetAccounts;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Bth.Modules.Accounts.Presentation.Accounts;

internal sealed class GetAccount : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/accounts/{id:guid}", async (Guid id, ISender sender) =>
        {
            Result<AccountResponse> result = await sender.Send(new GetAccountQuery(id));
            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .WithTags("Accounts")
        .WithSummary("Consulta uma conta");
    }
}

internal sealed class GetAccounts : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/accounts", async (ISender sender) =>
        {
            Result<IReadOnlyList<AccountResponse>> result = await sender.Send(new GetAccountsQuery());
            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .WithTags("Accounts")
        .WithSummary("Lista as contas");
    }
}
