using JadwalSholat.Core.Models;
using JadwalSholat.Core.Services;

namespace JadwalSholat.Core.Tests;

public class PrayerScheduleBuilderTests
{
    private static readonly DateOnly Date = new(2026, 9, 12);

    [Fact]
    public void Build_WithDefaultSettings_AzanMatchesRawTime()
    {
        var raw = TestData.RawDay(Date);
        var schedule = PrayerScheduleBuilder.Build(raw, TestData.DefaultSettings());

        Assert.Equal(new TimeOnly(4, 33), schedule.Get(PrayerName.Fajr).AzanTime);
        Assert.Equal(new TimeOnly(11, 53), schedule.Get(PrayerName.Dhuhr).AzanTime);
        Assert.Equal(new TimeOnly(19, 2), schedule.Get(PrayerName.Isha).AzanTime);
    }

    [Fact]
    public void Build_ObligatoryPrayer_IqamahIsAzanPlusIqamahMinutes()
    {
        var raw = TestData.RawDay(Date);
        var settings = TestData.DefaultSettings();
        settings.IqamahMinutes[PrayerName.Dhuhr] = 12;

        var schedule = PrayerScheduleBuilder.Build(raw, settings);
        var dhuhr = schedule.Get(PrayerName.Dhuhr);

        Assert.Equal(dhuhr.AzanDateTime.AddMinutes(12), dhuhr.IqamahDateTime);
    }

    [Fact]
    public void Build_ObligatoryPrayer_WindowEndIsIqamahPlusDurationMinutes()
    {
        var raw = TestData.RawDay(Date);
        var settings = TestData.DefaultSettings();
        settings.IqamahMinutes[PrayerName.Maghrib] = 5;
        settings.PrayerDurationMinutes[PrayerName.Maghrib] = 20;

        var schedule = PrayerScheduleBuilder.Build(raw, settings);
        var maghrib = schedule.Get(PrayerName.Maghrib);

        Assert.Equal(maghrib.AzanDateTime.AddMinutes(5), maghrib.IqamahDateTime);
        Assert.Equal(maghrib.AzanDateTime.AddMinutes(5 + 20), maghrib.WindowEndDateTime);
    }

    [Fact]
    public void Build_Shuruk_HasNoIqamahOrWindow()
    {
        var raw = TestData.RawDay(Date);
        var schedule = PrayerScheduleBuilder.Build(raw, TestData.DefaultSettings());
        var shuruk = schedule.Get(PrayerName.Shuruk);

        Assert.Null(shuruk.IqamahDateTime);
        Assert.Null(shuruk.WindowEndDateTime);
    }

    [Theory]
    [InlineData(5)]
    [InlineData(-5)]
    public void Build_TimeAdjustment_ShiftsAzanByConfiguredMinutes(int adjustment)
    {
        var raw = TestData.RawDay(Date);
        var settings = TestData.DefaultSettings();
        settings.TimeAdjustmentMinutes[PrayerName.Asr] = adjustment;

        var schedule = PrayerScheduleBuilder.Build(raw, settings);

        Assert.Equal(new TimeOnly(15, 7).AddMinutes(adjustment), schedule.Get(PrayerName.Asr).AzanTime);
    }

    [Fact]
    public void Build_NegativeAdjustment_CanRollAzanIntoPreviousCalendarDay()
    {
        // Fajr at 00:05 with a -10 minute adjustment rolls to 23:55 the day before.
        var raw = TestData.RawDay(Date, fajr: new TimeOnly(0, 5));
        var settings = TestData.DefaultSettings();
        settings.TimeAdjustmentMinutes[PrayerName.Fajr] = -10;

        var schedule = PrayerScheduleBuilder.Build(raw, settings);
        var fajr = schedule.Get(PrayerName.Fajr);

        Assert.Equal(Date.AddDays(-1), DateOnly.FromDateTime(fajr.AzanDateTime));
        Assert.Equal(new TimeOnly(23, 55), fajr.AzanTime);
        // NominalDate still reflects the source dataset day, independent of the rollover.
        Assert.Equal(Date, fajr.NominalDate);
    }

    [Fact]
    public void Build_AllSixPrayersPresent_InChronologicalOrder()
    {
        var raw = TestData.RawDay(Date);
        var schedule = PrayerScheduleBuilder.Build(raw, TestData.DefaultSettings());

        Assert.Equal(
            [PrayerName.Fajr, PrayerName.Shuruk, PrayerName.Dhuhr, PrayerName.Asr, PrayerName.Maghrib, PrayerName.Isha],
            schedule.Entries.Select(e => e.Name));
        Assert.True(schedule.Entries.Zip(schedule.Entries.Skip(1))
            .All(pair => pair.First.AzanDateTime < pair.Second.AzanDateTime));
    }
}
