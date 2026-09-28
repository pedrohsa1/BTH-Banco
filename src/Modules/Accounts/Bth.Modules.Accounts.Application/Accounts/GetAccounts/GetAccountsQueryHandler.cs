using Bth.Common.Application.Messaging;
using Bth.Common.Domain;
using Bth.Modules.Accounts.Domain.Accounts;

namespace Bth.Modules.Accounts.Application.Accounts.GetAccounts;

internal sealed class GetAccountsQueryHandler(IAccountRepository accountRepository)
    : IQueryHandler<GetAccountsQuery, IReadOnlyList<AccountResponse>>
{
    public async Task<Result<IReadOnlyList<AccountResponse>>> Handle(
        GetAccountsQuery request,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<Account> accounts = await accountRepository.ListAsync(cancellationToken);

        return accounts.Select(AccountMapper.ToResponse).ToList();
    }
}
