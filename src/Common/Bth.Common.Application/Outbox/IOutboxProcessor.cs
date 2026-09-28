using System.Reflection;

namespace Bth.Common.Application.Outbox;

public interface IOutboxProcessor
{
    Task ProcessAsync(string schema, Assembly handlersAssembly, CancellationToken cancellationToken = default);
}
