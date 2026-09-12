namespace JadwalSholat.Core.Models;

public sealed class WallpaperItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>Relative asset path, absolute URL, or a data: URL for an uploaded image.</summary>
    public string Url { get; set; } = "";

    public string? Label { get; set; }
}

public sealed class WallpaperSettings
{
    public bool Enabled { get; set; } = true;
    public int IntervalSeconds { get; set; } = 10;
    public List<WallpaperItem> Items { get; set; } = [];
}
