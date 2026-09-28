using Bth.Common.Application.Clock;
using Bth.Common.Application.Data;
using Bth.Common.Application.EventBus;
using Bth.Common.Application.Outbox;
using Bth.Common.Infrastructure.Clock;
using Bth.Common.Infrastructure.Data;
using Bth.Common.Infrastructure.EventBus;
using Bth.Common.Infrastructure.Outbox;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Bth.Common.Infrastructure;

public static class InfrastructureConfiguration
{
    public static IServiceCollection AddCommonInfrastructure(this IServiceCollection services, bool useInlineEventBus)
    {
        services.TryAddSingleton<InsertOutboxMessagesInterceptor>();
        services.TryAddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddScoped<IDbConnectionFactory, DbConnectionFactory>();
        services.AddScoped<IOutboxProcessor, OutboxProcessor>();
        services.AddScoped<IntegrationEventDispatcher>();

        if (useInlineEventBus)
        {
            services.AddScoped<IEventBus, InlineEventBus>();
        }

        return services;
    }
}
