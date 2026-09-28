using Bth.Common.Infrastructure.Outbox;
using Bth.Modules.Accounts.Application.Abstractions.Data;
using Bth.Modules.Accounts.Domain.Accounts;
using Microsoft.EntityFrameworkCore;

namespace Bth.Modules.Accounts.Infrastructure.Database;

public sealed class AccountsDbContext(DbContextOptions<AccountsDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public DbSet<Account> Accounts { get; set; }

    public DbSet<AccountEntry> AccountEntries { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Accounts);
        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
        modelBuilder.ApplyConfiguration(new AccountConfiguration());
        modelBuilder.ApplyConfiguration(new AccountEntryConfiguration());
    }
}
