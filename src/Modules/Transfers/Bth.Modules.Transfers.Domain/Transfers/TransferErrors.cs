using Bth.Common.Domain;

namespace Bth.Modules.Transfers.Domain.Transfers;

public static class TransferErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "Transfers.NotFound",
        "Transferência não encontrada.");

    public static readonly Error SameAccount = Error.Validation(
        "Transfers.SameAccount",
        "A conta de origem e a conta de destino devem ser diferentes.");

    public static readonly Error InvalidAmount = Error.Validation(
        "Transfers.InvalidAmount",
        "O valor da transferência deve ser maior que zero.");

    public static readonly Error ScheduleMustBeFuture = Error.Validation(
        "Transfers.ScheduleMustBeFuture",
        "A data do agendamento deve ser futura.");

    public static readonly Error CannotCancel = Error.Conflict(
        "Transfers.CannotCancel",
        "Somente uma transferência agendada pode ser cancelada.");

    public static readonly Error InvalidTransition = Error.Conflict(
        "Transfers.InvalidTransition",
        "A transferência não pode mudar para o status solicitado.");

    public static readonly Error AttemptLimitExceeded = Error.Problem(
        "Transfers.AttemptLimitExceeded",
        "Limite de tentativas de transferência nesta hora foi excedido.");

    public static readonly Error AmountLimitExceeded = Error.Problem(
        "Transfers.AmountLimitExceeded",
        "Limite de valor de transferência nesta hora foi excedido.");

    public static readonly Error IdempotencyConflict = Error.Conflict(
        "Transfers.IdempotencyConflict",
        "A chave de idempotência já foi usada com dados diferentes.");

    public static Error AccountNotFound() => Error.NotFound(
        "Transfers.AccountNotFound",
        "Conta de origem ou destino não encontrada.");
}
