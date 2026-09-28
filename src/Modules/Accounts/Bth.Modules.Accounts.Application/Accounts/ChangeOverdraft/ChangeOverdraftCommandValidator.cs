using FluentValidation;

namespace Bth.Modules.Accounts.Application.Accounts.ChangeOverdraft;

internal sealed class ChangeOverdraftCommandValidator : AbstractValidator<ChangeOverdraftCommand>
{
    public ChangeOverdraftCommandValidator()
    {
        RuleFor(command => command.AccountId).NotEmpty();
        RuleFor(command => command.OverdraftLimit).GreaterThanOrEqualTo(0);
    }
}
