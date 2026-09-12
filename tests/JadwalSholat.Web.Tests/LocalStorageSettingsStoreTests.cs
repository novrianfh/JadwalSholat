using Bunit;
using JadwalSholat.Core.Models;
using JadwalSholat.Web.Services;

namespace JadwalSholat.Web.Tests;

public class LocalStorageSettingsStoreTests : BunitContext
{
    private const string Key = "jadwalsholat.settings.v1";

    [Fact]
    public async Task LoadAsync_NoStoredValue_ReturnsDefaultSettings()
    {
        JSInterop.Setup<string?>("localStorage.getItem", Key).SetResult(null);
        var store = new LocalStorageSettingsStore(JSInterop.JSRuntime);

        var settings = await store.LoadAsync();

        Assert.Equal("1301", settings.CityId);
        Assert.Equal(10, settings.IqamahMinutes[PrayerName.Fajr]);
    }

    [Fact]
    public async Task LoadAsync_CorruptJson_FallsBackToDefaultInsteadOfThrowing()
    {
        JSInterop.Setup<string?>("localStorage.getItem", Key).SetResult("{ not valid json");
        var store = new LocalStorageSettingsStore(JSInterop.JSRuntime);

        var settings = await store.LoadAsync();

        Assert.Equal(AppSettings.CreateDefault().CityId, settings.CityId);
    }

    [Fact]
    public async Task LoadAsync_ValidStoredJson_RoundTripsValues()
    {
        var original = AppSettings.CreateDefault();
        original.CityId = "1638";
        original.Mosque.Name = "Masjid Ujian";
        original.IqamahMinutes[PrayerName.Maghrib] = 7;
        var json = System.Text.Json.JsonSerializer.Serialize(original);

        JSInterop.Setup<string?>("localStorage.getItem", Key).SetResult(json);
        var store = new LocalStorageSettingsStore(JSInterop.JSRuntime);

        var loaded = await store.LoadAsync();

        Assert.Equal("1638", loaded.CityId);
        Assert.Equal("Masjid Ujian", loaded.Mosque.Name);
        Assert.Equal(7, loaded.IqamahMinutes[PrayerName.Maghrib]);
    }

    [Fact]
    public async Task SaveAsync_WritesSerializedSettingsUnderExpectedKey()
    {
        var handler = JSInterop.SetupVoid("localStorage.setItem", _ => true);
        handler.SetVoidResult();

        var store = new LocalStorageSettingsStore(JSInterop.JSRuntime);
        var settings = AppSettings.CreateDefault();
        settings.Mosque.Name = "Masjid Test Simpan";

        await store.SaveAsync(settings);

        var invocation = Assert.Single(handler.Invocations);
        Assert.Equal(Key, invocation.Arguments[0]);
        Assert.Contains("Masjid Test Simpan", invocation.Arguments[1]?.ToString());
    }
}
