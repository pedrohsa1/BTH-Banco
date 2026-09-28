using Bth.Common.Infrastructure.Outbox;
using Bth.Modules.Transfers.Domain.Transfers;
using Microsoft.EntityFrameworkCore;

namespace Bth.Modules.Transfers.Infrastructure.Database;

public sealed class TransfersDbContext(DbContextOptions<TransfersDbContext> options) : DbContext(options)
{
    public DbSet<Transfer> Transfers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Transfers);
        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
        modelBuilder.ApplyConfiguration(new TransferConfiguration());
    }
}
