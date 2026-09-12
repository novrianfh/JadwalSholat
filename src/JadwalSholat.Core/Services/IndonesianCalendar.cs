using System.Globalization;

namespace JadwalSholat.Core.Services;

/// <summary>Formats dates in Bahasa Indonesia without depending on WASM ICU/globalization data being loaded
/// (day/month names are written out directly, so this works even with InvariantGlobalization).</summary>
public static class IndonesianCalendar
{
    private static readonly string[] DayNames =
        ["Minggu", "Senin", "Selasa", "Rabu", "Kamis", "Jumat", "Sabtu"];

    private static readonly string[] GregorianMonthNames =
    [
        "Januari", "Februari", "Maret", "April", "Mei", "Juni",
        "Juli", "Agustus", "September", "Oktober", "November", "Desember"
    ];

    private static readonly string[] HijriMonthNames =
    [
        "Muharram", "Safar", "Rabiul Awal", "Rabiul Akhir", "Jumadil Awal", "Jumadil Akhir",
        "Rajab", "Syaban", "Ramadhan", "Syawal", "Dzulkaidah", "Dzulhijjah"
    ];

    private static readonly HijriCalendar Hijri = new();

    /// <summary>e.g. "Sabtu, 12 September 2026".</summary>
    public static string FormatGregorianLong(DateOnly date)
    {
        var dayName = DayNames[(int)date.DayOfWeek];
        var monthName = GregorianMonthNames[date.Month - 1];
        return $"{dayName}, {date.Day} {monthName} {date.Year}";
    }

    /// <summary>e.g. "1 Rabiul Akhir 1448 H". Uses .NET's tabular Hijri calendar, which can differ by a day
    /// from the Kemenag rukyat (moon-sighting) announcement around month boundaries — shown as an estimate.</summary>
    public static string FormatHijri(DateOnly date)
    {
        var dt = date.ToDateTime(TimeOnly.MinValue);
        var day = Hijri.GetDayOfMonth(dt);
        var month = Hijri.GetMonth(dt);
        var year = Hijri.GetYear(dt);
        return $"{day} {HijriMonthNames[month - 1]} {year} H";
    }
}
