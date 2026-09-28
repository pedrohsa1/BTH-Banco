using System.Text.Json;
using Bth.Common.Application.EventBus;

namespace Bth.Common.Infrastructure.EventBus;

internal sealed class InlineEventBus(IntegrationEventDispatcher dispatcher) : IEventBus
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public Task PublishAsync<T>(T integrationEvent, CancellationToken cancellationToken = default)
        where T : IIntegrationEvent
    {
        string json = JsonSerializer.Serialize(integrationEvent, integrationEvent.GetType(), SerializerOptions);
        return dispatcher.DispatchAsync(integrationEvent.GetType().Name, json, cancellationToken);
    }
}
