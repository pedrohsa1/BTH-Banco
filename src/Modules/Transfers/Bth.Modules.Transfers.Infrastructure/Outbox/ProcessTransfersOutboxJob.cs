using Bth.Common.Application.Outbox;

namespace Bth.Modules.Transfers.Infrastructure.Outbox;

public sealed class ProcessTransfersOutboxJob(IOutboxProcessor outboxProcessor)
{
    public Task Run() =>
        outboxProcessor.ProcessAsync("transfers", Application.AssemblyReference.Assembly, CancellationToken.None);
}
