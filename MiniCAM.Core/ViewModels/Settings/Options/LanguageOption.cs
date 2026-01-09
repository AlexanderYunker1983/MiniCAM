namespace MiniCAM.Core.ViewModels;

/// <summary>
/// Represents a language/culture option for application localization.
/// </summary>
public partial class LanguageOption : OptionBase
{
    /// <summary>
    /// Gets the culture name (alias for Key for backward compatibility).
    /// </summary>
    public string CultureName => Key;

    /// <summary>
    /// Initializes a new instance of the LanguageOption class.
    /// </summary>
    /// <param name="cultureName">The culture name (e.g., "en-US", "ru-RU").</param>
    /// <param name="displayName">The localized display name.</param>
    public LanguageOption(string cultureName, string displayName)
        : base(cultureName, displayName)
    {
    }
}
