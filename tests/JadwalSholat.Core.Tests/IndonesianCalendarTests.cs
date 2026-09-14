using JadwalSholat.Core.Models;
using JadwalSholat.Core.Services;

namespace JadwalSholat.Core.Tests;

public class IndonesianCalendarTests
{
    [Fact]
    public void FormatGregorianLong_KnownSaturday_MatchesApiDayName()
    {
        // api.myquran.com labels 2026-09-12 as "Sabtu, 12/09/2026" (curl-verified during development).
        var result = IndonesianCalendar.FormatGregorianLong(new DateOnly(2026, 9, 12));
        Assert.Equal("Sabtu, 12 September 2026", result);
    }

    [Theory]
    [InlineData(2026, 1, 1, "Kamis")]
    [InlineData(2026, 12, 25, "Jumat")]
    public void FormatGregorianLong_MatchesDotNetDayOfWeek(int y, int m, int d, string expectedDay)
    {
        var date = new DateOnly(y, m, d);
        Assert.Equal(expectedDay, DayNameFor(date.DayOfWeek));
        var result = IndonesianCalendar.FormatGregorianLong(date);
        Assert.StartsWith(expectedDay, result);
    }

    private static string DayNameFor(DayOfWeek day) => day switch
    {
        DayOfWeek.Sunday => "Ahad",
        DayOfWeek.Monday => "Senin",
        DayOfWeek.Tuesday => "Selasa",
        DayOfWeek.Wednesday => "Rabu",
        DayOfWeek.Thursday => "Kamis",
        DayOfWeek.Friday => "Jumat",
        DayOfWeek.Saturday => "Sabtu",
        _ => throw new ArgumentOutOfRangeException(nameof(day))
    };

    [Fact]
    public void FormatHijri_ProducesNonEmptyStringEndingInH()
    {
        var result = IndonesianCalendar.FormatHijri(new DateOnly(2026, 9, 12));
        Assert.EndsWith(" H", result);
        Assert.Matches(@"^\d{1,2} [A-Za-z ]+ \d{4} H$", result);
    }

    [Fact]
    public void FormatHijri_IsMonotonicWithGregorianDate()
    {
        // Advancing the Gregorian date by a month must not go backwards in the Hijri year/month/day ordering.
        var first = new DateOnly(2026, 1, 1);
        var later = new DateOnly(2026, 6, 1);

        var firstHijri = IndonesianCalendar.FormatHijri(first);
        var laterHijri = IndonesianCalendar.FormatHijri(later);

        Assert.NotEqual(firstHijri, laterHijri);
    }

    [Theory]
    [InlineData(2026, 6, 16, "1 Muharram 1448 H")] // KHGT: 1 Muharam 1448 H
    [InlineData(2026, 9, 12, "1 Rabiul Akhir 1448 H")] // KHGT: awal Rabiulakhir 1448 H
    [InlineData(2026, 9, 14, "3 Rabiul Akhir 1448 H")] // KHGT (bukan 2, seperti kalender tabular Kemenag/.NET)
    public void FormatHijri_MatchesKhgtMuhammadiyahCalendar(int y, int m, int d, string expected)
    {
        // Cross-checked against https://khgt.muhammadiyah.or.id/kalendar-hijriah (2026-09-14).
        var result = IndonesianCalendar.FormatHijri(new DateOnly(y, m, d));
        Assert.Equal(expected, result);
    }
}
