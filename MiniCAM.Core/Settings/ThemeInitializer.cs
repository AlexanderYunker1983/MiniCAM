using Avalonia;
using Avalonia.Styling;

namespace MiniCAM.Core.Settings;

/// <summary>
/// Initializes and switches application theme based on settings.
/// </summary>
public static class ThemeInitializer
{
    /// <summary>
    /// Applies theme from current settings (or defaults).
    /// </summary>
    public static void Initialize()
    {
        var settings = SettingsManager.Current;
        var theme = string.IsNullOrWhiteSpace(settings.Theme)
            ? AppSettings.ThemeAuto
            : settings.Theme;

        ApplyTheme(theme);
    }

    /// <summary>
    /// Applies specified theme and saves it to settings.
    /// </summary>
    /// <param name="theme">"auto", "light", "dark".</param>
    public static void ApplyTheme(string theme)
    {
        if (Application.Current is { } app)
        {
            app.RequestedThemeVariant = theme switch
            {
                AppSettings.ThemeLight => ThemeVariant.Light,
                AppSettings.ThemeDark => ThemeVariant.Dark,
                _ => ThemeVariant.Default // "auto" – follow system
            };
        }

        var settings = SettingsManager.Current;
        settings.Theme = theme;
        SettingsManager.SaveCurrent();
    }
}


