using JadwalSholat.Core.Models;

namespace JadwalSholat.Core.Tests;

/// <summary>Shared fixtures. Times are the real Jakarta values returned by api.myquran.com on 2026-09-12
/// (verified via curl during development), so tests exercise realistic, not made-up, data.</summary>
internal static class TestData
{
    public static DailyPrayerTimes RawDay(DateOnly date, TimeOnly? fajr = null, TimeOnly? isha = null) => new()
    {
        Date = date,
        Fajr = fajr ?? new TimeOnly(4, 33),
        Shuruk = new TimeOnly(5, 44),
        Dhuhr = new TimeOnly(11, 53),
        Asr = new TimeOnly(15, 7),
        Maghrib = new TimeOnly(17, 54),
        Isha = isha ?? new TimeOnly(19, 2),
    };

    public static AppSettings DefaultSettings() => AppSettings.CreateDefault();
}
