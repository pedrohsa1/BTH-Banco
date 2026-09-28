using FluentValidation;

namespace Bth.Modules.Accounts.Application.Accounts.ChangeAccountLimits;

internal sealed class ChangeAccountLimitsCommandValidator : AbstractValidator<ChangeAccountLimitsCommand>
{
    public ChangeAccountLimitsCommandValidator()
    {
        RuleFor(command => command.AccountId).NotEmpty();
        RuleFor(command => command.DayMaxAmountPerHour).GreaterThan(0);
        RuleFor(command => command.NightMaxAmountPerHour).GreaterThan(0);
        RuleFor(command => command.DayMaxAttemptsPerHour).GreaterThan(0);
        RuleFor(command => command.NightMaxAttemptsPerHour).GreaterThan(0);
    }
}
