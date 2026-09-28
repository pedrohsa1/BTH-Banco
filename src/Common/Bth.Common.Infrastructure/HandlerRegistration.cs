using System.Reflection;
using Bth.Common.Application.EventBus;
using Bth.Common.Application.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Bth.Common.Infrastructure;

public static class HandlerRegistration
{
    public static IServiceCollection AddDomainEventHandlers(this IServiceCollection services, Assembly assembly)
    {
        IEnumerable<Type> handlerTypes = assembly.GetTypes()
            .Where(type => type is { IsAbstract: false, IsInterface: false } &&
                           type.IsAssignableTo(typeof(IDomainEventHandler)));

        foreach (Type handlerType in handlerTypes)
        {
            services.TryAddScoped(handlerType);
        }

        return services;
    }

    public static IServiceCollection AddIntegrationEventHandlers(this IServiceCollection services, Assembly assembly)
    {
        foreach (Type handlerType in assembly.GetTypes().Where(type => type is { IsAbstract: false, IsInterface: false }))
        {
            Type? serviceType = handlerType.GetInterfaces().FirstOrDefault(type =>
                type.IsGenericType &&
                type.GetGenericTypeDefinition() == typeof(IIntegrationEventHandler<>));

            if (serviceType is not null)
            {
                services.AddScoped(serviceType, handlerType);
            }
        }

        return services;
    }
}
