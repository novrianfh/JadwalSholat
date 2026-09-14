using JadwalSholat.Core.Models;

namespace JadwalSholat.Core.Tests;

public class CityTests
{
    [Fact]
    public void Bundled_ContainsAllProvincialCapitalsAndCoreCities()
    {
        var names = City.Bundled.Select(c => c.Name).ToHashSet();
        var expectedCore = new[]
        {
            "Banda Aceh", "Medan", "Padang", "Pekanbaru", "Tanjungpinang", "Jambi", "Palembang",
            "Pangkal Pinang", "Bengkulu", "Bandar Lampung", "Jakarta", "Serang", "Bandung",
            "Semarang", "Yogyakarta", "Surabaya", "Denpasar", "Mataram", "Kupang", "Pontianak",
            "Palangka Raya", "Banjarmasin", "Samarinda", "Tanjung Selor", "Manado", "Gorontalo",
            "Palu", "Mamuju", "Makassar", "Kendari", "Ambon", "Sofifi", "Manokwari", "Sorong",
            "Jayapura", "Nabire", "Wamena", "Merauke",
        };
        Assert.All(expectedCore, name => Assert.Contains(name, names));
        Assert.Equal(50, City.Bundled.Count);
    }

    [Fact]
    public void Bundled_HasNoDuplicateIds()
    {
        var ids = City.Bundled.Select(c => c.Id).ToList();
        Assert.Equal(ids.Count, ids.Distinct().Count());
    }

    [Theory]
    [InlineData(-6.2, 106.8, "Jakarta")]
    [InlineData(-7.25, 112.75, "Surabaya")]
    [InlineData(-7.8, 110.36, "Yogyakarta")]
    [InlineData(3.59, 98.67, "Medan")]
    [InlineData(-5.15, 119.43, "Makassar")]
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
