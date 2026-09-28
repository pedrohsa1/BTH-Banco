using Bth.Modules.Transfers.Application.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Bth.Composition;

public static class CompositionModule
{
    public static IServiceCollection AddBankingComposition(this IServiceCollection services)
    {
        services.AddScoped<ITransferExecutor, TransferExecutor>();
        return services;
    }
}

public sealed class NoopTransferScheduler : ITransferScheduler
{
    public void Schedule(Guid transferId, DateTime scheduledAtUtc)
    {
    }
}
