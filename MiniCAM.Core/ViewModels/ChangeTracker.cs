using System;
using System.Collections.Generic;

namespace MiniCAM.Core.ViewModels;

/// <summary>
/// Tracks changes to a single value of type T.
/// </summary>
public class ChangeTracker<T>
{
    private T _originalValue = default!;
    private T _currentValue = default!;

    /// <summary>
    /// Gets or sets the original value (baseline for comparison).
    /// </summary>
    public T OriginalValue
    {
        get => _originalValue;
        set
        {
            _originalValue = value;
            _currentValue = value;
        }
    }

    /// <summary>
    /// Gets or sets the current value.
    /// </summary>
    public T CurrentValue
    {
        get => _currentValue;
        set => _currentValue = value;
    }

    /// <summary>
    /// Gets a value indicating whether the current value differs from the original value.
    /// </summary>
    public bool IsModified => !Equals(_currentValue, _originalValue);

    /// <summary>
    /// Resets the current value to the original value.
    /// </summary>
    public void Reset()
    {
        _currentValue = _originalValue;
    }

    /// <summary>
    /// Updates the original value to match the current value (marks as saved).
    /// </summary>
    public void AcceptChanges()
    {
        _originalValue = _currentValue;
    }

    /// <summary>
    /// Creates a new ChangeTracker with the specified original value.
    /// </summary>
    public static ChangeTracker<T> Create(T originalValue)
    {
        return new ChangeTracker<T> { OriginalValue = originalValue };
    }
}

/// <summary>
/// Manages change tracking for multiple properties by name.
/// </summary>
public class ChangeTracker
{
    private readonly Dictionary<string, object> _trackers = new();

    /// <summary>
    /// Tracks a property by name with its original value.
    /// </summary>
    public void Track<T>(string propertyName, T originalValue)
    {
        _trackers[propertyName] = ChangeTracker<T>.Create(originalValue);
    }

    /// <summary>
    /// Updates the current value for a tracked property.
    /// </summary>
    public void Update<T>(string propertyName, T currentValue)
    {
        if (_trackers.TryGetValue(propertyName, out var tracker) && tracker is ChangeTracker<T> typedTracker)
        {
            typedTracker.CurrentValue = currentValue;
        }
        else
        {
            Track(propertyName, currentValue);
        }
    }

    /// <summary>
    /// Gets the change tracker for a specific property.
    /// </summary>
    public ChangeTracker<T>? GetTracker<T>(string propertyName)
    {
        if (_trackers.TryGetValue(propertyName, out var tracker) && tracker is ChangeTracker<T> typedTracker)
        {
            return typedTracker;
        }
        return null;
    }

    /// <summary>
    /// Checks if a property has been modified.
    /// </summary>
    public bool IsModified<T>(string propertyName)
    {
        var tracker = GetTracker<T>(propertyName);
        return tracker?.IsModified ?? false;
    }

    /// <summary>
    /// Resets a property to its original value.
    /// </summary>
    public void Reset<T>(string propertyName)
    {
        GetTracker<T>(propertyName)?.Reset();
    }

    /// <summary>
    /// Resets all tracked properties to their original values.
    /// </summary>
    public void ResetAll()
    {
        foreach (var tracker in _trackers.Values)
        {
            if (tracker is IResettable resettable)
            {
                resettable.Reset();
            }
        }
    }

    /// <summary>
    /// Accepts all changes (updates original values to current values).
    /// </summary>
    public void AcceptAllChanges()
    {
        foreach (var tracker in _trackers.Values)
        {
            if (tracker is IAcceptable acceptable)
            {
                acceptable.AcceptChanges();
            }
        }
    }

    /// <summary>
    /// Clears all tracked properties.
    /// </summary>
    public void Clear()
    {
        _trackers.Clear();
    }
}

/// <summary>
/// Interface for objects that can be reset.
/// </summary>
internal interface IResettable
{
    void Reset();
}

/// <summary>
/// Interface for objects that can accept changes.
/// </summary>
internal interface IAcceptable
{
    void AcceptChanges();
}
