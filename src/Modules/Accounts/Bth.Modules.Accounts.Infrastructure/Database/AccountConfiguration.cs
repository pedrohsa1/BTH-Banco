using Bth.Modules.Accounts.Domain.Accounts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bth.Modules.Accounts.Infrastructure.Database;

internal sealed class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.HasKey(account => account.Id);

        builder.Property(account => account.HolderName).HasMaxLength(200).IsRequired();
        builder.Property(account => account.Document).HasMaxLength(20).IsRequired();
        builder.HasIndex(account => account.Document).IsUnique();

        builder.Property(account => account.Balance).HasPrecision(18, 2);
        builder.Property(account => account.OverdraftLimit).HasPrecision(18, 2);
        builder.Property(account => account.DayMaxAmountPerHour).HasPrecision(18, 2);
        builder.Property(account => account.NightMaxAmountPerHour).HasPrecision(18, 2);
        builder.Property(account => account.Status).HasConversion<string>().HasMaxLength(20);

        builder.Ignore(account => account.AvailableFunds);
    }
}
