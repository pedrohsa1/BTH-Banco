using System.Text.Json;
using Bth.Common.Application.EventBus;
using Bth.Common.Infrastructure.EventBus;
using Bth.Modules.Transfers.Application.Abstractions;
using Hangfire;
using Hangfire.Dashboard;

namespace Bth.Api.Infrastructure;

public sealed class HangfireEventBus(IBackgroundJobClient jobs) : IEventBus
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public Task PublishAsync<T>(T integrationEvent, CancellationToken cancellationToken = default)
        where T : IIntegrationEvent
    {
        string json = JsonSerializer.Serialize(integrationEvent, integrationEvent.GetType(), SerializerOptions);
        jobs.Enqueue<IntegrationEventJob>(job => job.Dispatch(integrationEvent.GetType().Name, json));
        return Task.CompletedTask;
    }
}

public sealed class IntegrationEventJob(IntegrationEventDispatcher dispatcher)
{
    public Task Dispatch(string typeName, string json) =>
        dispatcher.DispatchAsync(typeName, json, CancellationToken.None);
}

public sealed class HangfireTransferScheduler(IBackgroundJobClient jobs) : ITransferScheduler
{
    public void Schedule(Guid transferId, DateTime scheduledAtUtc)
    {
        jobs.Schedule<ExecuteScheduledTransferJob>(
            job => job.Run(transferId),
            new DateTimeOffset(DateTime.SpecifyKind(scheduledAtUtc, DateTimeKind.Utc)));
    }
}

public sealed class ExecuteScheduledTransferJob(ITransferExecutor executor)
{
    public Task Run(Guid transferId) => executor.ExecuteDueAsync(transferId, CancellationToken.None);
}

public sealed class DispatchDueTransfersJob(ITransferExecutor executor)
{
    public Task Run() => executor.DispatchDueAsync(CancellationToken.None);
}

internal sealed class HangfireDashboardAuthorization : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context) => true;
}
