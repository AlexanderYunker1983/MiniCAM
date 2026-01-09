using System;
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
    /// <param name="settingsService">Settings service instance.</param>
    public static void Initialize(ISettingsService settingsService)
    {
        var settings = settingsService.Current;
        var theme = string.IsNullOrWhiteSpace(settings.Theme)
            ? AppSettings.ThemeAuto
            : settings.Theme;

        ApplyTheme(theme, settingsService);
    }

    /// <summary>
    /// Applies specified theme and saves it to settings.
    /// </summary>
    /// <param name="theme">"auto", "light", "dark".</param>
    /// <param name="settingsService">Settings service instance.</param>
    public static void ApplyTheme(string theme, ISettingsService settingsService)
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

        try
        {
            var settings = settingsService.Current;
            settings.Theme = theme;
            settingsService.SaveCurrent();
        }
        catch (Exception ex)
        {
            // Log error but don't prevent theme change
            // Theme is already applied to UI, settings will be saved on next successful save operation
            System.Diagnostics.Debug.WriteLine($"Failed to save theme setting: {ex.Message}");
        }
    }
}


