using JadwalSholat.Core.Models;

namespace JadwalSholat.Core.Services;

public sealed record CityScheduleData(string CityId, string CityName, IReadOnlyDictionary<DateOnly, DailyPrayerTimes> DaysByDate);

/// <summary>Loads one city's whole pre-fetched year of prayer times. Implemented against HttpClient in the Web
/// app (reading the bundled wwwroot/data/prayertimes/*.json) and against a fake handler in tests.</summary>
public interface IPrayerTimeDataSource
{
    Task<CityScheduleData?> LoadCityScheduleAsync(string cityId, CancellationToken ct = default);
}
