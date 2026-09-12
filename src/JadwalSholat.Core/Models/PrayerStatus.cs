namespace JadwalSholat.Core.Models;

/// <summary>A point-in-time read of "what's happening right now", computed by <see cref="Services.PrayerStatusCalculator"/>.</summary>
public sealed record PrayerStatus(
    DateTime Now,
    PrayerTimeEntry NextPrayer,
    TimeSpan CountdownToNext,
    bool IsImminent,
    PrayerName CurrentPeriodPrayerName,
    PrayerName? ActiveBannerPrayerName)
{
    /// <summary>True in the last 60 seconds before <see cref="NextPrayer"/>'s Azan (Requirement.md #20).</summary>
    public bool HasActiveBanner => ActiveBannerPrayerName is not null;
}
