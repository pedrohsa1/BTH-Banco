using Bth.Api.Infrastructure;
using Bth.Api.Realtime;
using Bth.Common.Application;
using Bth.Common.Application.EventBus;
using Bth.Common.Infrastructure;
using Bth.Common.Infrastructure.EventBus;
using Bth.Common.Presentation.Endpoints;
using Bth.Composition;
using Bth.Modules.Accounts.Infrastructure;
using Bth.Modules.Accounts.Infrastructure.Accounts;
using Bth.Modules.Accounts.Infrastructure.Database;
using Bth.Modules.Accounts.Infrastructure.Outbox;
using Bth.Modules.Accounts.IntegrationEvents;
using Bth.Modules.Transfers.Application.Abstractions;
using Bth.Modules.Transfers.Infrastructure;
using Bth.Modules.Transfers.Infrastructure.Database;
using Bth.Modules.Transfers.Infrastructure.Outbox;
using Bth.Modules.Transfers.IntegrationEvents;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "Brasil Terrenos Holding — Transferências",
        Version = "v1",
        Description = "API de transferências entre contas."
    });
});

bool hangfireEnabled = builder.Configuration.GetValue("Hangfire:Enabled", true);
string connectionString = builder.Configuration.GetConnectionString("Database")
    ?? throw new InvalidOperationException("Connection string 'Database' não configurada.");

builder.Services.AddApplication(
[
    Bth.Modules.Accounts.Application.AssemblyReference.Assembly,
    Bth.Modules.Transfers.Application.AssemblyReference.Assembly
]);

builder.Services.AddCommonInfrastructure(useInlineEventBus: !hangfireEnabled);
builder.Services.AddAccountsModule(builder.Configuration);
builder.Services.AddTransfersModule(builder.Configuration);
builder.Services.AddBankingComposition();
builder.Services.AddSignalR();
builder.Services.AddScoped<Bth.Common.Application.Notifications.IBankingNotifier, SignalRBankingNotifier>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("frontend", policy =>
        policy.WithOrigins(
                builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ??
                ["http://localhost:5173", "http://localhost:8080"])
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

if (hangfireEnabled)
{
    builder.Services.AddHangfire(config => config
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UsePostgreSqlStorage(options => options.UseNpgsqlConnection(connectionString, _ => { })));

    builder.Services.AddHangfireServer();
    builder.Services.AddScoped<IEventBus, HangfireEventBus>();
    builder.Services.AddScoped<ITransferScheduler, HangfireTransferScheduler>();
}
else
{
    builder.Services.AddScoped<ITransferScheduler, NoopTransferScheduler>();
}

IntegrationEventDispatcher.Register(typeof(AccountBalanceChangedIntegrationEvent).Assembly);
IntegrationEventDispatcher.Register(typeof(TransferStatusChangedIntegrationEvent).Assembly);

WebApplication app = builder.Build();

app.UseExceptionHandler(handler =>
{
    handler.Run(async context =>
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(new
        {
            title = "Falha no servidor",
            detail = "Ocorreu um erro inesperado."
        });
    });
});

app.UseCors("frontend");
app.UseSwagger();
app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", "Transferências v1"));

using (IServiceScope scope = app.Services.CreateScope())
{
    AccountsDbContext accounts = scope.ServiceProvider.GetRequiredService<AccountsDbContext>();
    TransfersDbContext transfers = scope.ServiceProvider.GetRequiredService<TransfersDbContext>();
    await accounts.Database.MigrateAsync();
    await transfers.Database.MigrateAsync();
    await accounts.Database.ExecuteSqlRawAsync(
        "UPDATE accounts.accounts SET status = 'Inactive' WHERE status = 'Blocked'");
    await AccountSeeder.SeedAsync(scope.ServiceProvider);
}

if (hangfireEnabled)
{
    app.UseHangfireDashboard("/hangfire", new DashboardOptions
    {
        Authorization = [new HangfireDashboardAuthorization()]
    });
    RecurringJob.AddOrUpdate<ProcessAccountsOutboxJob>(
        "accounts-outbox",
        job => job.Run(),
        Cron.Minutely);
    RecurringJob.AddOrUpdate<ProcessTransfersOutboxJob>(
        "transfers-outbox",
        job => job.Run(),
        Cron.Minutely);
    RecurringJob.AddOrUpdate<DispatchDueTransfersJob>(
        "dispatch-due-transfers",
        job => job.Run(),
        Cron.Minutely);
}

app.MapGet("health", () => Results.Ok(new { status = "ok" }));
app.MapHub<BankingHub>("/hubs/banking");
app.MapEndpoints();

app.Run();

public partial class Program;
