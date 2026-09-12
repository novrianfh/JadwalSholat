using Bunit;
using JadwalSholat.Core.Models;
using JadwalSholat.Core.Services;
using JadwalSholat.Web.Components;

namespace JadwalSholat.Web.Tests;

public class ClockPanelTests : BunitContext
{
    private static readonly DateOnly Date = new(2026, 9, 12);
    private static readonly AppSettings Settings = AppSettings.CreateDefault();

    private static PrayerDaySchedule Schedule() =>
        PrayerScheduleBuilder.Build(new DailyPrayerTimes
        {
            Date = Date,
            Fajr = new TimeOnly(4, 33),
            Shuruk = new TimeOnly(5, 44),
            Dhuhr = new TimeOnly(11, 53),
            Asr = new TimeOnly(15, 7),
            Maghrib = new TimeOnly(17, 54),
            Isha = new TimeOnly(19, 2),
        }, Settings);

    [Fact]
    public void RendersCurrentTime_SplitAcrossHourMinuteAndSecondsInFocusMode()
    {
        // Focus mode (Compact=false, the default) splits HH:mm from :ss onto separate elements so the
        // main digits can grow far larger than a single "HH:mm:ss" line would allow (see app.css).
        var schedule = Schedule();
        var now = Date.ToDateTime(new TimeOnly(10, 30, 15));
        var status = PrayerStatusCalculator.Compute(null, schedule, null, now);

        var cut = Render<ClockPanel>(p => p.Add(x => x.Status, status).Add(x => x.Now, now));

        Assert.Equal("10:30", cut.Find(".clock-panel-time-main").TextContent);
        Assert.Equal(":15", cut.Find(".clock-panel-time-seconds").TextContent);
    }

    [Fact]
    public void RendersCurrentTime_AsOneUnsplitLineInCompactMode()
    {
        var schedule = Schedule();
        var now = Date.ToDateTime(new TimeOnly(10, 30, 15));
        var status = PrayerStatusCalculator.Compute(null, schedule, null, now);

        var cut = Render<ClockPanel>(p => p
            .Add(x => x.Status, status)
            .Add(x => x.Now, now)
            .Add(x => x.Compact, true));

        Assert.Contains("10:30:15", cut.Markup);
        Assert.Empty(cut.FindAll(".clock-panel-time-main"));
    }

    [Fact]
    public void ShowsCountdownToNextPrayer_WhenNoActiveBanner()
    {
        var schedule = Schedule();
        var now = Date.ToDateTime(new TimeOnly(10, 30, 0));
        var status = PrayerStatusCalculator.Compute(null, schedule, null, now);

        var cut = Render<ClockPanel>(p => p.Add(x => x.Status, status).Add(x => x.Now, now));

        Assert.Contains("Menuju Dzuhur", cut.Markup);
        Assert.DoesNotContain("Waktu Sholat", cut.Markup);
    }

    [Fact]
    public void CountdownToNextPrayer_OmitsSeconds()
    {
        var schedule = Schedule();
        // Dhuhr azan at 11:53; 1h 29m 45s before it, seconds must not appear in the countdown value.
        var now = Date.ToDateTime(new TimeOnly(10, 23, 15));
        var status = PrayerStatusCalculator.Compute(null, schedule, null, now);

        var cut = Render<ClockPanel>(p => p.Add(x => x.Status, status).Add(x => x.Now, now));

        var value = cut.Find(".countdown-value").TextContent;
        Assert.Equal("01:30", value); // 1h 29m 45s rounds up to 1h 30m with seconds dropped
    }

    [Fact]
    public void ShowsActiveBanner_InsteadOfCountdown_WhenPrayerWindowActive()
    {
        var schedule = Schedule();
        var dhuhrIqamah = schedule.Get(PrayerName.Dhuhr).IqamahDateTime!.Value;
        var status = PrayerStatusCalculator.Compute(null, schedule, null, dhuhrIqamah.AddMinutes(2));

        var cut = Render<ClockPanel>(p => p.Add(x => x.Status, status).Add(x => x.Now, dhuhrIqamah));

        Assert.Contains("Waktu Sholat Dzuhur", cut.Markup);
        Assert.DoesNotContain("Menuju", cut.Markup);
    }

