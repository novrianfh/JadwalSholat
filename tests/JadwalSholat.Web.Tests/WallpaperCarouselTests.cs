using Bunit;
using JadwalSholat.Core.Models;
using JadwalSholat.Web.Components;

namespace JadwalSholat.Web.Tests;

public class WallpaperCarouselTests : BunitContext
{
    private static List<WallpaperItem> Images() =>
    [
        new() { Id = "1", Url = "a.svg" },
        new() { Id = "2", Url = "b.svg" },
        new() { Id = "3", Url = "c.svg" },
    ];

    [Fact]
    public void RendersOneLayerPerImage()
    {
        var cut = Render<WallpaperCarousel>(p => p.Add(x => x.Images, Images()));
        Assert.Equal(3, cut.FindAll(".wallpaper-layer").Count);
    }

    [Fact]
    public void FirstLayerIsActiveInitially()
    {
        var cut = Render<WallpaperCarousel>(p => p.Add(x => x.Images, Images()));
        var layers = cut.FindAll(".wallpaper-layer");

        Assert.Contains("is-active", layers[0].ClassName);
        Assert.DoesNotContain("is-active", layers[1].ClassName);
        Assert.DoesNotContain("is-active", layers[2].ClassName);
    }

    [Fact]
    public void EnabledFalse_AppliesHiddenClass()
    {
        var cut = Render<WallpaperCarousel>(p => p
            .Add(x => x.Images, Images())
            .Add(x => x.Enabled, false));

        Assert.Contains("is-hidden", cut.Find(".wallpaper-carousel").ClassName);
    }

    [Fact]
    public void NoImages_RendersNoLayersWithoutThrowing()
    {
        var cut = Render<WallpaperCarousel>(p => p.Add(x => x.Images, new List<WallpaperItem>()));
        Assert.Empty(cut.FindAll(".wallpaper-layer"));
    }
}
