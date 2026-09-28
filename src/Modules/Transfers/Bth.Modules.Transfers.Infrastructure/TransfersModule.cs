using Bth.Common.Infrastructure;
using Bth.Common.Infrastructure.Outbox;
using Bth.Common.Presentation.Endpoints;
using Bth.Modules.Transfers.Application.Transfers;
using Bth.Modules.Transfers.Domain.Transfers;
using Bth.Modules.Transfers.Infrastructure.Database;
using Bth.Modules.Transfers.Infrastructure.Transfers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bth.Modules.Transfers.Infrastructure;

public static class TransfersModule
{
    public static IServiceCollection AddTransfersModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<TransfersDbContext>((serviceProvider, options) =>
            options
                .UseNpgsql(
                    configuration.GetConnectionString("Database"),
                    npgsql => npgsql.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Transfers))
                .UseSnakeCaseNamingConvention()
                .AddInterceptors(serviceProvider.GetRequiredService<InsertOutboxMessagesInterceptor>()));

        services.Configure<LimitOptions>(configuration.GetSection(LimitOptions.SectionName));
        services.AddScoped<ITransferRepository, TransferRepository>();
        services.AddDomainEventHandlers(Application.AssemblyReference.Assembly);
        services.AddIntegrationEventHandlers(Presentation.AssemblyReference.Assembly);
        services.AddEndpoints(Presentation.AssemblyReference.Assembly);

        return services;
    }
}
