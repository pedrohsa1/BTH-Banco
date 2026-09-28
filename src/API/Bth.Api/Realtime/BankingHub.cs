using Bth.Common.Application.Notifications;
using Microsoft.AspNetCore.SignalR;

namespace Bth.Api.Realtime;

public sealed class BankingHub : Hub;

public sealed class SignalRBankingNotifier(IHubContext<BankingHub> hubContext) : IBankingNotifier
{
    public Task TransferChangedAsync(TransferNotification notification, CancellationToken cancellationToken = default) =>
        hubContext.Clients.All.SendAsync("transferUpdated", notification, cancellationToken);

    public Task AccountChangedAsync(AccountNotification notification, CancellationToken cancellationToken = default) =>
        hubContext.Clients.All.SendAsync("accountUpdated", notification, cancellationToken);
}
