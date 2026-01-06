namespace MiniCAM.Core.Settings;

/// <summary>
/// Application settings model.
/// </summary>
public class AppSettings
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

    // Code Generation Settings
    public bool? UseLineNumbers { get; set; }
    public string? StartLineNumber { get; set; }
    public string? LineNumberStep { get; set; }
    public bool? GenerateComments { get; set; }
    public bool? AllowArcs { get; set; }
    public bool? FormatCommands { get; set; }
    public bool? SetWorkCoordinateSystem { get; set; }
    public string? CoordinateSystem { get; set; }
    public bool? SetAbsoluteCoordinates { get; set; }
    public bool? AllowRelativeCoordinates { get; set; }
    public bool? SetZerosAtStart { get; set; }
    public string? X0 { get; set; }
    public string? Y0 { get; set; }
    public string? Z0 { get; set; }
    public bool? MoveToPointAtEnd { get; set; }
    public string? X { get; set; }
    public string? Y { get; set; }
    public string? Z { get; set; }

    // Spindle Settings
    public bool? AddSpindleCode { get; set; }
    public bool? SetSpindleSpeed { get; set; }
    public string? SpindleSpeed { get; set; }
    public bool? EnableSpindleBeforeOperations { get; set; }
    public string? SpindleEnableCommand { get; set; }
    public bool? AddSpindleDelayAfterEnable { get; set; }
    public string? SpindleDelayParameter { get; set; }
    public string? SpindleDelayValue { get; set; }
    public bool? DisableSpindleAfterOperations { get; set; }

    // Coolant Settings
    public bool? AddCoolantCode { get; set; }
    public bool? EnableCoolantAtStart { get; set; }
    public bool? DisableCoolantAtEnd { get; set; }
}

