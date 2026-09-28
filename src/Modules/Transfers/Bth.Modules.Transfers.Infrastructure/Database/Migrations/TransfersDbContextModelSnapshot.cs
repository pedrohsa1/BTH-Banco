using System;
using Bth.Modules.Transfers.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Bth.Modules.Transfers.Infrastructure.Database.Migrations
{
    [DbContext(typeof(TransfersDbContext))]
    partial class TransfersDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("transfers")
                .HasAnnotation("ProductVersion", "10.0.12")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Bth.Common.Infrastructure.Outbox.OutboxMessage", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasColumnType("jsonb")
                        .HasColumnName("content");

                    b.Property<string>("Error")
                        .HasMaxLength(4000)
                        .HasColumnType("character varying(4000)")
                        .HasColumnName("error");

                    b.Property<DateTime>("OccurredOnUtc")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("occurred_on_utc");

                    b.Property<DateTime?>("ProcessedOnUtc")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("processed_on_utc");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)")
                        .HasColumnName("type");

                    b.HasKey("Id")
                        .HasName("pk_outbox_messages");

                    b.ToTable("outbox_messages", "transfers");
                });

            modelBuilder.Entity("Bth.Modules.Transfers.Domain.Transfers.Transfer", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<decimal>("Amount")
                        .HasPrecision(18, 2)
                        .HasColumnType("numeric(18,2)")
                        .HasColumnName("amount");

                    b.Property<DateTime?>("AttemptedAtUtc")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("attempted_at_utc");

                    b.Property<DateTime>("CreatedAtUtc")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at_utc");

                    b.Property<Guid>("DestinationAccountId")
                        .HasColumnType("uuid")
                        .HasColumnName("destination_account_id");

                    b.Property<string>("FailureReason")
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)")
                        .HasColumnName("failure_reason");

                    b.Property<string>("IdempotencyKey")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("idempotency_key");

                    b.Property<DateTime?>("ProcessedAtUtc")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("processed_at_utc");

                    b.Property<string>("RequestHash")
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)")
                        .HasColumnName("request_hash");

                    b.Property<DateTime?>("ScheduledAtUtc")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("scheduled_at_utc");

                    b.Property<Guid>("SourceAccountId")
                        .HasColumnType("uuid")
                        .HasColumnName("source_account_id");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("character varying(20)")
                        .HasColumnName("status");

                    b.HasKey("Id")
                        .HasName("pk_transfers");

                    b.HasIndex("IdempotencyKey")
                        .IsUnique()
                        .HasDatabaseName("ix_transfers_idempotency_key")
                        .HasFilter("idempotency_key IS NOT NULL");

                    b.HasIndex("SourceAccountId", "AttemptedAtUtc")
                        .HasDatabaseName("ix_transfers_source_account_id_attempted_at_utc");

                    b.HasIndex("Status", "ScheduledAtUtc")
                        .HasDatabaseName("ix_transfers_status_scheduled_at_utc");

                    b.ToTable("transfers", "transfers");
                });
#pragma warning restore 612, 618
        }
    }
}
