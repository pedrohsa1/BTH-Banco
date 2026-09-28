using Bth.Common.Application.Messaging;

namespace Bth.Modules.Accounts.Application.Accounts.GetAccount;

public sealed record GetAccountQuery(Guid AccountId) : IQuery<AccountResponse>;
