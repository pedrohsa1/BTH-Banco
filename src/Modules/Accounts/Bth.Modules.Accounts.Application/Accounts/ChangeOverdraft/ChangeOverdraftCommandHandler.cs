using Bth.Common.Application.Messaging;
using Bth.Common.Domain;
using Bth.Modules.Accounts.Application.Abstractions.Data;
using Bth.Modules.Accounts.Domain.Accounts;

namespace Bth.Modules.Accounts.Application.Accounts.ChangeOverdraft;

internal sealed class ChangeOverdraftCommandHandler(
    IAccountRepository accountRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<ChangeOverdraftCommand>
{
    public async Task<Result> Handle(ChangeOverdraftCommand request, CancellationToken cancellationToken)
    {
        Account? account = await accountRepository.GetAsync(request.AccountId, cancellationToken);

        if (account is null)
        {
            return Result.Failure(AccountErrors.NotFound);
        }

        Result result = account.ChangeOverdraft(request.OverdraftLimit);

        if (result.IsFailure)
        {
            return result;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
