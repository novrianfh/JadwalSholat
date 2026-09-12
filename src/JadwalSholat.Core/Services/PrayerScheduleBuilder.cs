using JadwalSholat.Core.Models;

namespace JadwalSholat.Core.Services;

/// <summary>Turns a raw API-sourced day into a fully adjusted schedule by applying the user's
/// time-adjustment, iqamah, and prayer-duration settings (Requirement.md #24-26). Pure and stateless.</summary>
public static class PrayerScheduleBuilder
{
    public static PrayerDaySchedule Build(DailyPrayerTimes raw, AppSettings settings)
    {
        var entries = new List<PrayerTimeEntry>(PrayerNames.Chronological.Count);

        foreach (var name in PrayerNames.Chronological)
        {
            var adjustmentMinutes = settings.TimeAdjustmentMinutes.GetValueOrDefault(name, 0);
            var azanDateTime = raw.Date.ToDateTime(raw.Get(name)).AddMinutes(adjustmentMinutes);

            DateTime? iqamahDateTime = null;
            DateTime? windowEndDateTime = null;
            if (PrayerNames.Obligatory.Contains(name))
            {
                var iqamahMinutes = settings.IqamahMinutes.GetValueOrDefault(name, 10);
                var durationMinutes = settings.PrayerDurationMinutes.GetValueOrDefault(name, 15);
                iqamahDateTime = azanDateTime.AddMinutes(iqamahMinutes);
                windowEndDateTime = iqamahDateTime.Value.AddMinutes(durationMinutes);
            }

            entries.Add(new PrayerTimeEntry(name, raw.Date, azanDateTime, iqamahDateTime, windowEndDateTime));
        }

        return new PrayerDaySchedule(raw.Date, entries);
    }
}
