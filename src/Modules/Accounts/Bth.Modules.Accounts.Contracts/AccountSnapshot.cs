namespace Bth.Modules.Accounts.Contracts;

public sealed record AccountSnapshot(
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
    int NightMaxAttemptsPerHour);
