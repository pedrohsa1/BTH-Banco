using Bth.Common.Application.Clock;
using Bth.Common.Application.Messaging;
using Bth.Common.Domain;
using Bth.Modules.Accounts.Application.Abstractions.Data;
using Bth.Modules.Accounts.Domain.Accounts;

namespace Bth.Modules.Accounts.Application.Accounts.OpenAccount;

internal sealed class OpenAccountCommandHandler(
    IAccountRepository accountRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<OpenAccountCommand, Guid>
{
    public async Task<Result<Guid>> Handle(OpenAccountCommand request, CancellationToken cancellationToken)
    {
        if (await accountRepository.DocumentExistsAsync(request.Document.Trim(), cancellationToken))
        {
            return Result.Failure<Guid>(AccountErrors.DocumentAlreadyExists);
        }

        Result<Account> accountResult = Account.Open(
            request.HolderName,
            request.Document,
            request.InitialBalance,
            request.OverdraftLimit,
            request.DayMaxAmountPerHour,
            request.DayMaxAttemptsPerHour,
            request.NightMaxAmountPerHour,
            request.NightMaxAttemptsPerHour,
            dateTimeProvider.UtcNow);

        if (accountResult.IsFailure)
        {
            return Result.Failure<Guid>(accountResult.Error);
        }

        accountRepository.Insert(accountResult.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return accountResult.Value.Id;
    }
}
