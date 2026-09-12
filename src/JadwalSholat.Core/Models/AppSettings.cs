namespace JadwalSholat.Core.Models;

public enum LocationMode
{
    Manual,
    AutoDetect
}

/// <summary>The single settings aggregate persisted to LocalStorage.</summary>
public sealed class AppSettings
{
    public string CityId { get; set; } = "1301";
    public LocationMode LocationMode { get; set; } = LocationMode.Manual;
    public MosqueSettings Mosque { get; set; } = new();
    public WallpaperSettings Wallpaper { get; set; } = new();

    /// <summary>Minutes added to (or, if negative, subtracted from) each API-sourced clock time. Covers all 6 rows, including Shuruk.</summary>
    public Dictionary<PrayerName, int> TimeAdjustmentMinutes { get; set; } = PrayerNames.Chronological.ToDictionary(p => p, _ => 0);

    /// <summary>Minutes from Azan to Iqamah, per obligatory prayer. Default 10 (Requirement.md #25).</summary>
    public Dictionary<PrayerName, int> IqamahMinutes { get; set; } = PrayerNames.Obligatory.ToDictionary(p => p, _ => 10);

    /// <summary>Length in minutes of the "waktu sholat" active window, measured from Iqamah. Default 15 (Requirement.md #26).</summary>
    public Dictionary<PrayerName, int> PrayerDurationMinutes { get; set; } = PrayerNames.Obligatory.ToDictionary(p => p, _ => 15);

    public static AppSettings CreateDefault() => new();
}
