namespace MiniCAM.Core.ViewModels;

/// <summary>
/// Represents a work coordinate system option (G54-G59).
/// </summary>
public partial class CoordinateSystemOption : OptionBase
{
    /// <summary>
    /// Initializes a new instance of the CoordinateSystemOption class.
    /// </summary>
    /// <param name="key">The coordinate system key (e.g., "G54", "G55").</param>
    /// <param name="displayName">The localized display name.</param>
    public CoordinateSystemOption(string key, string displayName)
        : base(key, displayName)
    {
    }
}
