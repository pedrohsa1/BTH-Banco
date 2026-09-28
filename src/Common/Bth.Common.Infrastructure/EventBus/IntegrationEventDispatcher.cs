using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;
using Bth.Common.Application.EventBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Bth.Common.Infrastructure.EventBus;

public sealed class IntegrationEventDispatcher(
    IServiceScopeFactory serviceScopeFactory,
    ILogger<IntegrationEventDispatcher> logger)
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
    private static readonly ConcurrentDictionary<string, Type> Types = new();

    public static void Register(Assembly assembly)
    {
        foreach (Type type in assembly.GetTypes().Where(type =>
                     type is { IsAbstract: false, IsInterface: false } &&
                     type.IsAssignableTo(typeof(IIntegrationEvent))))
        {
            Types[type.Name] = type;
        }
    }

    public async Task DispatchAsync(string typeName, string json, CancellationToken cancellationToken = default)
    {
        if (!Types.TryGetValue(typeName, out Type? eventType))
        {
            eventType = AppDomain.CurrentDomain.GetAssemblies()
                .Select(assembly => assembly.GetType(typeName, throwOnError: false))
                .FirstOrDefault(type => type is not null);

            if (eventType is null)
            {
                logger.LogWarning("Evento de integração desconhecido: {TypeName}", typeName);
                return;
            }
        }

        var integrationEvent = (IIntegrationEvent)JsonSerializer.Deserialize(json, eventType, SerializerOptions)!;

        using IServiceScope scope = serviceScopeFactory.CreateScope();

        Type handlerInterface = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);
        IEnumerable<object?> handlers = scope.ServiceProvider.GetServices(handlerInterface);

        foreach (object? handler in handlers)
        {
            if (handler is IIntegrationEventHandler integrationEventHandler)
            {
                await integrationEventHandler.Handle(integrationEvent, cancellationToken);
            }
        }
    }
}
