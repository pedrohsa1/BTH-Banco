using System.Data;
using System.Security.Cryptography;
using System.Text;
using Bth.Common.Application.Clock;
using Bth.Common.Application.Outbox;
using Bth.Common.Domain;
using Bth.Common.Infrastructure.Outbox;
using Bth.Modules.Accounts.Domain.Accounts;
using Bth.Modules.Accounts.Infrastructure.Database;
using Bth.Modules.Transfers.Application.Abstractions;
using Bth.Modules.Transfers.Application.Transfers;
using Bth.Modules.Transfers.Domain.Transfers;
using Bth.Modules.Transfers.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Bth.Composition;

public sealed class TransferExecutor(
    IConfiguration configuration,
    IDateTimeProvider dateTimeProvider,
    IOptions<LimitOptions> limitOptions,
    ITransferScheduler transferScheduler,
    IOutboxProcessor outboxProcessor,
    InsertOutboxMessagesInterceptor outboxInterceptor,
    ILogger<TransferExecutor> logger) : ITransferExecutor
{
    public async Task<Result<TransferResponse>> ExecuteImmediateAsync(
        Guid sourceAccountId,
        Guid destinationAccountId,
        decimal amount,
        string? idempotencyKey,
        CancellationToken cancellationToken)
    {
        string? key = NormalizeKey(idempotencyKey);
        string hash = Hash(sourceAccountId, destinationAccountId, amount, null);

        return await InTransactionAsync(async (accounts, transfers, cancellationToken) =>
        {
            Result<TransferResponse>? replay = await FindReplayAsync(transfers, key, hash, cancellationToken);
            if (replay is not null)
            {
                return replay;
            }

            (Account? source, Account? destination, Result<TransferResponse>? missing) =
                await LockAccountsAsync(accounts, sourceAccountId, destinationAccountId, cancellationToken);

            if (missing is not null)
            {
                return missing;
            }

            DateTime utcNow = dateTimeProvider.UtcNow;
            var transfer = Transfer.CreateImmediate(
                sourceAccountId,
                destinationAccountId,
                amount,
                utcNow,
                key,
                hash);

            transfers.Transfers.Add(transfer);

            string? failure = await MoveOrExplainAsync(
                accounts,
                transfers,
                source!,
                destination!,
                transfer,
                utcNow,
                cancellationToken);

            if (failure is not null)
            {
                transfer.Fail(failure, utcNow);
                accounts.ChangeTracker.Clear();
                await transfers.SaveChangesAsync(cancellationToken);
                return TransferMapper.ToResponse(transfer, replayed: false);
            }

            await accounts.SaveChangesAsync(cancellationToken);
            await transfers.SaveChangesAsync(cancellationToken);
            return TransferMapper.ToResponse(transfer, replayed: false);
        }, cancellationToken);
    }

    public async Task<Result<TransferResponse>> ScheduleAsync(
        Guid sourceAccountId,
        Guid destinationAccountId,
        decimal amount,
        DateTime scheduledAtUtc,
        string? idempotencyKey,
        CancellationToken cancellationToken)
    {
        string? key = NormalizeKey(idempotencyKey);
        string hash = Hash(sourceAccountId, destinationAccountId, amount, scheduledAtUtc);
        DateTime utcNow = dateTimeProvider.UtcNow;

        Result<TransferResponse> result = await InTransactionAsync(async (accounts, transfers, cancellationToken) =>
        {
            Result<TransferResponse>? replay = await FindReplayAsync(transfers, key, hash, cancellationToken);
            if (replay is not null)
            {
                return replay;
            }

            (Account? source, Account? destination, Result<TransferResponse>? missing) =
                await LockAccountsAsync(accounts, sourceAccountId, destinationAccountId, cancellationToken);

            if (missing is not null)
            {
                return missing;
            }

            if (source!.Status != AccountStatus.Active || destination!.Status != AccountStatus.Active)
            {
                return Result.Failure<TransferResponse>(AccountErrors.NotActive);
            }

            Result<Transfer> scheduled = Transfer.Schedule(
                sourceAccountId,
                destinationAccountId,
                amount,
                scheduledAtUtc,
                utcNow,
                key,
                hash);

            if (scheduled.IsFailure)
            {
                return Result.Failure<TransferResponse>(scheduled.Error);
            }

            transfers.Transfers.Add(scheduled.Value);
            await transfers.SaveChangesAsync(cancellationToken);
            return TransferMapper.ToResponse(scheduled.Value, replayed: false);
        }, cancellationToken);

        if (result.IsSuccess && !result.Value.Replayed && result.Value.Status == TransferStatus.Scheduled.ToString())
        {
            transferScheduler.Schedule(result.Value.Id, scheduledAtUtc);
        }

        return result;
    }

    public Task<Result<TransferResponse>> CancelAsync(Guid transferId, CancellationToken cancellationToken) =>
        InTransactionAsync(async (_, transfers, cancellationToken) =>
        {
            await transfers.Database.ExecuteSqlInterpolatedAsync(
                $"SELECT id FROM transfers.transfers WHERE id = {transferId} FOR UPDATE",
                cancellationToken);

            Transfer? transfer = await transfers.Transfers
                .FirstOrDefaultAsync(item => item.Id == transferId, cancellationToken);

            if (transfer is null)
            {
                return Result.Failure<TransferResponse>(TransferErrors.NotFound);
            }

            Result cancel = transfer.Cancel();
            if (cancel.IsFailure)
            {
                return Result.Failure<TransferResponse>(cancel.Error);
            }

            await transfers.SaveChangesAsync(cancellationToken);
            return TransferMapper.ToResponse(transfer, replayed: false);
        }, cancellationToken);

    public async Task ExecuteDueAsync(Guid transferId, CancellationToken cancellationToken)
    {
        try
        {
            await InTransactionAsync(async (accounts, transfers, cancellationToken) =>
            {
                await transfers.Database.ExecuteSqlInterpolatedAsync(
                    $"SELECT id FROM transfers.transfers WHERE id = {transferId} FOR UPDATE",
                    cancellationToken);

                Transfer? transfer = await transfers.Transfers
                    .FirstOrDefaultAsync(item => item.Id == transferId, cancellationToken);

                if (transfer is null || transfer.Status != TransferStatus.Scheduled)
                {
                    return Result.Success(false);
                }

                DateTime utcNow = dateTimeProvider.UtcNow;
                if (transfer.ScheduledAtUtc > utcNow)
                {
                    return Result.Success(false);
                }

                Result processing = transfer.BeginProcessing(utcNow);
                if (processing.IsFailure)
                {
                    return Result.Failure<bool>(processing.Error);
                }

                (Account? source, Account? destination, _) =
                    await LockAccountsAsync(
                        accounts,
                        transfer.SourceAccountId,
                        transfer.DestinationAccountId,
                        cancellationToken);

                if (source is null || destination is null)
                {
                    transfer.Fail(TransferErrors.AccountNotFound().Description, utcNow);
                    accounts.ChangeTracker.Clear();
                    await transfers.SaveChangesAsync(cancellationToken);
                    return Result.Success(true);
                }

                string? failure = await MoveOrExplainAsync(
                    accounts,
                    transfers,
                    source,
                    destination,
                    transfer,
                    utcNow,
                    cancellationToken);

                if (failure is not null)
                {
                    transfer.Fail(failure, utcNow);
                    accounts.ChangeTracker.Clear();
                    await transfers.SaveChangesAsync(cancellationToken);
                    return Result.Success(true);
                }

                await accounts.SaveChangesAsync(cancellationToken);
                await transfers.SaveChangesAsync(cancellationToken);
                return Result.Success(true);
            }, cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Falha ao executar a transferência agendada {TransferId}", transferId);
        }
    }

    public async Task DispatchDueAsync(CancellationToken cancellationToken)
    {
        await using NpgsqlConnection connection = await OpenConnectionAsync(cancellationToken);
        await using TransfersDbContext transfers = CreateTransfersContext(connection);
        DateTime utcNow = dateTimeProvider.UtcNow;
        List<Guid> dueIds = await transfers.Transfers
            .Where(transfer => transfer.Status == TransferStatus.Scheduled && transfer.ScheduledAtUtc <= utcNow)
            .Select(transfer => transfer.Id)
            .ToListAsync(cancellationToken);

        foreach (Guid transferId in dueIds)
        {
            await ExecuteDueAsync(transferId, cancellationToken);
        }
    }

    private async Task<string?> MoveOrExplainAsync(
        AccountsDbContext accounts,
        TransfersDbContext transfers,
        Account source,
        Account destination,
        Transfer transfer,
        DateTime utcNow,
        CancellationToken cancellationToken)
    {
        if (source.Status != AccountStatus.Active || destination.Status != AccountStatus.Active)
        {
            return AccountErrors.NotActive.Description;
        }

        LimitWindow window = CreateWindow(utcNow, source);
        int attempts = await transfers.Transfers.CountAsync(
            item =>
                item.SourceAccountId == source.Id &&
                item.Id != transfer.Id &&
                item.AttemptedAtUtc >= window.StartUtc &&
                item.AttemptedAtUtc < window.EndUtc &&
                item.Status != TransferStatus.Cancelled &&
                item.Status != TransferStatus.Scheduled,
            cancellationToken);

        decimal completedAmount = await transfers.Transfers
            .Where(item =>
                item.SourceAccountId == source.Id &&
                item.Id != transfer.Id &&
                item.Status == TransferStatus.Completed &&
                item.AttemptedAtUtc >= window.StartUtc &&
                item.AttemptedAtUtc < window.EndUtc)
            .SumAsync(item => (decimal?)item.Amount, cancellationToken) ?? 0m;

        Result limit = TransferLimitEvaluator.Evaluate(
            window.MaxAmount,
            window.MaxAttempts,
            attempts,
            completedAmount,
            transfer.Amount);

        if (limit.IsFailure)
        {
            return limit.Error.Description;
        }

        Result debit = source.Debit(transfer.Amount, transfer.Id);
        if (debit.IsFailure)
        {
            return debit.Error.Description;
        }

        Result credit = destination.Credit(transfer.Amount, transfer.Id);
        if (credit.IsFailure)
        {
            return credit.Error.Description;
        }

        accounts.AccountEntries.Add(AccountEntry.Create(
            source.Id,
            transfer.Id,
            EntryType.Debit,
            transfer.Amount,
            source.Balance,
            utcNow));

        accounts.AccountEntries.Add(AccountEntry.Create(
            destination.Id,
            transfer.Id,
            EntryType.Credit,
            transfer.Amount,
            destination.Balance,
            utcNow));

        Result completed = transfer.Complete(utcNow);
        return completed.IsFailure ? completed.Error.Description : null;
    }

    private LimitWindow CreateWindow(DateTime utcNow, Account source)
    {
        TimeZoneInfo timeZone = BusinessClock.ResolveTimeZone(limitOptions.Value.TimeZone);
        DateTime local = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utcNow, DateTimeKind.Utc), timeZone);
        bool night = BusinessClock.IsNight(local, limitOptions.Value.NightStartHour, limitOptions.Value.NightEndHour);
        (DateTime startUtc, DateTime endUtc) = BusinessClock.GetHourWindow(utcNow, timeZone);

        return night
            ? new LimitWindow(startUtc, endUtc, source.NightMaxAmountPerHour, source.NightMaxAttemptsPerHour)
            : new LimitWindow(startUtc, endUtc, source.DayMaxAmountPerHour, source.DayMaxAttemptsPerHour);
    }

    private async Task<Result<T>> InTransactionAsync<T>(
        Func<AccountsDbContext, TransfersDbContext, CancellationToken, Task<Result<T>>> action,
        CancellationToken cancellationToken)
    {
        const int maxAttempts = 3;

        for (int attempt = 1; ; attempt++)
        {
            try
            {
                return await InTransactionOnceAsync(action, cancellationToken);
            }
            catch (Exception exception) when (attempt < maxAttempts && IsTransientConcurrency(exception))
            {
                logger.LogWarning(exception, "Conflito de concorrência na transferência. Tentativa {Attempt}", attempt);
            }
        }
    }

    private async Task<Result<T>> InTransactionOnceAsync<T>(
        Func<AccountsDbContext, TransfersDbContext, CancellationToken, Task<Result<T>>> action,
        CancellationToken cancellationToken)
    {
        await using NpgsqlConnection connection = await OpenConnectionAsync(cancellationToken);
        await using AccountsDbContext accounts = CreateAccountsContext(connection);
        await using TransfersDbContext transfers = CreateTransfersContext(connection);
        await using NpgsqlTransaction transaction =
            await connection.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

        await accounts.Database.UseTransactionAsync(transaction, cancellationToken);
        await transfers.Database.UseTransactionAsync(transaction, cancellationToken);

        Result<T> result = await action(accounts, transfers, cancellationToken);

        if (result.IsSuccess)
        {
            await transaction.CommitAsync(cancellationToken);
            await PublishOutboxAsync(cancellationToken);
        }

        return result;
    }

    private async Task PublishOutboxAsync(CancellationToken cancellationToken)
    {
        try
        {
            await outboxProcessor.ProcessAsync(
                "accounts",
                Bth.Modules.Accounts.Application.AssemblyReference.Assembly,
                cancellationToken);
            await outboxProcessor.ProcessAsync(
                "transfers",
                Bth.Modules.Transfers.Application.AssemblyReference.Assembly,
                cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "A transferência foi confirmada, mas a outbox será reprocessada pelo Hangfire");
        }
    }

    private async Task<(Account? Source, Account? Destination, Result<TransferResponse>? Missing)> LockAccountsAsync(
        AccountsDbContext accounts,
        Guid sourceAccountId,
        Guid destinationAccountId,
        CancellationToken cancellationToken)
    {
        foreach (Guid accountId in new[] { sourceAccountId, destinationAccountId }.OrderBy(id => id))
        {
            await accounts.Database.ExecuteSqlInterpolatedAsync(
                $"SELECT id FROM accounts.accounts WHERE id = {accountId} FOR UPDATE",
                cancellationToken);
        }

        Account? source = await accounts.Accounts.FirstOrDefaultAsync(account => account.Id == sourceAccountId, cancellationToken);
        Account? destination = await accounts.Accounts.FirstOrDefaultAsync(account => account.Id == destinationAccountId, cancellationToken);

        if (source is null || destination is null)
        {
            return (source, destination, Result.Failure<TransferResponse>(TransferErrors.AccountNotFound()));
        }

        return (source, destination, null);
    }

    private static async Task<Result<TransferResponse>?> FindReplayAsync(
        TransfersDbContext transfers,
        string? key,
        string hash,
        CancellationToken cancellationToken)
    {
        if (key is null)
        {
            return null;
        }

        Transfer? existing = await transfers.Transfers
            .FirstOrDefaultAsync(transfer => transfer.IdempotencyKey == key, cancellationToken);

        if (existing is null)
        {
            return null;
        }

        if (!string.Equals(existing.RequestHash, hash, StringComparison.Ordinal))
        {
            return Result.Failure<TransferResponse>(TransferErrors.IdempotencyConflict);
        }

        return TransferMapper.ToResponse(existing, replayed: true);
    }

    private async Task<NpgsqlConnection> OpenConnectionAsync(CancellationToken cancellationToken)
    {
        var connection = new NpgsqlConnection(configuration.GetConnectionString("Database"));
        await connection.OpenAsync(cancellationToken);
        return connection;
    }

    private AccountsDbContext CreateAccountsContext(NpgsqlConnection connection)
    {
        var options = new DbContextOptionsBuilder<AccountsDbContext>()
            .UseNpgsql(connection)
            .UseSnakeCaseNamingConvention()
            .AddInterceptors(outboxInterceptor)
            .Options;

        return new AccountsDbContext(options);
    }

    private TransfersDbContext CreateTransfersContext(NpgsqlConnection connection)
    {
        var options = new DbContextOptionsBuilder<TransfersDbContext>()
            .UseNpgsql(connection)
            .UseSnakeCaseNamingConvention()
            .AddInterceptors(outboxInterceptor)
            .Options;

        return new TransfersDbContext(options);
    }

    private static string? NormalizeKey(string? idempotencyKey) =>
        string.IsNullOrWhiteSpace(idempotencyKey) ? null : idempotencyKey.Trim();

    private static string Hash(Guid sourceAccountId, Guid destinationAccountId, decimal amount, DateTime? scheduledAtUtc)
    {
        string raw = $"{sourceAccountId:N}|{destinationAccountId:N}|{amount:0.00}|{scheduledAtUtc:O}";
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(raw)));
    }

    private static bool IsTransientConcurrency(Exception exception)
    {
        for (Exception? current = exception; current is not null; current = current.InnerException)
        {
            if (current is PostgresException postgres && postgres.SqlState is "40001" or "40P01")
            {
                return true;
            }
        }

        return false;
    }

    private sealed record LimitWindow(DateTime StartUtc, DateTime EndUtc, decimal MaxAmount, int MaxAttempts);
}
