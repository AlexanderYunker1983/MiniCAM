namespace MiniCAM.Core.Settings.Models;

/// <summary>
/// Application-level settings (culture, theme).
/// </summary>
public class ApplicationSettings
{
    /// <summary>
    /// Constant for automatic culture detection based on system settings.
    /// </summary>
    public const string CultureAuto = "auto";

    /// <summary>
    /// Constant for English (United States) culture.
    /// </summary>
    public const string CultureEnglish = "en-US";

    /// <summary>
    /// Constant for Russian (Russia) culture.
    /// </summary>
    public const string CultureRussian = "ru-RU";

    /// <summary>
    /// Constant for automatic theme (follow system).
    /// </summary>
    public const string ThemeAuto = "auto";

    /// <summary>
    /// Constant for light theme.
    /// </summary>
    public const string ThemeLight = "light";

    /// <summary>
    /// Constant for dark theme.
    /// </summary>
    public const string ThemeDark = "dark";

    /// <summary>
    /// Gets or sets the selected culture name.
    /// Possible values: "auto", "en-US", "ru-RU".
    /// "auto" means use system regional settings.
    /// </summary>
    public string? Culture { get; set; }

    /// <summary>
    /// Gets or sets the selected theme.
    /// Possible values: "auto", "light", "dark".
    /// "auto" means follow system theme.
    /// </summary>
    public string? Theme { get; set; }
}
