using System;
using System.Globalization;
using System.Resources;

namespace MiniCAM.Core.Localization;

/// <summary>
/// Provides access to localized string resources.
/// </summary>
public static class Resources
{
    private static readonly ResourceManager ResourceManager = new("MiniCAM.Core.Localization.Resources", typeof(Resources).Assembly);

    /// <summary>
    /// Event that is raised when the culture changes.
    /// </summary>
    public static event EventHandler<CultureChangedEventArgs>? CultureChanged;

    /// <summary>
    /// Gets the current culture for localization.
    /// </summary>
    public static CultureInfo? CurrentCulture
    {
        get => CultureInfo.CurrentUICulture;
        set
        {
            var oldCulture = CultureInfo.CurrentUICulture;
            CultureInfo.CurrentUICulture = value ?? CultureInfo.InvariantCulture;
            
            // Raise event if culture actually changed
            if (oldCulture.Name != CultureInfo.CurrentUICulture.Name)
            {
                CultureChanged?.Invoke(null, new CultureChangedEventArgs(oldCulture, CultureInfo.CurrentUICulture));
            }
        }
    }

    /// <summary>
    /// Gets the localized string for the specified resource name.
    /// </summary>
    /// <param name="name">The name of the resource to retrieve.</param>
    /// <returns>The localized string, or null if not found.</returns>
    public static string? GetString(string name)
    {
        return ResourceManager.GetString(name, CurrentCulture);
    }

    /// <summary>
    /// Gets the localized string for the specified resource name.
    /// </summary>
    /// <param name="name">The name of the resource to retrieve.</param>
    /// <param name="defaultValue">The default value to return if the resource is not found.</param>
    /// <returns>The localized string, or the default value if not found.</returns>
    public static string GetString(string name, string defaultValue)
    {
        return ResourceManager.GetString(name, CurrentCulture) ?? defaultValue;
    }

    /// <summary>
    /// Sets the application culture to the specified language.
    /// </summary>
    /// <param name="cultureName">Culture name (e.g., "en-US", "ru-RU").</param>
    public static void SetCulture(string cultureName)
    {
        try
        {
            CurrentCulture = new CultureInfo(cultureName);
        }
        catch (CultureNotFoundException)
        {
            // Fallback to default culture if specified culture is not found
            CurrentCulture = CultureInfo.InvariantCulture;
        }
    }

    // Example properties for common strings
    public static string AppName => GetString(nameof(AppName), "MiniCAM");
    public static string Welcome => GetString(nameof(Welcome), "Welcome");
    public static string WindowTitle => GetString(nameof(WindowTitle), "MiniCAM");
    
    // Menu strings
    public static string MenuSettings => GetString(nameof(MenuSettings), "Settings");
    public static string MenuApplicationSettings => GetString(nameof(MenuApplicationSettings), "Application Settings");

    // Ribbon tab headers
    public static string RibbonTabDrilling => GetString(nameof(RibbonTabDrilling), "Drilling");
    public static string RibbonTabPocket => GetString(nameof(RibbonTabPocket), "Pocket");
    public static string RibbonTabProfile => GetString(nameof(RibbonTabProfile), "Profile");
    public static string RibbonTabOther => GetString(nameof(RibbonTabOther), "Other");

    // Application settings window strings
    public static string ApplicationSettingsTitle => GetString(nameof(ApplicationSettingsTitle), "Application settings");
    public static string LanguageLabel => GetString(nameof(LanguageLabel), "Application language");
    public static string ButtonApply => GetString(nameof(ButtonApply), "Apply");
    public static string ButtonReset => GetString(nameof(ButtonReset), "Reset changes");
    public static string LanguageAuto => GetString(nameof(LanguageAuto), "Auto (system)");
    public static string LanguageEnglish => GetString(nameof(LanguageEnglish), "English");
    public static string LanguageRussian => GetString(nameof(LanguageRussian), "Russian");

    public static string ThemeLabel => GetString(nameof(ThemeLabel), "Application theme");
    public static string ThemeAuto => GetString(nameof(ThemeAuto), "System");
    public static string ThemeLight => GetString(nameof(ThemeLight), "Light");
    public static string ThemeDark => GetString(nameof(ThemeDark), "Dark");
}

/// <summary>
/// Event arguments for culture change events.
/// </summary>
public class CultureChangedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the previous culture.
    /// </summary>
    public CultureInfo OldCulture { get; }

    /// <summary>
    /// Gets the new culture.
    /// </summary>
    public CultureInfo NewCulture { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CultureChangedEventArgs"/> class.
    /// </summary>
    /// <param name="oldCulture">The previous culture.</param>
    /// <param name="newCulture">The new culture.</param>
    public CultureChangedEventArgs(CultureInfo oldCulture, CultureInfo newCulture)
    {
        OldCulture = oldCulture;
        NewCulture = newCulture;
    }
}

