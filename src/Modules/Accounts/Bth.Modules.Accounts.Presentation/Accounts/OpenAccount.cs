using Bth.Common.Domain;
using Bth.Common.Presentation.Endpoints;
using Bth.Common.Presentation.Results;
using Bth.Modules.Accounts.Application.Accounts.OpenAccount;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Bth.Modules.Accounts.Presentation.Accounts;

internal sealed class OpenAccount : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/accounts", async (OpenAccountRequest request, ISender sender) =>
        {
            Result<Guid> result = await sender.Send(new OpenAccountCommand(
                request.HolderName,
                request.Document,
                request.InitialBalance,
                request.OverdraftLimit,
                request.DayMaxAmountPerHour ?? 5000m,
                request.DayMaxAttemptsPerHour ?? 5,
                request.NightMaxAmountPerHour ?? 1000m,
                request.NightMaxAttemptsPerHour ?? 3));

            return result.Match(
                id => Results.Created($"/api/accounts/{id}", new { id }),
                ApiResults.Problem);
        })
        .WithTags("Accounts")
        .WithSummary("Abre uma conta para uma pessoa");
    }

    internal sealed record OpenAccountRequest(
        string HolderName,
        string Document,
        decimal InitialBalance,
        decimal OverdraftLimit,
        decimal? DayMaxAmountPerHour,
        int? DayMaxAttemptsPerHour,
        decimal? NightMaxAmountPerHour,
        int? NightMaxAttemptsPerHour);
}