    [Fact]
    public void ShowsIqamahCountdown_InsteadOfNextPrayerCountdown_WhenWaitingOnIqamah()
    {
        var schedule = Schedule();
        var dhuhrAzan = schedule.Get(PrayerName.Dhuhr).AzanDateTime;
        var now = dhuhrAzan.AddMinutes(1);
        var status = PrayerStatusCalculator.Compute(null, schedule, null, now);

        var cut = Render<ClockPanel>(p => p.Add(x => x.Status, status).Add(x => x.Now, now));

        Assert.Contains("Menuju Iqamah Dzuhur", cut.Markup);
        Assert.DoesNotContain("Menuju Ashar", cut.Markup);
        Assert.DoesNotContain("Waktu Sholat", cut.Markup);
        Assert.Contains("is-shrunk", cut.Markup);
    }

    [Fact]
    public void MainClock_OmitsSeconds_WhileWaitingOnIqamah()
    {
        var schedule = Schedule();
        var dhuhrAzan = schedule.Get(PrayerName.Dhuhr).AzanDateTime;
        var now = dhuhrAzan.AddMinutes(1).AddSeconds(15);
        var status = PrayerStatusCalculator.Compute(null, schedule, null, now);

        var cut = Render<ClockPanel>(p => p
            .Add(x => x.Status, status)
            .Add(x => x.Now, now)
            .Add(x => x.Compact, true));

        Assert.Equal("11:54", cut.Find(".clock-panel-time").TextContent);
    }

    [Fact]
    public void ShrinksClock_WhenActiveBannerShowing()
    {
        var schedule = Schedule();
        var dhuhrIqamah = schedule.Get(PrayerName.Dhuhr).IqamahDateTime!.Value;
        var status = PrayerStatusCalculator.Compute(null, schedule, null, dhuhrIqamah.AddMinutes(2));

        var cut = Render<ClockPanel>(p => p.Add(x => x.Status, status).Add(x => x.Now, dhuhrIqamah));

        Assert.Contains("is-shrunk", cut.Markup);
    }

    [Fact]
    public void AppliesImminentCssClass_WhenCountdownUnderOneMinute()
    {
        var schedule = Schedule();
        var now = schedule.Get(PrayerName.Asr).AzanDateTime.AddSeconds(-30);
        var status = PrayerStatusCalculator.Compute(null, schedule, null, now);

        var cut = Render<ClockPanel>(p => p.Add(x => x.Status, status).Add(x => x.Now, now));

        Assert.Contains("is-imminent", cut.Markup);
    }

    [Fact]
    public void RendersTimesWithColons_RegardlessOfHostCulture()
    {
        // Regression test: "HH:mm:ss" treats ':' as the *culture's* time separator unless the format
        // is applied with InvariantCulture, so on a machine whose locale uses '.' the clock silently
        // rendered "10.30.15" instead of "10:30:15". Finnish is one such locale.
        var finnish = System.Globalization.CultureInfo.GetCultureInfo("fi-FI");
        Assert.NotEqual(":", finnish.DateTimeFormat.TimeSeparator); // sanity: this culture must expose the bug if unfixed

        var original = System.Threading.Thread.CurrentThread.CurrentCulture;
        System.Threading.Thread.CurrentThread.CurrentCulture = finnish;
        try
        {
            var schedule = Schedule();
            var now = Date.ToDateTime(new TimeOnly(10, 30, 15));
            var status = PrayerStatusCalculator.Compute(null, schedule, null, now);

            var cut = Render<ClockPanel>(p => p.Add(x => x.Status, status).Add(x => x.Now, now));

            Assert.Equal("10:30", cut.Find(".clock-panel-time-main").TextContent);
            Assert.Equal(":15", cut.Find(".clock-panel-time-seconds").TextContent);
        }
        finally
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = original;
        }
    }

    [Fact]
    public void UsesCompactCssClass_WhenCompactParameterTrue()
    {
        var schedule = Schedule();
        var now = Date.ToDateTime(new TimeOnly(10, 30, 0));
        var status = PrayerStatusCalculator.Compute(null, schedule, null, now);

        var cut = Render<ClockPanel>(p => p
            .Add(x => x.Status, status)
            .Add(x => x.Now, now)
            .Add(x => x.Compact, true));

        Assert.Contains("is-compact", cut.Markup);
        Assert.DoesNotContain("is-focus", cut.Markup);
    }
}
