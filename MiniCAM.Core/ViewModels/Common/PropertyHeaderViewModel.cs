using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MiniCAM.Core.ViewModels;

/// <summary>
/// View model for a property header with text and font style.
/// Used to display property labels with change indicators.
/// </summary>
public partial class PropertyHeaderViewModel : ObservableObject
{
    [ObservableProperty]
    private string _text = string.Empty;

    [ObservableProperty]
    private FontStyle _fontStyle = FontStyle.Normal;

    /// <summary>
    /// Initializes a new instance of the PropertyHeaderViewModel class.
    /// </summary>
    /// <param name="text">The header text.</param>
    /// <param name="fontStyle">The font style.</param>
    public PropertyHeaderViewModel(string text = "", FontStyle fontStyle = FontStyle.Normal)
    {
        _text = text;
        _fontStyle = fontStyle;
    }

    /// <summary>
    /// Updates the header to indicate a modified state.
    /// </summary>
    /// <param name="baseText">The base text without modification indicator.</param>
    /// <param name="isModified">Whether the property is modified.</param>
    public void UpdateState(string baseText, bool isModified)
    {
        Text = isModified ? $"{baseText} *" : baseText;
        FontStyle = isModified ? FontStyle.Italic : FontStyle.Normal;
    }
}
