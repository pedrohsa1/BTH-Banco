namespace Bth.Common.Domain;

public sealed record ValidationError : Error
{
    public ValidationError(Error[] errors)
        : base("General.Validation", "Um ou mais erros de validação ocorreram.", ErrorType.Validation)
    {
        Errors = errors;
    }

    public Error[] Errors { get; }
}
