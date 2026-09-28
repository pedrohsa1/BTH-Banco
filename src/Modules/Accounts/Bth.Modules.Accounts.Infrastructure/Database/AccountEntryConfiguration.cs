using Bth.Modules.Accounts.Domain.Accounts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bth.Modules.Accounts.Infrastructure.Database;

internal sealed class AccountEntryConfiguration : IEntityTypeConfiguration<AccountEntry>
{
    public void Configure(EntityTypeBuilder<AccountEntry> builder)
    {
        builder.HasKey(entry => entry.Id);

        builder.Property(entry => entry.Amount).HasPrecision(18, 2);
        builder.Property(entry => entry.BalanceAfter).HasPrecision(18, 2);
        builder.Property(entry => entry.Type).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(entry => new { entry.TransferId, entry.Type }).IsUnique();
        builder.HasIndex(entry => entry.AccountId);
    }
}
