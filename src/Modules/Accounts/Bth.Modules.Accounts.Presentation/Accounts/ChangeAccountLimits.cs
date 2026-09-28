using Bth.Common.Domain;
using Bth.Common.Presentation.Endpoints;
using Bth.Common.Presentation.Results;
using Bth.Modules.Accounts.Application.Accounts.ChangeAccountLimits;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Bth.Modules.Accounts.Presentation.Accounts;

internal sealed class ChangeAccountLimits : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch("api/accounts/{id:guid}/limits", async (Guid id, ChangeLimitsRequest request, ISender sender) =>
        {
            Result result = await sender.Send(new ChangeAccountLimitsCommand(
                id,
                request.DayMaxAmountPerHour,
                request.DayMaxAttemptsPerHour,
                request.NightMaxAmountPerHour,
                request.NightMaxAttemptsPerHour));

            return result.Match(Results.NoContent, ApiResults.Problem);
        })
        .WithTags("Accounts")
        .WithSummary("Altera os limites por hora de uma conta");
    }

    internal sealed record ChangeLimitsRequest(
        decimal DayMaxAmountPerHour,
        int DayMaxAttemptsPerHour,
        decimal NightMaxAmountPerHour,
        int NightMaxAttemptsPerHour);
}
