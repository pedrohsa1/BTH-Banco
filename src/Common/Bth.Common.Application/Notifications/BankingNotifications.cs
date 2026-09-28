namespace Bth.Common.Application.Notifications;

public sealed record TransferNotification(
    Guid Id,
    Guid SourceAccountId,
    Guid DestinationAccountId,
    decimal Amount,
    string Status,
    DateTime? ScheduledAtUtc,
    DateTime CreatedAtUtc,
    DateTime? ProcessedAtUtc,
    string? FailureReason);

public sealed record AccountNotification(
    Guid Id,
    string HolderName,
    decimal Balance,
    decimal OverdraftLimit,
    decimal AvailableFunds,
    string Status);

public interface IBankingNotifier
{
    Task TransferChangedAsync(TransferNotification notification, CancellationToken cancellationToken = default);

    Task AccountChangedAsync(AccountNotification notification, CancellationToken cancellationToken = default);
}
