using Bth.Common.Application.Clock;
using Bth.Modules.Accounts.Domain.Accounts;
using Bth.Modules.Accounts.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bth.Modules.Accounts.Infrastructure.Accounts;

public static class AccountSeeder
{
    public static readonly Guid AnaId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid BrunoId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid CarlaId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    public static readonly Guid DiegoId = Guid.Parse("44444444-4444-4444-4444-444444444444");

    public static async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        var configuration = services.GetRequiredService<IConfiguration>();
        if (!configuration.GetValue("Seed:Enabled", true))
        {
            return;
        }

        var dbContext = services.GetRequiredService<AccountsDbContext>();
        if (await dbContext.Accounts.AnyAsync(cancellationToken))
        {
            return;
        }

        var clock = services.GetRequiredService<IDateTimeProvider>();
        DateTime utcNow = clock.UtcNow;

        Account ana = Open(AnaId, "Ana Souza", "11111111111", 500m, 1000m, utcNow);
        Account bruno = Open(BrunoId, "Bruno Lima", "22222222222", 2500m, 0m, utcNow);
        Account carla = Open(CarlaId, "Carla Dias", "33333333333", 800m, 500m, utcNow);
        Account diego = Open(DiegoId, "Diego Rocha", "44444444444", 1500m, 0m, utcNow);
        diego.ChangeStatus(AccountStatus.Inactive);

        dbContext.Accounts.AddRange(ana, bruno, carla, diego);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static Account Open(
        Guid id,
        string name,
        string document,
        decimal balance,
        decimal overdraft,
        DateTime utcNow)
    {
        var result = Account.Open(name, document, balance, overdraft, 5000m, 5, 1000m, 3, utcNow, id);
        return result.Value;
    }
}
