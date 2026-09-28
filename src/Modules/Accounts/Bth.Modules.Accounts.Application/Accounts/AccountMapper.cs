using Bth.Modules.Accounts.Domain.Accounts;

namespace Bth.Modules.Accounts.Application.Accounts;

internal static class AccountMapper
{
    public static AccountResponse ToResponse(Account account) =>
        new(
            account.Id,
            account.HolderName,
            account.Document,
            account.Balance,
            account.OverdraftLimit,
            account.AvailableFunds,
            account.Status.ToString(),
            account.DayMaxAmountPerHour,
            account.DayMaxAttemptsPerHour,
            account.NightMaxAmountPerHour,
            account.NightMaxAttemptsPerHour,
            account.CreatedAtUtc);
}
