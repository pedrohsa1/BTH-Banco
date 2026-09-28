using Bth.Common.Application.EventBus;
using Bth.Common.Application.Notifications;
using Bth.Modules.Accounts.IntegrationEvents;

namespace Bth.Modules.Accounts.Presentation.Accounts;

internal sealed class AccountBalanceChangedIntegrationEventHandler(IBankingNotifier notifier)
    : IntegrationEventHandler<AccountBalanceChangedIntegrationEvent>
{
    public override Task Handle(
        AccountBalanceChangedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default) =>
        notifier.AccountChangedAsync(
            new AccountNotification(
                integrationEvent.AccountId,
                integrationEvent.HolderName,
                integrationEvent.Balance,
                integrationEvent.OverdraftLimit,
                integrationEvent.Balance + integrationEvent.OverdraftLimit,
                integrationEvent.Status),
            cancellationToken);
}
