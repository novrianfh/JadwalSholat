using JadwalSholat.Core.Models;

namespace JadwalSholat.Core.Services;

/// <summary>Computes "what's happening right now" from three adjacent days of schedule (Requirement.md #19-21).
/// Three days are required, not one, because the countdown must roll from Isha into tomorrow's Fajr at midnight,
/// and the active window can still belong to yesterday's Isha in the minutes just after midnight. Pure and stateless.</summary>
public static class PrayerStatusCalculator
{
    private static readonly TimeSpan ImminentThreshold = TimeSpan.FromSeconds(60);

    public static PrayerStatus Compute(PrayerDaySchedule? yesterday, PrayerDaySchedule today, PrayerDaySchedule? tomorrow, DateTime now)
    {
        var nextPrayer = FindNextPrayer(today, tomorrow, now);
        var countdown = nextPrayer.AzanDateTime - now;
        if (countdown < TimeSpan.Zero) countdown = TimeSpan.Zero;
        var isImminent = countdown <= ImminentThreshold;

        var currentPeriodName = FindCurrentPeriod(yesterday, today, now);
        var activeBannerName = FindActiveBanner(yesterday, today, now);
        var iqamahWaitEntry = FindIqamahWaitEntry(yesterday, today, now);

        return new PrayerStatus(now, nextPrayer, countdown, isImminent, currentPeriodName, activeBannerName, iqamahWaitEntry);
    }

    private static PrayerTimeEntry FindNextPrayer(PrayerDaySchedule today, PrayerDaySchedule? tomorrow, DateTime now)
    {
        var candidate = PrayerNames.Obligatory
            .Select(today.Get)
            .Where(e => e.AzanDateTime > now)
            .OrderBy(e => e.AzanDateTime)
            .FirstOrDefault();
        if (candidate is not null) return candidate;

        if (tomorrow is not null) return tomorrow.Get(PrayerName.Fajr);

        throw new InvalidOperationException(
            "Jadwal untuk besok belum tersedia. Perbarui data jadwal sholat di pengaturan.");
    }

    private static PrayerName FindCurrentPeriod(PrayerDaySchedule? yesterday, PrayerDaySchedule today, DateTime now)
    {
        var candidates = PrayerNames.Chronological.Select(today.Get);
        if (yesterday is not null) candidates = candidates.Concat(PrayerNames.Chronological.Select(yesterday.Get));

        var last = candidates
            .Where(e => e.AzanDateTime <= now)
            .OrderByDescending(e => e.AzanDateTime)
            .FirstOrDefault();

        // Before today's Fajr with no prior day loaded (first day the app ever ran): treat as still "in" Isha.
        return last?.Name ?? PrayerName.Isha;
    }

    private static PrayerName? FindActiveBanner(PrayerDaySchedule? yesterday, PrayerDaySchedule today, DateTime now)
    {
        var windows = PrayerNames.Obligatory.Select(today.Get);
        if (yesterday is not null) windows = windows.Append(yesterday.Get(PrayerName.Isha));

        return windows
            .Where(e => e.IqamahDateTime is not null && e.WindowEndDateTime is not null)
            .Where(e => now >= e.IqamahDateTime!.Value && now < e.WindowEndDateTime!.Value)
            .Select(e => (PrayerName?)e.Name)
            .FirstOrDefault();
    }

    /// <summary>The prayer whose Azan already happened but whose Iqamah hasn't yet — the window where the
    /// countdown display should target Iqamah rather than the next prayer's Azan.</summary>
    private static PrayerTimeEntry? FindIqamahWaitEntry(PrayerDaySchedule? yesterday, PrayerDaySchedule today, DateTime now)
    {
        var windows = PrayerNames.Obligatory.Select(today.Get);
        if (yesterday is not null) windows = windows.Append(yesterday.Get(PrayerName.Isha));

        return windows
            .Where(e => e.IqamahDateTime is not null)
            .FirstOrDefault(e => now >= e.AzanDateTime && now < e.IqamahDateTime!.Value);
    }
}
