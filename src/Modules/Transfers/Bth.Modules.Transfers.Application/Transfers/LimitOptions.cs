namespace Bth.Modules.Transfers.Application.Transfers;

public sealed class LimitOptions
{
    public const string SectionName = "Limits";

    public string TimeZone { get; set; } = "America/Sao_Paulo";

    public int NightStartHour { get; set; } = 20;

    public int NightEndHour { get; set; } = 6;
}
