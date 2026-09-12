using JadwalSholat.Core.Models;

namespace JadwalSholat.Core.Services;

/// <summary>Persistence boundary for <see cref="AppSettings"/> (Requirement.md: "penyimpanan menggunakan LocalStorage").
/// Kept out of Core's dependencies deliberately: the concrete implementation lives in the Web project and
/// talks to the browser's LocalStorage via JS interop, so Core stays a plain, browser-free class library.</summary>
public interface ISettingsStore
{
    Task<AppSettings> LoadAsync();
    Task SaveAsync(AppSettings settings);
}
