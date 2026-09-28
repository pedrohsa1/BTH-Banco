using FluentValidation;

namespace Bth.Modules.Accounts.Application.Accounts.ChangeAccountStatus;

internal sealed class ChangeAccountStatusCommandValidator : AbstractValidator<ChangeAccountStatusCommand>
{
    public ChangeAccountStatusCommandValidator()
    {
        RuleFor(command => command.AccountId).NotEmpty();
        RuleFor(command => command.Status).NotEmpty();
    }
}
