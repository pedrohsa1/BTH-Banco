using Bth.Common.Application.Messaging;
using Bth.Common.Domain;
using Bth.Modules.Accounts.Domain.Accounts;

namespace Bth.Modules.Accounts.Application.Accounts.GetAccount;

internal sealed class GetAccountQueryHandler(IAccountRepository accountRepository)
    : IQueryHandler<GetAccountQuery, AccountResponse>
{
    public async Task<Result<AccountResponse>> Handle(GetAccountQuery request, CancellationToken cancellationToken)
    {
        Account? account = await accountRepository.GetAsync(request.AccountId, cancellationToken);

        if (account is null)
        {
            return Result.Failure<AccountResponse>(AccountErrors.NotFound);
        }

        return AccountMapper.ToResponse(account);
    }
}
