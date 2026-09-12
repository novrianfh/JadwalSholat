using System.Net;
using System.Net.Http.Json;
using JadwalSholat.Core.Models;

namespace JadwalSholat.Core.Services;

/// <summary>Fetches a city's dataset from "data/prayertimes/{cityId}.json" relative to the supplied HttpClient's
/// BaseAddress. In the Blazor WASM app that resolves to the static file bundled under wwwroot.</summary>
public sealed class HttpPrayerTimeDataSource(HttpClient httpClient) : IPrayerTimeDataSource
{
    public async Task<CityScheduleData?> LoadCityScheduleAsync(string cityId, CancellationToken ct = default)
    {
        var response = await httpClient.GetAsync($"data/prayertimes/{cityId}.json", ct);
        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        response.EnsureSuccessStatusCode();

        var dto = await response.Content.ReadFromJsonAsync<CityScheduleFileDto>(cancellationToken: ct);
        if (dto is null) return null;

        var byDate = dto.Days.Select(d => d.ToDomain()).ToDictionary(d => d.Date);
        return new CityScheduleData(dto.CityId, dto.CityName, byDate);
    }
}
