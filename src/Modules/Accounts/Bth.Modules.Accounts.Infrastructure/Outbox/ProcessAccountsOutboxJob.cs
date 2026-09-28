using Bth.Common.Application.Outbox;

namespace Bth.Modules.Accounts.Infrastructure.Outbox;

public sealed class ProcessAccountsOutboxJob(IOutboxProcessor outboxProcessor)
{
    public Task Run() =>
        outboxProcessor.ProcessAsync("accounts", Application.AssemblyReference.Assembly, CancellationToken.None);
}
