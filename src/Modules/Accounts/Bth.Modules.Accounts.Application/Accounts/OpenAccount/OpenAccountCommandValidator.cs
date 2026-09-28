using FluentValidation;

namespace Bth.Modules.Accounts.Application.Accounts.OpenAccount;

internal sealed class OpenAccountCommandValidator : AbstractValidator<OpenAccountCommand>
{
    public OpenAccountCommandValidator()
    {
        RuleFor(command => command.HolderName).NotEmpty().MaximumLength(200);
        RuleFor(command => command.Document).NotEmpty().MaximumLength(20);
        RuleFor(command => command.InitialBalance).GreaterThanOrEqualTo(0);
        RuleFor(command => command.OverdraftLimit).GreaterThanOrEqualTo(0);
        RuleFor(command => command.DayMaxAmountPerHour).GreaterThan(0);
        RuleFor(command => command.NightMaxAmountPerHour).GreaterThan(0);
        RuleFor(command => command.DayMaxAttemptsPerHour).GreaterThan(0);
        RuleFor(command => command.NightMaxAttemptsPerHour).GreaterThan(0);
    }
}
