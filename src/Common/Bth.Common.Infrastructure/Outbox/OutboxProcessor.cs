using System.Data;
using System.Reflection;
using System.Text.Json;
using Bth.Common.Application.Clock;
using Bth.Common.Application.Data;
using Bth.Common.Application.Messaging;
using Bth.Common.Application.Outbox;
using Bth.Common.Domain;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Bth.Common.Infrastructure.Outbox;

internal sealed class OutboxProcessor(
    IDbConnectionFactory dbConnectionFactory,
    IServiceScopeFactory serviceScopeFactory,
    IDateTimeProvider dateTimeProvider,
    ILogger<OutboxProcessor> logger) : IOutboxProcessor
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private static readonly HashSet<string> AllowedSchemas = ["accounts", "transfers"];

    public async Task ProcessAsync(
        string schema,
        Assembly handlersAssembly,
        CancellationToken cancellationToken = default)
    {
        if (!AllowedSchemas.Contains(schema))
        {
            throw new ArgumentException($"Schema de outbox não permitido: {schema}", nameof(schema));
        }

        await using System.Data.Common.DbConnection connection =
            await dbConnectionFactory.OpenConnectionAsync(cancellationToken);
        await using System.Data.Common.DbTransaction transaction =
            await connection.BeginTransactionAsync(cancellationToken);

        IEnumerable<OutboxRow> messages = await connection.QueryAsync<OutboxRow>(
            $"""
             SELECT id AS Id, type AS Type, content AS Content
             FROM {schema}.outbox_messages
             WHERE processed_on_utc IS NULL
             ORDER BY occurred_on_utc
             LIMIT 50
             FOR UPDATE SKIP LOCKED
             """,
            transaction: transaction);

        foreach (OutboxRow message in messages)
        {
            string? error = null;

            try
            {
                Type? eventType = Type.GetType(message.Type, throwOnError: false);
                if (eventType is null || !typeof(IDomainEvent).IsAssignableFrom(eventType))
                {
                    throw new InvalidOperationException($"Tipo de evento de domínio desconhecido: {message.Type}");
                }

                var domainEvent = (IDomainEvent)JsonSerializer.Deserialize(message.Content, eventType, SerializerOptions)!;

                using IServiceScope scope = serviceScopeFactory.CreateScope();
                IEnumerable<IDomainEventHandler> handlers = DomainEventHandlersFactory.GetHandlers(
                    domainEvent.GetType(),
                    scope.ServiceProvider,
                    handlersAssembly);

                foreach (IDomainEventHandler handler in handlers)
                {
                    await handler.Handle(domainEvent, cancellationToken);
                }
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Falha ao processar outbox {Schema} {MessageId}", schema, message.Id);
                error = exception.ToString();
            }

            await connection.ExecuteAsync(
                $"""
                 UPDATE {schema}.outbox_messages
                 SET processed_on_utc = @ProcessedOnUtc,
                     error = @Error
                 WHERE id = @Id
                 """,
                new
                {
                    message.Id,
                    ProcessedOnUtc = dateTimeProvider.UtcNow,
                    Error = error
                },
                transaction);
        }

        await transaction.CommitAsync(cancellationToken);
    }

    private sealed record OutboxRow(Guid Id, string Type, string Content);
}
