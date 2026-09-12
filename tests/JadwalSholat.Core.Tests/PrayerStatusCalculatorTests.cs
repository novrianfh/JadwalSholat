using JadwalSholat.Core.Models;
using JadwalSholat.Core.Services;

namespace JadwalSholat.Core.Tests;

public class PrayerStatusCalculatorTests
{
    private static readonly DateOnly Today = new(2026, 9, 12);
    private static readonly AppSettings Settings = TestData.DefaultSettings(); // iqamah 10, duration 15

    private static PrayerDaySchedule Schedule(DateOnly date) => PrayerScheduleBuilder.Build(TestData.RawDay(date), Settings);

    // ---- NextPrayer ----

    [Fact]
    public void NextPrayer_BeforeFajr_IsTodaysFajr()
    {
        var yesterday = Schedule(Today.AddDays(-1));
        var today = Schedule(Today);
        var now = today.Get(PrayerName.Fajr).AzanDateTime.AddMinutes(-30);

        var status = PrayerStatusCalculator.Compute(yesterday, today, null, now);

        Assert.Equal(PrayerName.Fajr, status.NextPrayer.Name);
        Assert.Equal(TimeSpan.FromMinutes(30), status.CountdownToNext);
    }

    [Fact]
    public void NextPrayer_BetweenDhuhrAndAsr_IsAsr()
    {
        var today = Schedule(Today);
        var now = today.Get(PrayerName.Dhuhr).AzanDateTime.AddHours(1);

        var status = PrayerStatusCalculator.Compute(null, today, null, now);

        Assert.Equal(PrayerName.Asr, status.NextPrayer.Name);
    }

    [Fact]
    public void NextPrayer_AfterIsha_RollsOverToTomorrowsFajr()
    {
        var today = Schedule(Today);
        var tomorrow = Schedule(Today.AddDays(1));
        var now = today.Get(PrayerName.Isha).AzanDateTime.AddHours(2);

        var status = PrayerStatusCalculator.Compute(null, today, tomorrow, now);

        Assert.Equal(PrayerName.Fajr, status.NextPrayer.Name);
        Assert.Equal(Today.AddDays(1), status.NextPrayer.NominalDate);
    }

