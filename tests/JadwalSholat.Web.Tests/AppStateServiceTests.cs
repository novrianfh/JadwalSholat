using JadwalSholat.Core.Models;
using JadwalSholat.Core.Services;
using JadwalSholat.Web.Services;
using Moq;

namespace JadwalSholat.Web.Tests;

public class AppStateServiceTests
{
    [Fact]
    public async Task EnsureLoadedAsync_LoadsFromStoreOnce()
    {
        var store = new Mock<ISettingsStore>();
        store.Setup(s => s.LoadAsync()).ReturnsAsync(AppSettings.CreateDefault());
        var state = new AppStateService(store.Object);

        await state.EnsureLoadedAsync();
        await state.EnsureLoadedAsync();

        store.Verify(s => s.LoadAsync(), Times.Once);
    }

    [Fact]
    public async Task EnsureLoadedAsync_EmptyWallpaperList_SeedsDefaultsAndPersists()
    {
        var settings = AppSettings.CreateDefault();
        Assert.Empty(settings.Wallpaper.Items); // sanity check on the Core default

        var store = new Mock<ISettingsStore>();
        store.Setup(s => s.LoadAsync()).ReturnsAsync(settings);
        var state = new AppStateService(store.Object);

        await state.EnsureLoadedAsync();

        Assert.NotEmpty(state.Settings.Wallpaper.Items);
        store.Verify(s => s.SaveAsync(It.Is<AppSettings>(x => x.Wallpaper.Items.Count > 0)), Times.Once);
    }

    [Fact]
    public async Task EnsureLoadedAsync_ExistingWallpaperList_DoesNotOverwriteOrPersist()
    {
        var settings = AppSettings.CreateDefault();
        settings.Wallpaper.Items.Add(new WallpaperItem { Url = "custom.jpg" });
        var store = new Mock<ISettingsStore>();
        store.Setup(s => s.LoadAsync()).ReturnsAsync(settings);
        var state = new AppStateService(store.Object);

        await state.EnsureLoadedAsync();

        Assert.Single(state.Settings.Wallpaper.Items);
        Assert.Equal("custom.jpg", state.Settings.Wallpaper.Items[0].Url);
        store.Verify(s => s.SaveAsync(It.IsAny<AppSettings>()), Times.Never);
    }

    [Fact]
    public async Task SaveAsync_PersistsAndRaisesOnChange()
    {
        var store = new Mock<ISettingsStore>();
        store.Setup(s => s.LoadAsync()).ReturnsAsync(AppSettings.CreateDefault());
        var state = new AppStateService(store.Object);
        await state.EnsureLoadedAsync();

        var raised = false;
        state.OnChange += () => raised = true;

        var updated = AppSettings.CreateDefault();
        updated.Mosque.Name = "Masjid Baru";
        await state.SaveAsync(updated);

        Assert.True(raised);
        Assert.Equal("Masjid Baru", state.Settings.Mosque.Name);
        store.Verify(s => s.SaveAsync(updated), Times.Once);
    }
}
