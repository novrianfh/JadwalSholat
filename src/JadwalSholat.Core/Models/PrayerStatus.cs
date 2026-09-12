namespace JadwalSholat.Core.Models;

/// <summary>A point-in-time read of "what's happening right now", computed by <see cref="Services.PrayerStatusCalculator"/>.</summary>
public sealed record PrayerStatus(
    DateTime Now,
    PrayerTimeEntry NextPrayer,
    TimeSpan CountdownToNext,
    bool IsImminent,
    PrayerName CurrentPeriodPrayerName,
    PrayerName? ActiveBannerPrayerName,
    PrayerTimeEntry? IqamahWaitEntry)
{
    /// <summary>True in the last 60 seconds before <see cref="NextPrayer"/>'s Azan (Requirement.md #20).</summary>
    public bool HasActiveBanner => ActiveBannerPrayerName is not null;

    /// <summary>True between a prayer's Azan and its own Iqamah — the countdown display should switch from
    /// "menuju {next prayer}" to "menuju Iqamah {this prayer}" during this window.</summary>
    public bool HasIqamahCountdown => IqamahWaitEntry is not null;

    public TimeSpan CountdownToIqamah
    {
        get
        {
            if (IqamahWaitEntry is null) return TimeSpan.Zero;
            var span = IqamahWaitEntry.IqamahDateTime!.Value - Now;
            return span < TimeSpan.Zero ? TimeSpan.Zero : span;
        }
    }

    /// <summary>Which row a grid for <paramref name="gridDate"/> should highlight (Requirement.md #17) —
    /// the upcoming prayer, e.g. at 11:03 with Dzuhur at 11:38, Dzuhur lights up rather than whatever prayer's
    /// period we're currently in. Null once <see cref="NextPrayer"/> has rolled over past <paramref name="gridDate"/>
    /// (e.g. all evening after Isha, when the next prayer is tomorrow's Fajr): nothing in today's grid is
    /// "upcoming" any more at that point, so today's Fajr row must not light up as if it were.</summary>
    public PrayerName? HighlightedPrayerNameFor(DateOnly gridDate) =>
        NextPrayer.NominalDate == gridDate ? NextPrayer.Name : null;
}
