using Bth.Common.Application.Messaging;

namespace Bth.Modules.Accounts.Application.Accounts.OpenAccount;

public sealed record OpenAccountCommand(
    string HolderName,
    string Document,
    decimal InitialBalance,
    decimal OverdraftLimit,
    decimal DayMaxAmountPerHour,
    int DayMaxAttemptsPerHour,
    decimal NightMaxAmountPerHour,
    int NightMaxAttemptsPerHour) : ICommand<Guid>;
