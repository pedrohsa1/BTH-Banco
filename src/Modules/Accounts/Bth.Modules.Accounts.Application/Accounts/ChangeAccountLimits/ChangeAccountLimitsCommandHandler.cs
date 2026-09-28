using Bth.Common.Application.Messaging;
using Bth.Common.Domain;
using Bth.Modules.Accounts.Application.Abstractions.Data;
using Bth.Modules.Accounts.Domain.Accounts;

namespace Bth.Modules.Accounts.Application.Accounts.ChangeAccountLimits;

internal sealed class ChangeAccountLimitsCommandHandler(
    IAccountRepository accountRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<ChangeAccountLimitsCommand>
{
    public async Task<Result> Handle(ChangeAccountLimitsCommand request, CancellationToken cancellationToken)
    {
        Account? account = await accountRepository.GetAsync(request.AccountId, cancellationToken);

        if (account is null)
        {
            return Result.Failure(AccountErrors.NotFound);
        }

        Result result = account.ChangeLimits(
            request.DayMaxAmountPerHour,
            request.DayMaxAttemptsPerHour,
            request.NightMaxAmountPerHour,
            request.NightMaxAttemptsPerHour);

        if (result.IsFailure)
        {
            return result;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
