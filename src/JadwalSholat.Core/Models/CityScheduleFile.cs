using System.Text.Json.Serialization;

namespace JadwalSholat.Core.Models;

/// <summary>Wire format of wwwroot/data/prayertimes/{cityId}.json, produced by tools/generate_prayer_times.py.
/// This is the contract between the offline data generator and the app — keep both in sync if it changes.</summary>
public sealed class CityScheduleFileDto
{
    [JsonPropertyName("cityId")]
    public string CityId { get; set; } = "";

    [JsonPropertyName("cityName")]
    public string CityName { get; set; } = "";

    [JsonPropertyName("generatedAtUtc")]
    public DateTime GeneratedAtUtc { get; set; }

    [JsonPropertyName("source")]
    public string Source { get; set; } = "";

    [JsonPropertyName("days")]
    public List<DailyPrayerTimesDto> Days { get; set; } = [];
}

public sealed class DailyPrayerTimesDto
{
    [JsonPropertyName("date")]
    public string Date { get; set; } = "";

    [JsonPropertyName("fajr")]
    public string Fajr { get; set; } = "";

    [JsonPropertyName("shuruk")]
    public string Shuruk { get; set; } = "";

    [JsonPropertyName("dhuhr")]
    public string Dhuhr { get; set; } = "";

    [JsonPropertyName("asr")]
    public string Asr { get; set; } = "";

    [JsonPropertyName("maghrib")]
    public string Maghrib { get; set; } = "";

    [JsonPropertyName("isha")]
    public string Isha { get; set; } = "";

    public DailyPrayerTimes ToDomain() => new()
    {
        Date = DateOnly.Parse(Date),
        Fajr = TimeOnly.Parse(Fajr),
        Shuruk = TimeOnly.Parse(Shuruk),
        Dhuhr = TimeOnly.Parse(Dhuhr),
        Asr = TimeOnly.Parse(Asr),
        Maghrib = TimeOnly.Parse(Maghrib),
        Isha = TimeOnly.Parse(Isha),
    };
}
