using Bth.Common.Infrastructure;
using Bth.Common.Infrastructure.Outbox;
using Bth.Common.Presentation.Endpoints;
using Bth.Modules.Accounts.Application.Abstractions.Data;
using Bth.Modules.Accounts.Contracts;
using Bth.Modules.Accounts.Domain.Accounts;
using Bth.Modules.Accounts.Infrastructure.Accounts;
using Bth.Modules.Accounts.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bth.Modules.Accounts.Infrastructure;

public static class AccountsModule
{
    public static IServiceCollection AddAccountsModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AccountsDbContext>((serviceProvider, options) =>
            options
                .UseNpgsql(
                    configuration.GetConnectionString("Database"),
                    npgsql => npgsql.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Accounts))
                .UseSnakeCaseNamingConvention()
                .AddInterceptors(serviceProvider.GetRequiredService<InsertOutboxMessagesInterceptor>()));

        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<IAccountReader, AccountReader>();
        services.AddScoped<IUnitOfWork>(serviceProvider => serviceProvider.GetRequiredService<AccountsDbContext>());
        services.AddDomainEventHandlers(Application.AssemblyReference.Assembly);
        services.AddIntegrationEventHandlers(Presentation.AssemblyReference.Assembly);
        services.AddEndpoints(Presentation.AssemblyReference.Assembly);

        return services;
    }
}
