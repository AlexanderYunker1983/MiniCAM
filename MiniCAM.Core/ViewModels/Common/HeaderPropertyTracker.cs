using System;
using System.Collections.Generic;
using Avalonia.Media;
using Avalonia.Threading;
using MiniCAM.Core.Localization;

namespace MiniCAM.Core.ViewModels;

/// <summary>
/// Tracks property changes and manages header text and font style for each property.
/// Uses debouncing to optimize UI updates when multiple properties change rapidly.
/// </summary>
public class HeaderPropertyTracker
{
    private readonly Dictionary<string, PropertyHeaderInfo> _properties = new();
    private readonly DispatcherTimer _updateTimer;
    private bool _needsUpdate;

    /// <summary>
    /// Initializes a new instance of the HeaderPropertyTracker class.
    /// </summary>
    public HeaderPropertyTracker()
    {
        _updateTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(100)
        };
        _updateTimer.Tick += (s, e) =>
        {
            _updateTimer.Stop();
            if (_needsUpdate)
            {
                _needsUpdate = false;
                UpdateAllHeadersImmediate();
            }
        };
    }

    /// <summary>
    /// Registers a property for tracking with its header information.
    /// </summary>
    public void Register<T>(
        string propertyName,
        T originalValue,
        Func<string> getResourceString,
        Action<string> setHeaderText,
        Action<FontStyle> setFontStyle)
    {
        _properties[propertyName] = new PropertyHeaderInfo<T>
        {
            OriginalValue = originalValue,
            CurrentValue = originalValue,
            GetResourceString = getResourceString,
            SetHeaderText = setHeaderText,
            SetFontStyle = setFontStyle
        };
        UpdateHeader(propertyName);
    }

    /// <summary>
    /// Registers a property for tracking with PropertyHeaderViewModel.
    /// </summary>
    public void Register<T>(
        string propertyName,
        T originalValue,
        Func<string> getResourceString,
        PropertyHeaderViewModel headerViewModel)
    {
        _properties[propertyName] = new PropertyHeaderInfo<T>
        {
            OriginalValue = originalValue,
            CurrentValue = originalValue,
            GetResourceString = getResourceString,
            HeaderViewModel = headerViewModel
        };
        UpdateHeader(propertyName);
    }

    /// <summary>
    /// Updates the current value for a tracked property and updates its header.
    /// </summary>
    public void Update<T>(string propertyName, T currentValue)
    {
        if (_properties.TryGetValue(propertyName, out var info) && info is PropertyHeaderInfo<T> typedInfo)
        {
            typedInfo.CurrentValue = currentValue;
            UpdateHeader(propertyName);
        }
    }

    /// <summary>
    /// Updates the header indicator for a specific property.
    /// </summary>
    public void UpdateHeader(string propertyName)
    {
        if (_properties.TryGetValue(propertyName, out var info))
        {
            info.UpdateHeader();
        }
    }

    /// <summary>
    /// Updates all header indicators with debouncing to optimize performance.
    /// Use this method for regular updates that can be batched.
    /// </summary>
    public void UpdateAllHeaders()
    {
        ScheduleHeaderUpdate();
    }

    /// <summary>
    /// Immediately updates all header indicators without debouncing.
    /// Use this method when immediate update is required (e.g., after loading settings).
    /// </summary>
    public void UpdateAllHeadersImmediate()
    {
        _updateTimer.Stop();
        _needsUpdate = false;
        foreach (var propertyName in _properties.Keys)
        {
            UpdateHeader(propertyName);
        }
    }

    /// <summary>
    /// Schedules a header update with debouncing to prevent excessive UI updates.
    /// </summary>
    private void ScheduleHeaderUpdate()
    {
        _needsUpdate = true;
        _updateTimer.Stop();
        _updateTimer.Start();
    }

    /// <summary>
    /// Checks if a property has been modified.
    /// </summary>
    public bool IsModified<T>(string propertyName)
    {
        if (_properties.TryGetValue(propertyName, out var info) && info is PropertyHeaderInfo<T> typedInfo)
        {
            return !Equals(typedInfo.CurrentValue, typedInfo.OriginalValue);
        }
        return false;
    }

    /// <summary>
    /// Gets the original value for a tracked property.
    /// </summary>
    public T? GetOriginalValue<T>(string propertyName)
    {
        if (_properties.TryGetValue(propertyName, out var info) && info is PropertyHeaderInfo<T> typedInfo)
        {
            return typedInfo.OriginalValue;
        }
        return default;
    }

    /// <summary>
    /// Gets the current value for a tracked property.
    /// </summary>
    public T? GetCurrentValue<T>(string propertyName)
    {
        if (_properties.TryGetValue(propertyName, out var info) && info is PropertyHeaderInfo<T> typedInfo)
        {
            return typedInfo.CurrentValue;
        }
        return default;
    }

    /// <summary>
    /// Resets a property to its original value.
    /// </summary>
    public void Reset<T>(string propertyName)
    {
        if (_properties.TryGetValue(propertyName, out var info) && info is PropertyHeaderInfo<T> typedInfo)
        {
            typedInfo.CurrentValue = typedInfo.OriginalValue;
            UpdateHeader(propertyName);
        }
    }

    /// <summary>
    /// Resets all properties to their original values.
    /// </summary>
    public void ResetAll()
    {
        foreach (var info in _properties.Values)
        {
            info.Reset();
            info.UpdateHeader();
        }
    }

    /// <summary>
    /// Accepts all changes (updates original values to current values).
    /// </summary>
    public void AcceptAllChanges()
    {
        foreach (var info in _properties.Values)
        {
            info.AcceptChanges();
            info.UpdateHeader();
        }
    }

    /// <summary>
    /// Updates the original value for a property (typically after saving).
    /// </summary>
    public void UpdateOriginal<T>(string propertyName, T newOriginalValue)
    {
        if (_properties.TryGetValue(propertyName, out var info) && info is PropertyHeaderInfo<T> typedInfo)
        {
            typedInfo.OriginalValue = newOriginalValue;
            typedInfo.CurrentValue = newOriginalValue;
            UpdateHeader(propertyName);
        }
    }

    /// <summary>
    /// Clears all tracked properties and stops any pending updates.
    /// </summary>
    public void Clear()
    {
        _updateTimer.Stop();
        _needsUpdate = false;
        _properties.Clear();
    }
}

