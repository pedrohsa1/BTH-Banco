using Bth.Common.Domain;

namespace Bth.Modules.Transfers.Domain.Transfers;

public static class TransferLimitEvaluator
{
    public static Result Evaluate(
        decimal maxAmountPerHour,
        int maxAttemptsPerHour,
        int attemptsAlready,
        decimal completedAmount,
        decimal requestedAmount)
    {
        if (attemptsAlready >= maxAttemptsPerHour)
        {
            return Result.Failure(TransferErrors.AttemptLimitExceeded);
        }

        if (completedAmount + requestedAmount > maxAmountPerHour)
        {
            return Result.Failure(TransferErrors.AmountLimitExceeded);
        }

        return Result.Success();
    }
}

public static class BusinessClock
{
    public static bool IsNight(DateTime localTime, int nightStartHour, int nightEndHour)
    {
        int hour = localTime.Hour;

        if (nightStartHour == nightEndHour)
        {
            return false;
        }

        if (nightStartHour > nightEndHour)
        {
            return hour >= nightStartHour || hour < nightEndHour;
        }

        return hour >= nightStartHour && hour < nightEndHour;
    }

    public static (DateTime StartUtc, DateTime EndUtc) GetHourWindow(DateTime utcNow, TimeZoneInfo timeZone)
    {
        DateTime utc = DateTime.SpecifyKind(utcNow, DateTimeKind.Utc);
        DateTime local = TimeZoneInfo.ConvertTimeFromUtc(utc, timeZone);
        var startLocal = new DateTime(local.Year, local.Month, local.Day, local.Hour, 0, 0, DateTimeKind.Unspecified);
        DateTime startUtc = TimeZoneInfo.ConvertTimeToUtc(startLocal, timeZone);
        return (startUtc, startUtc.AddHours(1));
    }

    public static TimeZoneInfo ResolveTimeZone(string timeZoneId)
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
        }
    }
}
