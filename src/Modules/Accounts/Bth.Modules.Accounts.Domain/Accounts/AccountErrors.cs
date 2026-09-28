using Bth.Common.Domain;

namespace Bth.Modules.Accounts.Domain.Accounts;

public static class AccountErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "Accounts.NotFound",
        "Conta não encontrada.");

    public static readonly Error DocumentAlreadyExists = Error.Conflict(
        "Accounts.DocumentAlreadyExists",
        "Já existe uma conta para este documento.");

    public static readonly Error NotActive = Error.Problem(
        "Accounts.NotActive",
        "Conta inativa não pode enviar nem receber transferências.");

    public static readonly Error InsufficientFunds = Error.Problem(
        "Accounts.InsufficientFunds",
        "Saldo e cheque especial insuficientes para a transferência.");

    public static readonly Error InvalidAmount = Error.Problem(
        "Accounts.InvalidAmount",
        "O valor da movimentação deve ser maior que zero.");

    public static readonly Error InvalidOpening = Error.Validation(
        "Accounts.InvalidOpening",
        "Os dados de abertura da conta são inválidos.");

    public static readonly Error InvalidStatus = Error.Validation(
        "Accounts.InvalidStatus",
        "Status de conta inválido. Use Active ou Inactive.");

    public static readonly Error InvalidLimits = Error.Validation(
        "Accounts.InvalidLimits",
        "Os limites por hora precisam ser maiores que zero.");

    public static readonly Error InvalidOverdraft = Error.Validation(
        "Accounts.InvalidOverdraft",
        "O cheque especial não pode ser negativo.");

    public static readonly Error OverdraftBelowUsed = Error.Validation(
        "Accounts.OverdraftBelowUsed",
        "O cheque especial não cobre o valor já utilizado.");
}
