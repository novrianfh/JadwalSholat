namespace JadwalSholat.Core.Models;

/// <summary>Raw (unadjusted) prayer clock times for one city on one date, as sourced from the API dataset.</summary>
public sealed record DailyPrayerTimes
{
    public required DateOnly Date { get; init; }
    public required TimeOnly Fajr { get; init; }
    public required TimeOnly Shuruk { get; init; }
    public required TimeOnly Dhuhr { get; init; }
    public required TimeOnly Asr { get; init; }
    public required TimeOnly Maghrib { get; init; }
    public required TimeOnly Isha { get; init; }

    public TimeOnly Get(PrayerName name) => name switch
    {
        PrayerName.Fajr => Fajr,
        PrayerName.Shuruk => Shuruk,
        PrayerName.Dhuhr => Dhuhr,
        PrayerName.Asr => Asr,
        PrayerName.Maghrib => Maghrib,
        PrayerName.Isha => Isha,
        _ => throw new ArgumentOutOfRangeException(nameof(name))
    };
}
