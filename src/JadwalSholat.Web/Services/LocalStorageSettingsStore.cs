using System.Text.Json;
using JadwalSholat.Core.Models;
using JadwalSholat.Core.Services;
using Microsoft.JSInterop;

namespace JadwalSholat.Web.Services;

/// <summary>Persists <see cref="AppSettings"/> to the browser's LocalStorage (Requirement.md: "penyimpanan
/// menggunakan LocalStorage"). Plain JS interop rather than a third-party package: two calls don't
/// warrant a dependency (CLAUDE.md "Tech choice — vanilla by default").</summary>
public sealed class LocalStorageSettingsStore(IJSRuntime js) : ISettingsStore
{
    private const string StorageKey = "jadwalsholat.settings.v1";

    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = false };

    public async Task<AppSettings> LoadAsync()
    {
        string? json;
        try
        {
            json = await js.InvokeAsync<string?>("localStorage.getItem", StorageKey);
        }
        catch (JSException)
        {
            return AppSettings.CreateDefault();
        }

        if (string.IsNullOrWhiteSpace(json)) return AppSettings.CreateDefault();

        try
        {
            var settings = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions);
            return settings ?? AppSettings.CreateDefault();
        }
        catch (JsonException)
        {
            // Corrupt or from an incompatible earlier schema version: fall back rather than crash the dashboard.
            return AppSettings.CreateDefault();
        }
    }

    public async Task SaveAsync(AppSettings settings)
    {
        var json = JsonSerializer.Serialize(settings, JsonOptions);
        await js.InvokeVoidAsync("localStorage.setItem", StorageKey, json);
    }
}
