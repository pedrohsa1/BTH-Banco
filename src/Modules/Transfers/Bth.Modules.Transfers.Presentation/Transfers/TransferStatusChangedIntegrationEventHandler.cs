using Bth.Common.Application.EventBus;
using Bth.Common.Application.Notifications;
using Bth.Modules.Transfers.IntegrationEvents;

namespace Bth.Modules.Transfers.Presentation.Transfers;

internal sealed class TransferStatusChangedIntegrationEventHandler(IBankingNotifier notifier)
    : IntegrationEventHandler<TransferStatusChangedIntegrationEvent>
{
    public override Task Handle(
        TransferStatusChangedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default) =>
        notifier.TransferChangedAsync(
            new TransferNotification(
                integrationEvent.TransferId,
                integrationEvent.SourceAccountId,
                integrationEvent.DestinationAccountId,
                integrationEvent.Amount,
                integrationEvent.Status,
                integrationEvent.ScheduledAtUtc,
                integrationEvent.CreatedAtUtc,
                integrationEvent.ProcessedAtUtc,
                integrationEvent.FailureReason),
            cancellationToken);
}
