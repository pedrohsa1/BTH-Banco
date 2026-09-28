using Bth.Common.Application.Messaging;

namespace Bth.Modules.Accounts.Application.Accounts.GetAccounts;

public sealed record GetAccountsQuery : IQuery<IReadOnlyList<AccountResponse>>;
