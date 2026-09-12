using JadwalSholat.Core.Models;
using JadwalSholat.Core.Services;
using Moq;

namespace JadwalSholat.Core.Tests;

public class PrayerTimeRepositoryTests
{
    private static readonly DateOnly Today = new(2026, 9, 12);

    private static CityScheduleData MakeCityData(params DateOnly[] dates)
    {
        var byDate = dates.ToDictionary(d => d, d => TestData.RawDay(d));
        return new CityScheduleData("1301", "Jakarta", byDate);
    }

    [Fact]
    public async Task GetCityScheduleAsync_CachesAfterFirstLoad()
    {
        var mock = new Mock<IPrayerTimeDataSource>();
        mock.Setup(m => m.LoadCityScheduleAsync("1301", It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeCityData(Today));
        var repo = new PrayerTimeRepository(mock.Object);

        await repo.GetCityScheduleAsync("1301");
        await repo.GetCityScheduleAsync("1301");

        mock.Verify(m => m.LoadCityScheduleAsync("1301", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetWindowAsync_TodayMissing_ReturnsNull()
    {
        var mock = new Mock<IPrayerTimeDataSource>();
        mock.Setup(m => m.LoadCityScheduleAsync("1301", It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeCityData(Today.AddDays(-1))); // only yesterday, no "today"
        var repo = new PrayerTimeRepository(mock.Object);

        var window = await repo.GetWindowAsync("1301", TestData.DefaultSettings(), Today);

        Assert.Null(window);
    }

    [Fact]
    public async Task GetWindowAsync_AllThreeDaysPresent_BuildsFullWindow()
    {
        var mock = new Mock<IPrayerTimeDataSource>();
        mock.Setup(m => m.LoadCityScheduleAsync("1301", It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeCityData(Today.AddDays(-1), Today, Today.AddDays(1)));
        var repo = new PrayerTimeRepository(mock.Object);

        var window = await repo.GetWindowAsync("1301", TestData.DefaultSettings(), Today);

        Assert.NotNull(window);
        Assert.NotNull(window!.Yesterday);
        Assert.NotNull(window.Tomorrow);
        Assert.Equal(Today, window.Today.Date);
    }

    [Fact]
    public async Task GetWindowAsync_MissingNeighbours_LeavesThemNullWithoutFailing()
    {
        var mock = new Mock<IPrayerTimeDataSource>();
        mock.Setup(m => m.LoadCityScheduleAsync("1301", It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeCityData(Today)); // last day of the loaded dataset: no tomorrow
        var repo = new PrayerTimeRepository(mock.Object);

        var window = await repo.GetWindowAsync("1301", TestData.DefaultSettings(), Today);

        Assert.NotNull(window);
        Assert.Null(window!.Yesterday);
        Assert.Null(window.Tomorrow);
    }

    [Fact]
    public async Task GetCityScheduleAsync_UnknownCity_ReturnsNullAndDoesNotCache()
    {
        var mock = new Mock<IPrayerTimeDataSource>();
        mock.Setup(m => m.LoadCityScheduleAsync("9999", It.IsAny<CancellationToken>()))
            .ReturnsAsync((CityScheduleData?)null);
        var repo = new PrayerTimeRepository(mock.Object);

        var first = await repo.GetCityScheduleAsync("9999");
        var second = await repo.GetCityScheduleAsync("9999");

        Assert.Null(first);
        Assert.Null(second);
        mock.Verify(m => m.LoadCityScheduleAsync("9999", It.IsAny<CancellationToken>()), Times.Exactly(2));
    }
}
