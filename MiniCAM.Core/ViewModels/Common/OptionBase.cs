using CommunityToolkit.Mvvm.ComponentModel;

namespace MiniCAM.Core.ViewModels;

/// <summary>
/// Base class for option items with a key and display name.
/// </summary>
public abstract partial class OptionBase : ObservableObject
{
    /// <summary>
    /// Gets the key/identifier for this option.
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// Gets or sets the display name for this option.
    /// </summary>
    [ObservableProperty]
    private string _displayName;

    /// <summary>
    /// Initializes a new instance of the OptionBase class.
    /// </summary>
    /// <param name="key">The key/identifier for this option.</param>
    /// <param name="displayName">The display name for this option.</param>
    protected OptionBase(string key, string displayName)
    {
        Key = key;
        _displayName = displayName;
    }
}
