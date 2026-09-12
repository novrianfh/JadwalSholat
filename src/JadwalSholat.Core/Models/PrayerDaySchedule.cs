namespace JadwalSholat.Core.Models;

/// <summary>One prayer's fully-adjusted times for a given day: Azan (raw API time + the user's minute adjustment),
/// and, for the 5 obligatory prayers, Iqamah and the end of the active "waktu sholat" window.
/// All three are full <see cref="DateTime"/> values (not bare <see cref="TimeOnly"/>) because a large adjustment,
/// or an Isha window running late, can legitimately roll a time past midnight into the next calendar day.</summary>
public sealed record PrayerTimeEntry(PrayerName Name, DateOnly NominalDate, DateTime AzanDateTime, DateTime? IqamahDateTime, DateTime? WindowEndDateTime)
{
    public TimeOnly AzanTime => TimeOnly.FromDateTime(AzanDateTime);
    public TimeOnly? IqamahTime => IqamahDateTime is { } d ? TimeOnly.FromDateTime(d) : null;
    public TimeOnly? WindowEndTime => WindowEndDateTime is { } d ? TimeOnly.FromDateTime(d) : null;
}

/// <summary>All 6 rows for one nominal date, with settings already applied. Produced by <see cref="Services.PrayerScheduleBuilder"/>.</summary>
public sealed record PrayerDaySchedule(DateOnly Date, IReadOnlyList<PrayerTimeEntry> Entries)
{
    public PrayerTimeEntry Get(PrayerName name) => Entries.First(e => e.Name == name);
}
