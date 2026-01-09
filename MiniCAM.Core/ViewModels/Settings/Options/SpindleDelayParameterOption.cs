namespace MiniCAM.Core.ViewModels;

/// <summary>
/// Represents a spindle delay parameter option (F, P, Pxx.).
/// </summary>
public partial class SpindleDelayParameterOption : OptionBase
{
    /// <summary>
    /// Initializes a new instance of the SpindleDelayParameterOption class.
    /// </summary>
    /// <param name="key">The parameter key (e.g., "F", "P", "Pxx.").</param>
    /// <param name="displayName">The localized display name.</param>
    public SpindleDelayParameterOption(string key, string displayName)
        : base(key, displayName)
    {
    }
}
