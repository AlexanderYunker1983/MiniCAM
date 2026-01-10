namespace MiniCAM.Core.ViewModels.Settings.Options;

/// <summary>
/// Represents a theme option for application appearance.
/// </summary>
public partial class ThemeOption : Common.OptionBase
{
    /// <summary>
    /// Initializes a new instance of the ThemeOption class.
    /// </summary>
    /// <param name="key">The theme key (e.g., "Auto", "Light", "Dark").</param>
    /// <param name="displayName">The localized display name.</param>
    public ThemeOption(string key, string displayName)
        : base(key, displayName)
    {
    }
}
