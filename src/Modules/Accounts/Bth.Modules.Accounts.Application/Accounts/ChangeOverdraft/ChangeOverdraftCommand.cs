using Bth.Common.Application.Messaging;

namespace Bth.Modules.Accounts.Application.Accounts.ChangeOverdraft;

public sealed record ChangeOverdraftCommand(Guid AccountId, decimal OverdraftLimit) : ICommand;
