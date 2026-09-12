using JadwalSholat.Core.Models;

namespace JadwalSholat.Core.Tests;

public class CityTests
{
    [Fact]
    public void Bundled_HasExactlyTheFiveRequiredCities()
    {
        var names = City.Bundled.Select(c => c.Name).OrderBy(n => n);
        Assert.Equal(
            new[] { "Bandung", "Jakarta", "Semarang", "Surabaya", "Yogyakarta" }.OrderBy(n => n),
            names);
    }

    [Theory]
    [InlineData(-6.2, 106.8, "Jakarta")]
    [InlineData(-7.25, 112.75, "Surabaya")]
    [InlineData(-7.8, 110.36, "Yogyakarta")]
    public void Nearest_ReturnsClosestBundledCity(double lat, double lon, string expectedCity)
    {
        var nearest = City.Nearest(lat, lon);
        Assert.Equal(expectedCity, nearest.Name);
    }

    [Fact]
    public void FindById_UnknownId_ReturnsNull()
    {
        Assert.Null(City.FindById("9999"));
    }

    [Fact]
    public void FindById_KnownId_ReturnsCity()
    {
        Assert.Equal("Jakarta", City.FindById("1301")?.Name);
    }
}
