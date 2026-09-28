using Bth.Common.Domain;

namespace Bth.Modules.Accounts.Domain.Accounts;

public sealed class Account : Entity
{
    private Account()
    {
    }

    public Guid Id { get; private set; }

    public string HolderName { get; private set; } = string.Empty;

    public string Document { get; private set; } = string.Empty;

    public decimal Balance { get; private set; }

    public decimal OverdraftLimit { get; private set; }

    public AccountStatus Status { get; private set; }

    public decimal DayMaxAmountPerHour { get; private set; }

    public int DayMaxAttemptsPerHour { get; private set; }

    public decimal NightMaxAmountPerHour { get; private set; }

    public int NightMaxAttemptsPerHour { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public decimal AvailableFunds => Balance + OverdraftLimit;

    public static Result<Account> Open(
        string holderName,
        string document,
        decimal initialBalance,
        decimal overdraftLimit,
        decimal dayMaxAmountPerHour,
        int dayMaxAttemptsPerHour,
        decimal nightMaxAmountPerHour,
        int nightMaxAttemptsPerHour,
        DateTime utcNow,
        Guid? id = null)
    {
        if (string.IsNullOrWhiteSpace(holderName) ||
            string.IsNullOrWhiteSpace(document) ||
            initialBalance < 0 ||
            overdraftLimit < 0 ||
            dayMaxAmountPerHour <= 0 ||
            nightMaxAmountPerHour <= 0 ||
            dayMaxAttemptsPerHour <= 0 ||
            nightMaxAttemptsPerHour <= 0)
        {
            return Result.Failure<Account>(AccountErrors.InvalidOpening);
        }

        var account = new Account
        {
            Id = id ?? Guid.NewGuid(),
            HolderName = holderName.Trim(),
            Document = document.Trim(),
            Balance = decimal.Round(initialBalance, 2, MidpointRounding.AwayFromZero),
            OverdraftLimit = decimal.Round(overdraftLimit, 2, MidpointRounding.AwayFromZero),
            Status = AccountStatus.Active,
            DayMaxAmountPerHour = decimal.Round(dayMaxAmountPerHour, 2, MidpointRounding.AwayFromZero),
            DayMaxAttemptsPerHour = dayMaxAttemptsPerHour,
            NightMaxAmountPerHour = decimal.Round(nightMaxAmountPerHour, 2, MidpointRounding.AwayFromZero),
            NightMaxAttemptsPerHour = nightMaxAttemptsPerHour,
            CreatedAtUtc = utcNow
        };

        account.Raise(new AccountOpenedDomainEvent
        {
            AccountId = account.Id,
            Balance = account.Balance
        });

        return account;
    }

    public Result Debit(decimal amount, Guid transferId)
    {
        if (Status != AccountStatus.Active)
        {
            return Result.Failure(AccountErrors.NotActive);
        }

        if (amount <= 0)
        {
            return Result.Failure(AccountErrors.InvalidAmount);
        }

        if (Balance + OverdraftLimit < amount)
        {
            return Result.Failure(AccountErrors.InsufficientFunds);
        }

        Balance -= amount;

        Raise(new AccountDebitedDomainEvent
        {
            AccountId = Id,
            TransferId = transferId,
            Amount = amount,
            BalanceAfter = Balance,
            HolderName = HolderName,
            OverdraftLimit = OverdraftLimit,
            Status = Status.ToString()
        });

        return Result.Success();
    }

    public Result Credit(decimal amount, Guid transferId)
    {
        if (Status != AccountStatus.Active)
        {
            return Result.Failure(AccountErrors.NotActive);
        }

        if (amount <= 0)
        {
            return Result.Failure(AccountErrors.InvalidAmount);
        }

        Balance += amount;

        Raise(new AccountCreditedDomainEvent
        {
            AccountId = Id,
            TransferId = transferId,
            Amount = amount,
            BalanceAfter = Balance,
            HolderName = HolderName,
            OverdraftLimit = OverdraftLimit,
            Status = Status.ToString()
        });

        return Result.Success();
    }

    public Result ChangeStatus(AccountStatus status)
    {
        if (!Enum.IsDefined(status))
        {
            return Result.Failure(AccountErrors.InvalidStatus);
        }

        Status = status;

        Raise(new AccountStatusChangedDomainEvent
        {
            AccountId = Id,
            Status = Status.ToString(),
            HolderName = HolderName,
            Balance = Balance,
            OverdraftLimit = OverdraftLimit
        });

        return Result.Success();
    }

    public Result ChangeLimits(
        decimal dayMaxAmountPerHour,
        int dayMaxAttemptsPerHour,
        decimal nightMaxAmountPerHour,
        int nightMaxAttemptsPerHour)
    {
        if (dayMaxAmountPerHour <= 0 ||
            nightMaxAmountPerHour <= 0 ||
            dayMaxAttemptsPerHour <= 0 ||
            nightMaxAttemptsPerHour <= 0)
        {
            return Result.Failure(AccountErrors.InvalidLimits);
        }

        DayMaxAmountPerHour = decimal.Round(dayMaxAmountPerHour, 2, MidpointRounding.AwayFromZero);
        DayMaxAttemptsPerHour = dayMaxAttemptsPerHour;
        NightMaxAmountPerHour = decimal.Round(nightMaxAmountPerHour, 2, MidpointRounding.AwayFromZero);
        NightMaxAttemptsPerHour = nightMaxAttemptsPerHour;

        Raise(new AccountLimitsChangedDomainEvent
        {
            AccountId = Id,
            HolderName = HolderName,
            Balance = Balance,
            OverdraftLimit = OverdraftLimit,
            Status = Status.ToString()
        });

        return Result.Success();
    }

    public Result ChangeOverdraft(decimal overdraftLimit)
    {
        if (overdraftLimit < 0)
        {
            return Result.Failure(AccountErrors.InvalidOverdraft);
        }

        decimal rounded = decimal.Round(overdraftLimit, 2, MidpointRounding.AwayFromZero);

        if (Balance + rounded < 0)
        {
            return Result.Failure(AccountErrors.OverdraftBelowUsed);
        }

        OverdraftLimit = rounded;

        Raise(new AccountLimitsChangedDomainEvent
        {
            AccountId = Id,
            HolderName = HolderName,
            Balance = Balance,
            OverdraftLimit = OverdraftLimit,
            Status = Status.ToString()
        });

        return Result.Success();
    }
}
