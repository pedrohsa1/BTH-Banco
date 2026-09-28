using Bth.Common.Application.Messaging;

namespace Bth.Modules.Accounts.Application.Accounts.ChangeAccountStatus;

public sealed record ChangeAccountStatusCommand(Guid AccountId, string Status) : ICommand;
