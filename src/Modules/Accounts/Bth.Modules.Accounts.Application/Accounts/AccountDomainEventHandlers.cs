using Bth.Common.Application.EventBus;
using Bth.Common.Application.Messaging;
using Bth.Common.Application.Notifications;
using Bth.Modules.Accounts.Domain.Accounts;
using Bth.Modules.Accounts.IntegrationEvents;

namespace Bth.Modules.Accounts.Application.Accounts;

internal sealed class AccountDebitedDomainEventHandler(IEventBus eventBus)
    : DomainEventHandler<AccountDebitedDomainEvent>
{
    public override async Task Handle(AccountDebitedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        await eventBus.PublishAsync(
            new AccountBalanceChangedIntegrationEvent(
                Guid.NewGuid(),
                domainEvent.OccurredOnUtc,
                domainEvent.AccountId,
                domainEvent.HolderName,
                domainEvent.BalanceAfter,
                domainEvent.OverdraftLimit,
                domainEvent.Status),
            cancellationToken);
    }
}

internal sealed class AccountCreditedDomainEventHandler(IEventBus eventBus)
    : DomainEventHandler<AccountCreditedDomainEvent>
{
    public override async Task Handle(AccountCreditedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        await eventBus.PublishAsync(
            new AccountBalanceChangedIntegrationEvent(
                Guid.NewGuid(),
                domainEvent.OccurredOnUtc,
                domainEvent.AccountId,
                domainEvent.HolderName,
                domainEvent.BalanceAfter,
                domainEvent.OverdraftLimit,
                domainEvent.Status),
            cancellationToken);
    }
}

internal sealed class AccountLimitsChangedDomainEventHandler(IBankingNotifier notifier)
    : DomainEventHandler<AccountLimitsChangedDomainEvent>
{
    public override Task Handle(AccountLimitsChangedDomainEvent domainEvent, CancellationToken cancellationToken = default) =>
        notifier.AccountChangedAsync(
            new AccountNotification(
                domainEvent.AccountId,
                domainEvent.HolderName,
                domainEvent.Balance,
                domainEvent.OverdraftLimit,
                domainEvent.Balance + domainEvent.OverdraftLimit,
                domainEvent.Status),
            cancellationToken);
}

internal sealed class AccountStatusChangedDomainEventHandler(IBankingNotifier notifier)
    : DomainEventHandler<AccountStatusChangedDomainEvent>
{
    public override Task Handle(AccountStatusChangedDomainEvent domainEvent, CancellationToken cancellationToken = default) =>
        notifier.AccountChangedAsync(
            new AccountNotification(
                domainEvent.AccountId,
                domainEvent.HolderName,
                domainEvent.Balance,
                domainEvent.OverdraftLimit,
                domainEvent.Balance + domainEvent.OverdraftLimit,
                domainEvent.Status),
            cancellationToken);
}
