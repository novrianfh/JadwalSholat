using Bunit;
using JadwalSholat.Web.Services;

namespace JadwalSholat.Web.Tests;

public class GeolocationServiceTests : BunitContext
{
    [Fact]
    public async Task TryGetCurrentPositionAsync_Success_ReturnsCoordinates()
    {
        JSInterop.Setup<GeoPosition>("jadwalSholatInterop.getCurrentPosition")
            .SetResult(new GeoPosition(-6.9175, 107.6191));
        var service = new GeolocationService(JSInterop.JSRuntime);

        var result = await service.TryGetCurrentPositionAsync();

        Assert.NotNull(result);
        Assert.Equal(-6.9175, result!.Latitude);
        Assert.Equal(107.6191, result.Longitude);
    }

    [Fact]
    public async Task TryGetCurrentPositionAsync_JsThrows_ReturnsNullInsteadOfPropagating()
    {
        JSInterop.Setup<GeoPosition>("jadwalSholatInterop.getCurrentPosition")
            .SetException(new Microsoft.JSInterop.JSException("Permission denied"));
        var service = new GeolocationService(JSInterop.JSRuntime);

        var result = await service.TryGetCurrentPositionAsync();

        Assert.Null(result);
    }
}
