using JadwalSholat.Core.Models;

namespace JadwalSholat.Core.Tests;

public class AppSettingsTests
{
    [Fact]
    public void CreateDefault_TimeAdjustment_CoversAllSixPrayersAtZero()
    {
        var settings = AppSettings.CreateDefault();
        Assert.Equal(6, settings.TimeAdjustmentMinutes.Count);
        Assert.All(settings.TimeAdjustmentMinutes.Values, v => Assert.Equal(0, v));
    }

    [Fact]
    public void CreateDefault_Iqamah_CoversFiveObligatoryPrayersAtTenMinutes()
    {
        var settings = AppSettings.CreateDefault();
        Assert.Equal(5, settings.IqamahMinutes.Count);
        Assert.DoesNotContain(PrayerName.Shuruk, settings.IqamahMinutes.Keys);
        Assert.All(settings.IqamahMinutes.Values, v => Assert.Equal(10, v));
    }

    [Fact]
    public void CreateDefault_Duration_CoversFiveObligatoryPrayersAtFifteenMinutes()
    {
        var settings = AppSettings.CreateDefault();
        Assert.Equal(5, settings.PrayerDurationMinutes.Count);
        Assert.DoesNotContain(PrayerName.Shuruk, settings.PrayerDurationMinutes.Keys);
        Assert.All(settings.PrayerDurationMinutes.Values, v => Assert.Equal(15, v));
    }

    [Fact]
    public void CreateDefault_Mosque_HasNonEmptyName()
    {
        var settings = AppSettings.CreateDefault();
        Assert.False(string.IsNullOrWhiteSpace(settings.Mosque.Name));
    }

    [Fact]
    public void CreateDefault_Wallpaper_IntervalIsTenSeconds()
    {
        var settings = AppSettings.CreateDefault();
        Assert.Equal(10, settings.Wallpaper.IntervalSeconds);
    }
}
