using MudBlazor;

namespace JadwalSholat.Web;

/// <summary>Single shared MudBlazor theme: a calm dark palette designed to be legible on a mosque
/// display screen from a distance (high contrast, generous type scale).</summary>
public static class AppTheme
{
    public static readonly MudTheme Theme = new()
    {
        PaletteDark = new PaletteDark
        {
            Primary = "#2FBF83",
            Secondary = "#E8B23A",
            Background = "#0B1220",
            Surface = "#121B2E",
            AppbarBackground = "#0B1220",
            DrawerBackground = "#0B1220",
            TextPrimary = "#F4F7FB",
            TextSecondary = "#B7C3D6",
        },
        PaletteLight = new PaletteLight
        {
            Primary = "#1E9E6B",
            Secondary = "#B9821E",
            Background = "#F4F7FB",
            Surface = "#FFFFFF",
        },
        Typography = new Typography
        {
            Default = new DefaultTypography { FontFamily = ["Segoe UI", "Roboto", "Helvetica", "Arial", "sans-serif"] }
        }
    };
}