    [Fact]
    public void NextPrayer_AfterIsha_WithNoTomorrowData_ThrowsDescriptiveError()
    {
        var today = Schedule(Today);
        var now = today.Get(PrayerName.Isha).AzanDateTime.AddMinutes(1);

        var ex = Assert.Throws<InvalidOperationException>(() =>
            PrayerStatusCalculator.Compute(null, today, null, now));
        Assert.Contains("besok", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void NextPrayer_ExactlyAtAzanTime_IsNotCountedAsUpcoming_MovesToFollowingPrayer()
    {
        var today = Schedule(Today);
        var now = today.Get(PrayerName.Dhuhr).AzanDateTime; // exactly on the dot

        var status = PrayerStatusCalculator.Compute(null, today, null, now);

        Assert.Equal(PrayerName.Asr, status.NextPrayer.Name);
    }

    // ---- IsImminent ----

    [Theory]
    [InlineData(60, true)]
    [InlineData(59, true)]
    [InlineData(61, false)]
    [InlineData(1, true)]
    public void IsImminent_ThresholdIsExactlySixtySecondsBeforeAzan(int secondsBefore, bool expectedImminent)
    {
        var today = Schedule(Today);
        var now = today.Get(PrayerName.Maghrib).AzanDateTime.AddSeconds(-secondsBefore);

        var status = PrayerStatusCalculator.Compute(null, today, null, now);

        Assert.Equal(expectedImminent, status.IsImminent);
    }

    // ---- CurrentPeriodPrayerName ----

    [Fact]
    public void CurrentPeriod_JustAfterAsrAzan_IsAsr()
    {
        var today = Schedule(Today);
        var now = today.Get(PrayerName.Asr).AzanDateTime.AddMinutes(1);

        var status = PrayerStatusCalculator.Compute(null, today, null, now);

        Assert.Equal(PrayerName.Asr, status.CurrentPeriodPrayerName);
    }

    [Fact]
    public void CurrentPeriod_BeforeTodaysFajr_IsYesterdaysIsha()
    {
        var yesterday = Schedule(Today.AddDays(-1));
        var today = Schedule(Today);
        var now = today.Get(PrayerName.Fajr).AzanDateTime.AddMinutes(-10);

        var status = PrayerStatusCalculator.Compute(yesterday, today, null, now);

        Assert.Equal(PrayerName.Isha, status.CurrentPeriodPrayerName);
    }

    [Fact]
    public void CurrentPeriod_BeforeTodaysFajr_WithNoYesterdayData_FallsBackToIsha()
    {
        var today = Schedule(Today);
        var now = today.Get(PrayerName.Fajr).AzanDateTime.AddMinutes(-10);

        var status = PrayerStatusCalculator.Compute(null, today, null, now);

        Assert.Equal(PrayerName.Isha, status.CurrentPeriodPrayerName);
    }

    [Fact]
    public void CurrentPeriod_AtExactAzanMoment_SwitchesToThatPrayer()
    {
        var today = Schedule(Today);
        var now = today.Get(PrayerName.Maghrib).AzanDateTime;

        var status = PrayerStatusCalculator.Compute(null, today, null, now);

        Assert.Equal(PrayerName.Maghrib, status.CurrentPeriodPrayerName);
    }

    [Fact]
    public void CurrentPeriod_RecognisesShurukAsACurrentPeriodEvenThoughItIsNotObligatory()
    {
        var today = Schedule(Today);
        var now = today.Get(PrayerName.Shuruk).AzanDateTime.AddMinutes(5);

        var status = PrayerStatusCalculator.Compute(null, today, null, now);

        Assert.Equal(PrayerName.Shuruk, status.CurrentPeriodPrayerName);
    }

    // ---- ActiveBannerPrayerName ("Waktu sholat {nama}") ----

    [Fact]
    public void ActiveBanner_DuringWindow_ShowsThatPrayer()
    {
        var today = Schedule(Today);
        var dhuhr = today.Get(PrayerName.Dhuhr);
        var now = dhuhr.IqamahDateTime!.Value.AddMinutes(5); // mid-window (10 iqamah, 15 duration)

        var status = PrayerStatusCalculator.Compute(null, today, null, now);

        Assert.Equal(PrayerName.Dhuhr, status.ActiveBannerPrayerName);
        Assert.True(status.HasActiveBanner);
    }

    [Fact]
    public void ActiveBanner_AtIqamahMoment_IsInclusive()
    {
        var today = Schedule(Today);
        var now = today.Get(PrayerName.Asr).IqamahDateTime!.Value;

        var status = PrayerStatusCalculator.Compute(null, today, null, now);

        Assert.Equal(PrayerName.Asr, status.ActiveBannerPrayerName);
    }

    [Fact]
    public void ActiveBanner_AtWindowEndMoment_IsExclusive()
    {
        var today = Schedule(Today);
        var now = today.Get(PrayerName.Asr).WindowEndDateTime!.Value;

        var status = PrayerStatusCalculator.Compute(null, today, null, now);

        Assert.Null(status.ActiveBannerPrayerName);
    }

    [Fact]
    public void ActiveBanner_BetweenAzanAndIqamah_IsNotYetActive()
    {
        var today = Schedule(Today);
        var dhuhr = today.Get(PrayerName.Dhuhr);
        var now = dhuhr.AzanDateTime.AddMinutes(1); // before iqamah (10 min later)

        var status = PrayerStatusCalculator.Compute(null, today, null, now);

        Assert.Null(status.ActiveBannerPrayerName);
    }

    [Fact]
    public void ActiveBanner_OutsideAnyWindow_IsNull()
    {
        var today = Schedule(Today);
        var now = today.Get(PrayerName.Dhuhr).AzanDateTime.AddHours(1);

        var status = PrayerStatusCalculator.Compute(null, today, null, now);

        Assert.Null(status.ActiveBannerPrayerName);
    }

    [Fact]
    public void ActiveBanner_IshaWindowCrossingMidnight_StillActiveJustAfterMidnight()
    {
        // Isha at 23:58, iqamah +10 -> 00:08, duration 15 -> window end 00:23, all rolling into "today".
        var yesterdayRaw = TestData.RawDay(Today.AddDays(-1), isha: new TimeOnly(23, 58));
        var yesterday = PrayerScheduleBuilder.Build(yesterdayRaw, Settings);
        var today = Schedule(Today);

        var now = Today.ToDateTime(new TimeOnly(0, 15)); // inside the rolled-over window

        var status = PrayerStatusCalculator.Compute(yesterday, today, null, now);

        Assert.Equal(PrayerName.Isha, status.ActiveBannerPrayerName);
    }

    // ---- HighlightedPrayerNameFor (which grid row lights up) ----

    [Fact]
    public void HighlightedPrayerNameFor_UpcomingPrayerLaterToday_ReturnsIt()
    {
        // now 11:03, Dzuhur 11:38 -> Dzuhur is highlighted (the upcoming prayer), not Terbit (the current period).
        var today = Schedule(Today);
        var now = Today.ToDateTime(new TimeOnly(11, 3));

        var status = PrayerStatusCalculator.Compute(null, today, null, now);

        Assert.Equal(PrayerName.Dhuhr, status.NextPrayer.Name);
        Assert.Equal(PrayerName.Dhuhr, status.HighlightedPrayerNameFor(Today));
    }

    [Fact]
    public void HighlightedPrayerNameFor_NextPrayerRolledOverToTomorrow_ReturnsNullForTodaysGrid()
    {
        // After Isha, NextPrayer is tomorrow's Fajr. Today's Fajr row already happened hours ago and
        // must not light up just because the *name* "Fajr" matches — it belongs to a different date.
        var today = Schedule(Today);
        var tomorrow = Schedule(Today.AddDays(1));
        var now = today.Get(PrayerName.Isha).AzanDateTime.AddHours(2);

        var status = PrayerStatusCalculator.Compute(null, today, tomorrow, now);

        Assert.Equal(PrayerName.Fajr, status.NextPrayer.Name); // tomorrow's Fajr
        Assert.Null(status.HighlightedPrayerNameFor(Today)); // nothing lights up in *today's* grid
        Assert.Equal(PrayerName.Fajr, status.HighlightedPrayerNameFor(Today.AddDays(1))); // but tomorrow's grid would
    }
}
