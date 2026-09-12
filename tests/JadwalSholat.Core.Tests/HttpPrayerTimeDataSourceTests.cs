using System.Net;
using JadwalSholat.Core.Models;
using JadwalSholat.Core.Services;

namespace JadwalSholat.Core.Tests;

public class HttpPrayerTimeDataSourceTests
{
    private const string SampleJson = """
    {
      "cityId": "1301",
      "cityName": "Jakarta",
      "generatedAtUtc": "2026-09-12T00:00:00Z",
      "source": "api.myquran.com",
      "days": [
        { "date": "2026-09-12", "fajr": "04:33", "shuruk": "05:44", "dhuhr": "11:53", "asr": "15:07", "maghrib": "17:54", "isha": "19:02" }
      ]
    }
    """;

    [Fact]
    public async Task LoadCityScheduleAsync_ParsesDaysIntoDomainObjects()
    {
        var handler = new FakeHttpMessageHandler(_ => FakeHttpMessageHandler.Json(SampleJson));
        var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
        var source = new HttpPrayerTimeDataSource(client);

        var result = await source.LoadCityScheduleAsync("1301");

        Assert.NotNull(result);
        Assert.Equal("Jakarta", result!.CityName);
        var day = result.DaysByDate[new DateOnly(2026, 9, 12)];
        Assert.Equal(new TimeOnly(4, 33), day.Fajr);
        Assert.Equal(new TimeOnly(19, 2), day.Isha);
    }

    [Fact]
    public async Task LoadCityScheduleAsync_RequestsExpectedRelativePath()
    {
        HttpRequestMessage? captured = null;
        var handler = new FakeHttpMessageHandler(req => { captured = req; return FakeHttpMessageHandler.Json(SampleJson); });
        var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
        var source = new HttpPrayerTimeDataSource(client);

        await source.LoadCityScheduleAsync("1301");

        Assert.Equal("/data/prayertimes/1301.json", captured!.RequestUri!.AbsolutePath);
    }

    [Fact]
    public async Task LoadCityScheduleAsync_NotFound_ReturnsNullInsteadOfThrowing()
    {
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.NotFound));
        var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
        var source = new HttpPrayerTimeDataSource(client);

        var result = await source.LoadCityScheduleAsync("9999");

        Assert.Null(result);
    }
}
