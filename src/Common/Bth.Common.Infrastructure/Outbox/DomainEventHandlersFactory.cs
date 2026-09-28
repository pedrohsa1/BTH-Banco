using System.Collections.Concurrent;
using System.Reflection;
using Bth.Common.Application.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace Bth.Common.Infrastructure.Outbox;

public static class DomainEventHandlersFactory
{
    private static readonly ConcurrentDictionary<string, Type[]> HandlersDictionary = new();

    public static IEnumerable<IDomainEventHandler> GetHandlers(
        Type type,
        IServiceProvider serviceProvider,
        Assembly assembly)
    {
        Type[] handlerTypes = HandlersDictionary.GetOrAdd(
            $"{assembly.GetName().Name}:{type.Name}",
            _ => assembly.GetTypes()
                .Where(candidate => candidate.IsAssignableTo(typeof(IDomainEventHandler<>).MakeGenericType(type)))
                .ToArray());

        List<IDomainEventHandler> handlers = [];

        foreach (Type handlerType in handlerTypes)
        {
            object handler = serviceProvider.GetRequiredService(handlerType);
            handlers.Add((IDomainEventHandler)handler);
        }

        return handlers;
    }
}
