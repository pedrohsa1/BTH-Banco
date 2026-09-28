using FluentValidation;

namespace Bth.Modules.Transfers.Application.Transfers.CreateImmediateTransfer;

internal sealed class CreateImmediateTransferCommandValidator : AbstractValidator<CreateImmediateTransferCommand>
{
    public CreateImmediateTransferCommandValidator()
    {
        RuleFor(command => command.SourceAccountId).NotEmpty();
        RuleFor(command => command.DestinationAccountId).NotEmpty();
        RuleFor(command => command.Amount)
            .GreaterThan(0)
            .WithMessage("O valor da transferência deve ser maior que zero.");
        RuleFor(command => command)
            .Must(command => command.SourceAccountId != command.DestinationAccountId)
            .WithMessage("A conta de origem e a conta de destino devem ser diferentes.");
        RuleFor(command => command.Amount)
            .Must(amount => decimal.Round(amount, 2) == amount)
            .WithMessage("O valor deve ter no máximo duas casas decimais.");
    }
}
