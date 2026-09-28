namespace Bth.Modules.Accounts.Application.Accounts;

public sealed record AccountResponse(
    Guid Id,
    string HolderName,
    string Document,
    decimal Balance,
    decimal OverdraftLimit,
    decimal AvailableFunds,
    string Status,
    decimal DayMaxAmountPerHour,
    int DayMaxAttemptsPerHour,
    decimal NightMaxAmountPerHour,
    int NightMaxAttemptsPerHour,
    DateTime CreatedAtUtc);
