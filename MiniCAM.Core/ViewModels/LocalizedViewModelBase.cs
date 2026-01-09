using System;
using CommunityToolkit.Mvvm.ComponentModel;
using MiniCAM.Core.Localization;

namespace MiniCAM.Core.ViewModels;

/// <summary>
/// Base class for view models that need localization support.
/// Provides automatic subscription to culture changes and abstract method for updating localized strings.
/// </summary>
public abstract class LocalizedViewModelBase : ViewModelBase, IDisposable
{
    /// <summary>
    /// Initializes a new instance of the LocalizedViewModelBase class.
    /// </summary>
    protected LocalizedViewModelBase()
    {
        Resources.CultureChanged += OnCultureChanged;
        UpdateLocalizedStrings();
    }

    /// <summary>
    /// Handles culture change events.
    /// </summary>
    protected virtual void OnCultureChanged(object? sender, CultureChangedEventArgs e)
    {
        UpdateLocalizedStrings();
    }

    /// <summary>
    /// Updates all localized strings in the view model.
    /// Override this method to update localized properties.
    /// </summary>
    protected abstract void UpdateLocalizedStrings();

    /// <summary>
    /// Disposes the view model and unsubscribes from culture change events.
    /// </summary>
    public virtual void Dispose()
    {
        Resources.CultureChanged -= OnCultureChanged;
    }
}
