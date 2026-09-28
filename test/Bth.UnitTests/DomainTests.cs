using Bth.Modules.Accounts.Domain.Accounts;
using Bth.Modules.Transfers.Domain.Transfers;

namespace Bth.UnitTests;

public class AccountTests
{
    private static readonly DateTime Now = new(2026, 9, 25, 18, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Debit_within_balance_keeps_account_positive()
    {
        Account account = Open(500m, 1000m);

        var result = account.Debit(200m, Guid.NewGuid());

        Assert.True(result.IsSuccess);
        Assert.Equal(300m, account.Balance);
        Assert.Equal(1300m, account.AvailableFunds);
    }

    [Fact]
    public void Debit_can_use_overdraft_and_leave_negative_balance()
    {
        Account account = Open(500m, 1000m);

        var result = account.Debit(1000m, Guid.NewGuid());

        Assert.True(result.IsSuccess);
        Assert.Equal(-500m, account.Balance);
    }

    [Fact]
    public void Debit_above_balance_plus_overdraft_fails()
    {
        Account account = Open(500m, 1000m);

        var result = account.Debit(1501m, Guid.NewGuid());

        Assert.Equal(AccountErrors.InsufficientFunds, result.Error);
        Assert.Equal(500m, account.Balance);
    }

    [Fact]
    public void Credit_covers_used_overdraft_before_leaving_a_positive_balance()
    {
        Account account = Open(200m, 1000m);
        account.Debit(1000m, Guid.NewGuid());

        var result = account.Credit(1000m, Guid.NewGuid());

        Assert.True(result.IsSuccess);
        Assert.Equal(200m, account.Balance);
    }

    [Fact]
    public void Inactive_account_cannot_send_or_receive()
    {
        Account account = Open(500m, 0m);
        account.ChangeStatus(AccountStatus.Inactive);

        Assert.Equal(AccountErrors.NotActive, account.Debit(10m, Guid.NewGuid()).Error);
        Assert.Equal(AccountErrors.NotActive, account.Credit(10m, Guid.NewGuid()).Error);
        Assert.Equal(500m, account.Balance);
    }

    [Fact]
    public void ChangeLimits_updates_the_hourly_windows_of_that_account()
    {
        Account account = Open(500m, 1000m);

        var result = account.ChangeLimits(800m, 2, 300m, 1);

        Assert.True(result.IsSuccess);
        Assert.Equal(800m, account.DayMaxAmountPerHour);
        Assert.Equal(2, account.DayMaxAttemptsPerHour);
        Assert.Equal(300m, account.NightMaxAmountPerHour);
        Assert.Equal(1, account.NightMaxAttemptsPerHour);
    }

    [Fact]
    public void ChangeLimits_rejects_zero()
    {
        Account account = Open(500m, 1000m);

        var result = account.ChangeLimits(0m, 2, 300m, 1);

        Assert.Equal(AccountErrors.InvalidLimits, result.Error);
        Assert.Equal(5000m, account.DayMaxAmountPerHour);
    }

    [Fact]
    public void ChangeOverdraft_updates_the_limit_of_that_account()
    {
        Account account = Open(500m, 1000m);

        var result = account.ChangeOverdraft(250m);

        Assert.True(result.IsSuccess);
        Assert.Equal(250m, account.OverdraftLimit);
        Assert.Equal(750m, account.AvailableFunds);
    }

    [Fact]
    public void ChangeOverdraft_rejects_a_limit_below_the_amount_already_used()
    {
        Account account = Open(200m, 1000m);
        account.Debit(700m, Guid.NewGuid());

        var result = account.ChangeOverdraft(100m);

        Assert.Equal(AccountErrors.OverdraftBelowUsed, result.Error);
        Assert.Equal(1000m, account.OverdraftLimit);
    }

    private static Account Open(decimal balance, decimal overdraft) =>
        Account.Open("Ana", "123", balance, overdraft, 5000m, 5, 1000m, 3, Now).Value;
}

public class TransferStateTests
{
    private static readonly DateTime Now = new(2026, 9, 25, 18, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Scheduled_transfer_can_be_cancelled_before_execution()
    {
        var transfer = Transfer.Schedule(Guid.NewGuid(), Guid.NewGuid(), 10m, Now.AddHours(2), Now, null, "hash").Value;

        var result = transfer.Cancel();

        Assert.True(result.IsSuccess);
        Assert.Equal(TransferStatus.Cancelled, transfer.Status);
    }

    [Fact]
    public void Completed_transfer_cannot_be_cancelled()
    {
        var transfer = Transfer.CreateImmediate(Guid.NewGuid(), Guid.NewGuid(), 10m, Now, null, "hash");
        transfer.Complete(Now);

        Assert.Equal(TransferErrors.CannotCancel, transfer.Cancel().Error);
    }

    [Fact]
    public void Schedule_in_the_past_is_rejected()
    {
        var result = Transfer.Schedule(Guid.NewGuid(), Guid.NewGuid(), 10m, Now.AddMinutes(-1), Now, null, "hash");

        Assert.Equal(TransferErrors.ScheduleMustBeFuture, result.Error);
    }
}

public class TransferLimitTests
{
    [Fact]
    public void Rejects_when_attempt_count_reaches_the_hourly_limit()
    {
        var result = TransferLimitEvaluator.Evaluate(5000m, 5, attemptsAlready: 5, completedAmount: 0m, requestedAmount: 10m);

        Assert.Equal(TransferErrors.AttemptLimitExceeded, result.Error);
    }

    [Fact]
    public void Rejects_when_completed_amount_plus_request_exceeds_the_hourly_limit()
    {
        var result = TransferLimitEvaluator.Evaluate(5000m, 5, attemptsAlready: 1, completedAmount: 4500m, requestedAmount: 600m);

        Assert.Equal(TransferErrors.AmountLimitExceeded, result.Error);
    }

    [Fact]
    public void Night_window_uses_local_hour()
    {
        var zone = BusinessClock.ResolveTimeZone("America/Sao_Paulo");
        var nightUtc = new DateTime(2026, 9, 26, 2, 30, 0, DateTimeKind.Utc);
        var dayUtc = new DateTime(2026, 9, 25, 18, 0, 0, DateTimeKind.Utc);

        Assert.True(BusinessClock.IsNight(TimeZoneInfo.ConvertTimeFromUtc(nightUtc, zone), 20, 6));
        Assert.False(BusinessClock.IsNight(TimeZoneInfo.ConvertTimeFromUtc(dayUtc, zone), 20, 6));
    }
}
