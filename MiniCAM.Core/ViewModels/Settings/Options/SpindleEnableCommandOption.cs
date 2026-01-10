namespace MiniCAM.Core.ViewModels.Settings.Options;

/// <summary>
/// Represents a spindle enable command option (M3, M4).
/// </summary>
public partial class SpindleEnableCommandOption : Common.OptionBase
{
    /// <summary>
    /// Initializes a new instance of the SpindleEnableCommandOption class.
    /// </summary>
    /// <param name="key">The command key (e.g., "M3", "M4").</param>
    /// <param name="displayName">The localized display name.</param>
    public SpindleEnableCommandOption(string key, string displayName)
        : base(key, displayName)
    {
    }
}
