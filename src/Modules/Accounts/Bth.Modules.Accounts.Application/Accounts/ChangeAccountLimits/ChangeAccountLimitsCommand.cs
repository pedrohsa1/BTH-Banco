using Bth.Common.Application.Messaging;

namespace Bth.Modules.Accounts.Application.Accounts.ChangeAccountLimits;

public sealed record ChangeAccountLimitsCommand(
    Guid AccountId,
    decimal DayMaxAmountPerHour,
    int DayMaxAttemptsPerHour,
    decimal NightMaxAmountPerHour,
    int NightMaxAttemptsPerHour) : ICommand;
