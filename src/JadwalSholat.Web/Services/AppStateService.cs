using JadwalSholat.Core.Models;
using JadwalSholat.Core.Services;

namespace JadwalSholat.Web.Services;

/// <summary>Single in-memory copy of <see cref="AppSettings"/> for the app's lifetime, backed by
/// <see cref="ISettingsStore"/>. The Dashboard and Settings pages both depend on this instead of the store
/// directly, so a save on the Settings page is reflected on the Dashboard immediately via <see cref="OnChange"/>,
/// without a page reload.</summary>
public sealed class AppStateService(ISettingsStore store)
{
    private bool _loaded;

    public AppSettings Settings { get; private set; } = AppSettings.CreateDefault();

    public event Action? OnChange;

    public async Task EnsureLoadedAsync()
    {
        if (_loaded) return;
        Settings = await store.LoadAsync();

        if (Settings.Wallpaper.Items.Count == 0)
        {
            Settings.Wallpaper.Items = DefaultWallpapers.CreateSeedSet();
            await store.SaveAsync(Settings);
        }

        _loaded = true;
    }

    public async Task SaveAsync(AppSettings updated)
    {
        Settings = updated;
        await store.SaveAsync(Settings);
        OnChange?.Invoke();
    }
}
