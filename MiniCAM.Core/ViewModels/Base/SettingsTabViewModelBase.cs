using System;
using System.Linq.Expressions;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using MiniCAM.Core.Localization;
using MiniCAM.Core.Settings;

namespace MiniCAM.Core.ViewModels;

/// <summary>
/// Base class for settings tab view models with change tracking support.
/// </summary>
public abstract class SettingsTabViewModelBase : ViewModelBase, IDisposable
{
    /// <summary>
    /// Gets the header property tracker for managing property change indicators.
    /// </summary>
    protected HeaderPropertyTracker HeaderTracker { get; }

    protected SettingsTabViewModelBase()
    {
        HeaderTracker = new HeaderPropertyTracker();
        Resources.CultureChanged += OnCultureChanged;
    }

    protected virtual void OnCultureChanged(object? sender, CultureChangedEventArgs e)
    {
        UpdateLocalizedStrings();
        HeaderTracker.UpdateAllHeadersImmediate();
    }

    protected abstract void UpdateLocalizedStrings();

    /// <summary>
    /// Registers a property for change tracking with header indicator support.
    /// </summary>
    protected void RegisterProperty<T>(
        string propertyName,
        T originalValue,
        Func<string> getResourceString,
        Action<string> setHeaderText,
        Action<FontStyle> setFontStyle)
    {
        HeaderTracker.Register(propertyName, originalValue, getResourceString, setHeaderText, setFontStyle);
    }

    /// <summary>
    /// Updates the current value for a tracked property and updates its header indicator.
    /// </summary>
    protected void UpdatePropertyValue<T>(string propertyName, T currentValue)
    {
        HeaderTracker.Update(propertyName, currentValue);
    }

    /// <summary>
    /// Updates header text and font style based on whether the value is modified.
    /// This method is kept for backward compatibility but consider using RegisterProperty/UpdatePropertyValue instead.
    /// </summary>
    protected void UpdateHeaderIndicator<T>(
        T currentValue,
        T originalValue,
        Func<string> getResourceString,
        Action<string> setHeaderText,
        Action<FontStyle> setFontStyle)
    {
        var isModified = !Equals(currentValue, originalValue);
        setHeaderText(isModified ? $"{getResourceString()} *" : getResourceString());
        setFontStyle(isModified ? FontStyle.Italic : FontStyle.Normal);
    }

    /// <summary>
    /// Loads settings from AppSettings.
    /// </summary>
    public abstract void LoadFromSettings(AppSettings settings);

    /// <summary>
    /// Saves settings to AppSettings.
    /// </summary>
    public abstract void SaveToSettings(AppSettings settings);

    /// <summary>
    /// Resets to original values.
    /// </summary>
    public abstract void ResetToOriginal();

    /// <summary>
    /// Helper method to load a boolean property from settings and update the tracker.
    /// </summary>
    protected bool LoadBoolProperty(
        AppSettings settings,
        Expression<Func<AppSettings, bool?>> getter,
        bool defaultValue,
        string propertyName,
        Action<bool> setter)
    {
        var value = settings.GetValueOrDefault(getter, defaultValue);
        setter(value);
        HeaderTracker.UpdateOriginal(propertyName, value);
        return value;
    }

    /// <summary>
    /// Helper method to load a string property from settings and update the tracker.
    /// </summary>
    protected string LoadStringProperty(
        AppSettings settings,
        Expression<Func<AppSettings, string?>> getter,
        string defaultValue,
        string propertyName,
        Action<string> setter)
    {
        var value = settings.GetStringOrDefault(getter, defaultValue);
        setter(value);
        HeaderTracker.UpdateOriginal(propertyName, value);
        return value;
    }

    /// <summary>
    /// Helper method to reset a boolean property to its original value.
    /// </summary>
    protected bool ResetBoolProperty(string propertyName, bool defaultValue, Action<bool> setter)
    {
        var value = HeaderTracker.GetOriginalValue<bool>(propertyName);
        setter(value);
        return value;
    }

    /// <summary>
    /// Helper method to reset a string property to its original value.
    /// </summary>
    protected string ResetStringProperty(string propertyName, string defaultValue, Action<string> setter)
    {
        var value = HeaderTracker.GetOriginalValue<string>(propertyName) ?? defaultValue;
        setter(value);
        return value;
    }

    public virtual void Dispose()
    {
        Resources.CultureChanged -= OnCultureChanged;
        HeaderTracker?.Clear();
    }
}