/// <summary>
/// Base class for property header information.
/// </summary>
internal abstract class PropertyHeaderInfo
{
    public abstract void UpdateHeader();
    public abstract void Reset();
    public abstract void AcceptChanges();
}

/// <summary>
/// Stores information about a tracked property including its header text and font style.
/// </summary>
internal class PropertyHeaderInfo<T> : PropertyHeaderInfo
{
    public T OriginalValue { get; set; } = default!;
    public T CurrentValue { get; set; } = default!;
    public Func<string> GetResourceString { get; set; } = null!;
    public Action<string>? SetHeaderText { get; set; }
    public Action<FontStyle>? SetFontStyle { get; set; }
    public PropertyHeaderViewModel? HeaderViewModel { get; set; }

    public override void UpdateHeader()
    {
        var isModified = !Equals(CurrentValue, OriginalValue);
        var baseText = GetResourceString();

        if (HeaderViewModel != null)
        {
            // New approach: use PropertyHeaderViewModel
            HeaderViewModel.UpdateState(baseText, isModified);
        }
        else if (SetHeaderText != null && SetFontStyle != null)
        {
            // Legacy approach: use separate actions
            var headerText = isModified ? $"{baseText} *" : baseText;
            var fontStyle = isModified ? FontStyle.Italic : FontStyle.Normal;
            SetHeaderText(headerText);
            SetFontStyle(fontStyle);
        }
    }

    public override void Reset()
    {
        CurrentValue = OriginalValue;
    }

    public override void AcceptChanges()
    {
        OriginalValue = CurrentValue;
    }
}
