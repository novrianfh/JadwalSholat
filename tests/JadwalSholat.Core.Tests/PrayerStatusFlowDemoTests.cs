using System.Globalization;
using JadwalSholat.Core.Models;
using JadwalSholat.Core.Services;
using Xunit.Abstractions;

namespace JadwalSholat.Core.Tests;

/// <summary>Demonstrates the full clock-panel state machine end-to-end using a compressed schedule —
/// Iqamah 1 minute after Azan, "waktu sholat" window 1 minute long — so every transition
/// (idle countdown -> imminent -> Iqamah countdown -> active banner -> idle again) happens within a
/// 3-minute span instead of the real 10+15 minutes. Run with `dotnet test --logger "console;verbosity=detailed"`
/// to see the printed transition log.</summary>
public class PrayerStatusFlowDemoTests
{
    private readonly ITestOutputHelper _output;

    public PrayerStatusFlowDemoTests(ITestOutputHelper output) => _output = output;

    [Fact]
    public void FullFlow_WithOneMinuteIqamahAndOneMinuteWindow()
    {
        // Anchor: Dhuhr's Azan fires exactly 1 minute after this reference point, so every step below
        // is expressed as an offset from a fixed, readable "testNow".
        var testNow = new DateTime(2026, 9, 12, 10, 0, 0);
        var azan = testNow.AddMinutes(1);

        var settings = AppSettings.CreateDefault();
        settings.IqamahMinutes[PrayerName.Dhuhr] = 1; // normally 10
        settings.PrayerDurationMinutes[PrayerName.Dhuhr] = 1; // normally 15

        var raw = TestData.RawDay(DateOnly.FromDateTime(testNow)) with { Dhuhr = TimeOnly.FromDateTime(azan) };
        var today = PrayerScheduleBuilder.Build(raw, settings);

        var dhuhr = today.Get(PrayerName.Dhuhr);
        var iqamah = dhuhr.IqamahDateTime!.Value; // azan + 1 min
        var windowEnd = dhuhr.WindowEndDateTime!.Value; // iqamah + 1 min

        _output.WriteLine($"Azan      = {Fmt(azan)}");
        _output.WriteLine($"Iqamah    = {Fmt(iqamah)}");
        _output.WriteLine($"WindowEnd = {Fmt(windowEnd)}");
        _output.WriteLine("");

        // 1) Well before Azan: ordinary "Menuju Dzuhur" countdown, nothing special.
        Step(today, azan.AddSeconds(-90), s =>
        {
            Assert.False(s.IsImminent);
            Assert.False(s.HasIqamahCountdown);
            Assert.False(s.HasActiveBanner);
            Assert.Equal(PrayerName.Dhuhr, s.NextPrayer.Name);
        });

        // 2) Inside the last 60s before Azan: countdown turns "imminent" (pulsing red in the UI).
        Step(today, azan.AddSeconds(-30), s =>
        {
            Assert.True(s.IsImminent);
            Assert.False(s.HasIqamahCountdown);
        });

        // 3) Exactly at Azan: countdown display switches to "Menuju Iqamah Dzuhur".
        Step(today, azan, s =>
        {
            Assert.True(s.HasIqamahCountdown);
            Assert.Equal(PrayerName.Dhuhr, s.IqamahWaitEntry!.Name);
            Assert.False(s.HasActiveBanner);
        });

        // 4) Midway through the Azan->Iqamah wait: still counting down to Iqamah.
        Step(today, azan.AddSeconds(30), s =>
        {
            Assert.True(s.HasIqamahCountdown);
            Assert.True(s.CountdownToIqamah <= TimeSpan.FromSeconds(30));
        });

        // 5) Exactly at Iqamah: display switches to "Waktu Sholat Dzuhur" (the active banner).
        Step(today, iqamah, s =>
        {
            Assert.False(s.HasIqamahCountdown);
            Assert.True(s.HasActiveBanner);
            Assert.Equal(PrayerName.Dhuhr, s.ActiveBannerPrayerName);
        });

        // 6) Midway through the "waktu sholat" window: banner still showing.
        Step(today, iqamah.AddSeconds(30), s => Assert.True(s.HasActiveBanner));

        // 7) Exactly at WindowEnd: banner clears, display returns to "Menuju {next prayer}" (Asr).
        Step(today, windowEnd, s =>
        {
            Assert.False(s.HasActiveBanner);
            Assert.False(s.HasIqamahCountdown);
            Assert.Equal(PrayerName.Asr, s.NextPrayer.Name);
        });
    }

    private void Step(PrayerDaySchedule today, DateTime now, Action<PrayerStatus> assertions)
    {
        var status = PrayerStatusCalculator.Compute(null, today, null, now);

        var state = status.HasActiveBanner ? $"WAKTU SHOLAT {status.ActiveBannerPrayerName!.Value.ToIndonesian()}"
            : status.HasIqamahCountdown ? $"MENUJU IQAMAH {status.IqamahWaitEntry!.Name.ToIndonesian()} (sisa {status.CountdownToIqamah.ToString(@"mm\:ss", CultureInfo.InvariantCulture)})"
            : status.IsImminent ? $"MENUJU {status.NextPrayer.Name.ToIndonesian()} — IMMINENT (sisa {status.CountdownToNext.TotalSeconds:0}s)"
            : $"MENUJU {status.NextPrayer.Name.ToIndonesian()} (sisa {status.CountdownToNext.ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture)})";

        _output.WriteLine($"[{Fmt(now)}] {state}");

        assertions(status);
    }

    private static string Fmt(DateTime value) => value.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
}
