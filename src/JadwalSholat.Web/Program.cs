using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using JadwalSholat.Web;
using JadwalSholat.Web.Services;
using JadwalSholat.Core.Services;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddMudServices();

builder.Services.AddScoped<IPrayerTimeDataSource, HttpPrayerTimeDataSource>();
builder.Services.AddScoped<PrayerTimeRepository>();
builder.Services.AddScoped<ISettingsStore, LocalStorageSettingsStore>();
builder.Services.AddScoped<AppStateService>();
builder.Services.AddScoped<GeolocationService>();

await builder.Build().RunAsync();
