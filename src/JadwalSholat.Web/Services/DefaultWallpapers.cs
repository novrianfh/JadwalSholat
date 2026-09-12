using JadwalSholat.Core.Models;

namespace JadwalSholat.Web.Services;

/// <summary>The bundled gradient wallpapers shipped under wwwroot/images/wallpapers, seeded into a fresh
/// install's settings so the carousel has something to show before an admin adds their own (Requirement.md #34).</summary>
public static class DefaultWallpapers
{
    public static List<WallpaperItem> CreateSeedSet() =>
    [
        new() { Id = "default-1", Url = "images/wallpapers/wallpaper-1.svg", Label = "Zamrud" },
        new() { Id = "default-2", Url = "images/wallpapers/wallpaper-2.svg", Label = "Senja" },
        new() { Id = "default-3", Url = "images/wallpapers/wallpaper-3.svg", Label = "Malam" },
        new() { Id = "default-4", Url = "images/wallpapers/wallpaper-4.svg", Label = "Fajar" },
    ];
}
