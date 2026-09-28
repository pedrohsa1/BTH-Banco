using Bth.Common.Application.Messaging;
using Bth.Common.Domain;
using Bth.Modules.Accounts.Application.Abstractions.Data;
using Bth.Modules.Accounts.Domain.Accounts;

namespace Bth.Modules.Accounts.Application.Accounts.ChangeAccountStatus;

internal sealed class ChangeAccountStatusCommandHandler(
    IAccountRepository accountRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<ChangeAccountStatusCommand>
{
    public async Task<Result> Handle(ChangeAccountStatusCommand request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<AccountStatus>(request.Status, ignoreCase: true, out AccountStatus status))
        {
            return Result.Failure(AccountErrors.InvalidStatus);
        }

        Account? account = await accountRepository.GetAsync(request.AccountId, cancellationToken);

        if (account is null)
        {
            return Result.Failure(AccountErrors.NotFound);
        }

        Result result = account.ChangeStatus(status);

        if (result.IsFailure)
        {
            return result;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
