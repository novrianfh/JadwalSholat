namespace JadwalSholat.Core.Models;

public enum PrayerName
{
    Fajr,
    Shuruk,
    Dhuhr,
    Asr,
    Maghrib,
    Isha
}

public static class PrayerNames
{
    /// <summary>All 6 rows shown chronologically on the dashboard grid, in daily order.</summary>
    public static readonly IReadOnlyList<PrayerName> Chronological =
    [
        PrayerName.Fajr, PrayerName.Shuruk, PrayerName.Dhuhr, PrayerName.Asr, PrayerName.Maghrib, PrayerName.Isha
    ];

    /// <summary>The 5 obligatory prayers that have an Iqamah and an active "waktu sholat" window. Shuruk is excluded: it is a sunrise marker, not a prayer congregations perform.</summary>
    public static readonly IReadOnlyList<PrayerName> Obligatory =
    [
        PrayerName.Fajr, PrayerName.Dhuhr, PrayerName.Asr, PrayerName.Maghrib, PrayerName.Isha
    ];

    public static string ToIndonesian(this PrayerName name) => name switch
    {
        PrayerName.Fajr => "Subuh",
        PrayerName.Shuruk => "Terbit",
        PrayerName.Dhuhr => "Dzuhur",
        PrayerName.Asr => "Ashar",
        PrayerName.Maghrib => "Maghrib",
        PrayerName.Isha => "Isya",
        _ => throw new ArgumentOutOfRangeException(nameof(name))
    };
}
