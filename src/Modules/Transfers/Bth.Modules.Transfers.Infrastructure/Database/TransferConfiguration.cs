using Bth.Modules.Transfers.Domain.Transfers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bth.Modules.Transfers.Infrastructure.Database;

internal sealed class TransferConfiguration : IEntityTypeConfiguration<Transfer>
{
    public void Configure(EntityTypeBuilder<Transfer> builder)
    {
        builder.HasKey(transfer => transfer.Id);

        builder.Property(transfer => transfer.Amount).HasPrecision(18, 2);
        builder.Property(transfer => transfer.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(transfer => transfer.FailureReason).HasMaxLength(500);
        builder.Property(transfer => transfer.IdempotencyKey).HasMaxLength(100);
        builder.Property(transfer => transfer.RequestHash).HasMaxLength(128);

        builder.HasIndex(transfer => transfer.IdempotencyKey)
            .IsUnique()
            .HasFilter("idempotency_key IS NOT NULL");

        builder.HasIndex(transfer => new { transfer.SourceAccountId, transfer.AttemptedAtUtc });
        builder.HasIndex(transfer => new { transfer.Status, transfer.ScheduledAtUtc });
    }
}
