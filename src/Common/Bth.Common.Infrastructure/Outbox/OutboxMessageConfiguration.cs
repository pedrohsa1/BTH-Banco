using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bth.Common.Infrastructure.Outbox;

public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages");

        builder.HasKey(outboxMessage => outboxMessage.Id);

        builder.Property(outboxMessage => outboxMessage.Type).HasMaxLength(500);

        builder.Property(outboxMessage => outboxMessage.Content).HasColumnType("jsonb");

        builder.Property(outboxMessage => outboxMessage.Error).HasMaxLength(4000);
    }
}
