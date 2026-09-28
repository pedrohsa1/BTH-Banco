using Bth.Common.Application.EventBus;
using Bth.Common.Application.Messaging;
using Bth.Modules.Transfers.Domain.Transfers;
using Bth.Modules.Transfers.IntegrationEvents;

namespace Bth.Modules.Transfers.Application.Transfers;

internal sealed class TransferChangedDomainEventHandler(IEventBus eventBus)
    : DomainEventHandler<TransferChangedDomainEvent>
{
    public override Task Handle(TransferChangedDomainEvent domainEvent, CancellationToken cancellationToken = default) =>
        eventBus.PublishAsync(
            new TransferStatusChangedIntegrationEvent(
                Guid.NewGuid(),
                domainEvent.OccurredOnUtc,
                domainEvent.TransferId,
                domainEvent.SourceAccountId,
                domainEvent.DestinationAccountId,
                domainEvent.Amount,
                domainEvent.Status,
                domainEvent.ScheduledAtUtc,
                domainEvent.CreatedAtUtc,
                domainEvent.ProcessedAtUtc,
                domainEvent.FailureReason),
            cancellationToken);
}
