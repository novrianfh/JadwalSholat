using JadwalSholat.Core.Models;

namespace JadwalSholat.Core.Services;

/// <summary>A day plus its immediate neighbours, exactly what <see cref="PrayerStatusCalculator"/> needs.</summary>
public sealed record ThreeDayWindow(PrayerDaySchedule? Yesterday, PrayerDaySchedule Today, PrayerDaySchedule? Tomorrow);

/// <summary>Per-city in-memory cache over <see cref="IPrayerTimeDataSource"/>, so the JSON dataset is fetched
/// once per city per app session, and applies the current <see cref="AppSettings"/> to build schedules.</summary>
public sealed class PrayerTimeRepository(IPrayerTimeDataSource dataSource)
{
    private readonly Dictionary<string, CityScheduleData> _cache = [];

    public async Task<CityScheduleData?> GetCityScheduleAsync(string cityId, CancellationToken ct = default)
    {
        if (_cache.TryGetValue(cityId, out var cached)) return cached;

        var data = await dataSource.LoadCityScheduleAsync(cityId, ct);
        if (data is not null) _cache[cityId] = data;
        return data;
    }

    /// <returns>Null if today's raw data isn't available for this city — the caller must show a
    /// "data belum tersedia" state rather than crash.</returns>
    public async Task<ThreeDayWindow?> GetWindowAsync(string cityId, AppSettings settings, DateOnly today, CancellationToken ct = default)
    {
        var city = await GetCityScheduleAsync(cityId, ct);
        if (city is null || !city.DaysByDate.TryGetValue(today, out var todayRaw)) return null;

        PrayerDaySchedule? BuildIfPresent(DateOnly date) =>
            city.DaysByDate.TryGetValue(date, out var raw) ? PrayerScheduleBuilder.Build(raw, settings) : null;

        var todaySchedule = PrayerScheduleBuilder.Build(todayRaw, settings);
        var yesterday = BuildIfPresent(today.AddDays(-1));
        var tomorrow = BuildIfPresent(today.AddDays(1));

        return new ThreeDayWindow(yesterday, todaySchedule, tomorrow);
    }
}
