using Microsoft.JSInterop;

namespace JadwalSholat.Web.Services;

public sealed record GeoPosition(double Latitude, double Longitude);

/// <summary>Thin wrapper over the browser's navigator.geolocation (Requirement.md #19: "Lokasi bisa dideteksi
/// otomatis"). Returns null on any failure (permission denied, unsupported, timeout) so callers can fall back
/// to manual city selection without special-casing exceptions.</summary>
public sealed class GeolocationService(IJSRuntime js)
{
    public async Task<GeoPosition?> TryGetCurrentPositionAsync()
    {
        try
        {
            return await js.InvokeAsync<GeoPosition>("jadwalSholatInterop.getCurrentPosition");
        }
        catch (JSException)
        {
            return null;
        }
    }
}
