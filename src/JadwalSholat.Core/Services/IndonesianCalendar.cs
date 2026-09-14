using System.Globalization;

namespace JadwalSholat.Core.Services;

/// <summary>Formats dates in Bahasa Indonesia without depending on WASM ICU/globalization data being loaded
/// (day/month names are written out directly, so this works even with InvariantGlobalization).</summary>
public static class IndonesianCalendar
{
    private static readonly string[] DayNames =
        ["Ahad", "Senin", "Selasa", "Rabu", "Kamis", "Jumat", "Sabtu"];

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

    /// <summary>Awal tiap bulan Hijriyah menurut Kalender Hijriah Global Tunggal (KHGT) Muhammadiyah,
    /// dari https://khgt.muhammadiyah.or.id/kalendar-hijriah. Dipakai alih-alih HijriCalendar bawaan .NET
    /// (kalkulasi tabular yang sesekali berbeda 1 hari dari KHGT) supaya tanggal yang tampil konsisten
    /// dengan kalender resmi Muhammadiyah. Blok di bawah dibangkitkan otomatis oleh
    /// tools/generate_khgt_calendar.py (dijalankan berkala oleh
    /// .github/workflows/update-khgt-calendar.yml lewat PR, bukan auto-merge) — jangan diedit manual,
    /// jalankan skripnya untuk memperbarui/memperpanjang cakupan.</summary>
    // BEGIN KHGT-GENERATED (tools/generate_khgt_calendar.py — jangan edit manual)
    private static readonly (DateOnly Start, int Month, int Year)[] KhgtMonthStarts =
    [
        (new DateOnly(2025, 6, 26), 1, 1447),
        (new DateOnly(2025, 7, 26), 2, 1447),
        (new DateOnly(2025, 8, 24), 3, 1447),
        (new DateOnly(2025, 9, 23), 4, 1447),
        (new DateOnly(2025, 10, 23), 5, 1447),
        (new DateOnly(2025, 11, 21), 6, 1447),
        (new DateOnly(2025, 12, 21), 7, 1447),
        (new DateOnly(2026, 1, 20), 8, 1447),
        (new DateOnly(2026, 2, 18), 9, 1447),
        (new DateOnly(2026, 3, 20), 10, 1447),
        (new DateOnly(2026, 4, 18), 11, 1447),
        (new DateOnly(2026, 5, 18), 12, 1447),
        (new DateOnly(2026, 6, 16), 1, 1448),
        (new DateOnly(2026, 7, 15), 2, 1448),
        (new DateOnly(2026, 8, 14), 3, 1448),
        (new DateOnly(2026, 9, 12), 4, 1448),
        (new DateOnly(2026, 10, 12), 5, 1448),
        (new DateOnly(2026, 11, 10), 6, 1448),
        (new DateOnly(2026, 12, 10), 7, 1448),
        (new DateOnly(2027, 1, 9), 8, 1448),
        (new DateOnly(2027, 2, 8), 9, 1448),
        (new DateOnly(2027, 3, 9), 10, 1448),
        (new DateOnly(2027, 4, 8), 11, 1448),
        (new DateOnly(2027, 5, 7), 12, 1448),
    ];

    private static readonly DateOnly KhgtRangeEnd = new(2027, 6, 5);
    // END KHGT-GENERATED

    private static readonly HijriCalendar FallbackHijri = new();

    /// <summary>e.g. "Sabtu, 12 September 2026".</summary>
    public static string FormatGregorianLong(DateOnly date)
    {
        var dayName = DayNames[(int)date.DayOfWeek];
        var monthName = GregorianMonthNames[date.Month - 1];
        return $"{dayName}, {date.Day} {monthName} {date.Year}";
    }

    /// <summary>e.g. "3 Rabiul Akhir 1448 H". Uses the KHGT month-start table above when the date falls
    /// within its covered range; outside that range it falls back to .NET's tabular Hijri calendar as a
    /// rough estimate (can differ by a day from KHGT/rukyat around month boundaries).</summary>
    public static string FormatHijri(DateOnly date)
    {
        if (date >= KhgtMonthStarts[0].Start && date <= KhgtRangeEnd)
        {
            var entry = KhgtMonthStarts.Last(m => m.Start <= date);
            var day = date.DayNumber - entry.Start.DayNumber + 1;
            return $"{day} {HijriMonthNames[entry.Month - 1]} {entry.Year} H";
        }

        var dt = date.ToDateTime(TimeOnly.MinValue);
        var fallbackDay = FallbackHijri.GetDayOfMonth(dt);
        var fallbackMonth = FallbackHijri.GetMonth(dt);
        var fallbackYear = FallbackHijri.GetYear(dt);
        return $"{fallbackDay} {HijriMonthNames[fallbackMonth - 1]} {fallbackYear} H";
    }
}
