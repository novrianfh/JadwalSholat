using Bunit;
using JadwalSholat.Core.Models;
using JadwalSholat.Core.Services;
using JadwalSholat.Web.Components;

namespace JadwalSholat.Web.Tests;

public class PrayerTimesGridTests : BunitContext
{
    private static PrayerDaySchedule Schedule() =>
        PrayerScheduleBuilder.Build(new DailyPrayerTimes
        {
            Date = new DateOnly(2026, 9, 12),
            Fajr = new TimeOnly(4, 33),
            Shuruk = new TimeOnly(5, 44),
            Dhuhr = new TimeOnly(11, 53),
            Asr = new TimeOnly(15, 7),
            Maghrib = new TimeOnly(17, 54),
            Isha = new TimeOnly(19, 2),
        }, AppSettings.CreateDefault());

    public PrayerTimesGridTests()
    {
        // The component calls a scroll-into-view JS helper after render; make bUnit's fake JS runtime accept it.
        JSInterop.SetupVoid("jadwalSholatInterop.scrollCurrentPrayerIntoView").SetVoidResult();
    }

    [Fact]
    public void RendersAllSixPrayersInChronologicalOrder()
    {
        var cut = Render<PrayerTimesGrid>(p => p
            .Add(x => x.Schedule, Schedule())
            .Add(x => x.HighlightedPrayerName, PrayerName.Dhuhr));

        var names = cut.FindAll(".prayer-grid-name").Select(e => e.TextContent).ToList();
        Assert.Equal(["Subuh", "Terbit", "Dzuhur", "Ashar", "Maghrib", "Isya"], names);
    }

    [Fact]
    public void HighlightsOnlyTheGivenUpcomingPrayerCell()
    {
        // e.g. now 11:03, Dzuhur 11:38 -> Dzuhur (the upcoming prayer) is highlighted, not the one just passed.
        var cut = Render<PrayerTimesGrid>(p => p
            .Add(x => x.Schedule, Schedule())
            .Add(x => x.HighlightedPrayerName, PrayerName.Asr));

        var highlighted = cut.FindAll(".prayer-grid-cell.is-next");
        Assert.Single(highlighted);
        Assert.Contains("Ashar", highlighted[0].TextContent);
    }

    [Fact]
    public void NullHighlightedPrayerName_HighlightsNothing()
    {
        var cut = Render<PrayerTimesGrid>(p => p
            .Add(x => x.Schedule, Schedule())
            .Add(x => x.HighlightedPrayerName, (PrayerName?)null));

        Assert.Empty(cut.FindAll(".prayer-grid-cell.is-next"));
    }

    [Fact]
    public void RendersTimesWithColons_RegardlessOfHostCulture()
    {
        // Same class of bug as ClockPanelTests.RendersTimesWithColons_RegardlessOfHostCulture:
        // "HH:mm" must not pick up the current thread culture's time separator.
        var finnish = System.Globalization.CultureInfo.GetCultureInfo("fi-FI");
        var original = System.Threading.Thread.CurrentThread.CurrentCulture;
        System.Threading.Thread.CurrentThread.CurrentCulture = finnish;
        try
        {
            var cut = Render<PrayerTimesGrid>(p => p
                .Add(x => x.Schedule, Schedule())
                .Add(x => x.HighlightedPrayerName, PrayerName.Dhuhr));

            Assert.Contains("11:53", cut.Markup);
            Assert.Contains("12:03", cut.Markup); // Dhuhr iqamah (azan + default 10 min)
        }
        finally
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = original;
        }
    }

    [Fact]
    public void ShowsIqamahTime_OnlyForObligatoryPrayers()
    {
        var cut = Render<PrayerTimesGrid>(p => p
            .Add(x => x.Schedule, Schedule())
            .Add(x => x.HighlightedPrayerName, PrayerName.Fajr));

        var cells = cut.FindAll(".prayer-grid-cell");
        var shurukCell = cells[1]; // Fajr, Shuruk, Dhuhr, Asr, Maghrib, Isha
        Assert.Contains("Terbit", shurukCell.TextContent);
        Assert.DoesNotContain("Iqamah", shurukCell.TextContent);

        var fajrCell = cells[0];
        Assert.Contains("Iqamah", fajrCell.TextContent);
    }
}
