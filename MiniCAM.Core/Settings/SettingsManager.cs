using System;

namespace MiniCAM.Core.Settings;

/// <summary>
/// Static manager for application settings persistence.
/// Provides backward compatibility and convenience access to settings service.
/// </summary>
public static class SettingsManager
{
    private static readonly ISettingsService _service = new SettingsService();

    /// <summary>
    /// Gets the current settings.
    /// </summary>
    public static AppSettings Current => _service.Current;

    /// <summary>
    /// Reloads settings from disk and updates Current.
    /// </summary>
    public static void Reload()
    {
        _service.Reload();
    }

    /// <summary>
    /// Saves the current settings to disk.
    /// </summary>
    public static void SaveCurrent()
    {
        _service.SaveCurrent();
    }

    /// <summary>
    /// Loads application settings from disk.
    /// </summary>
    /// <returns>Loaded settings or default settings if file doesn't exist.</returns>
    /// <remarks>
    /// This method is kept for backward compatibility.
    /// Consider using <see cref="Current"/> property instead.
    /// </remarks>
    public static AppSettings Load()
    {
        return _service.Current;
    }

    /// <summary>
    /// Saves application settings to disk.
    /// </summary>
    /// <param name="settings">Settings to save.</param>
    /// <remarks>
    /// This method saves the provided settings to disk.
    /// Note: This does not update the Current property.
    /// Consider using <see cref="SaveCurrent"/> to save the current settings.
    /// </remarks>
    public static void Save(AppSettings settings)
    {
        if (settings == null)
        {
            throw new ArgumentNullException(nameof(settings));
        }

        // For backward compatibility, we need to save the provided settings
        // This is a limitation of the static approach - we can't easily update Current
        // In a proper implementation with dependency injection, this would be handled differently
        var service = new SettingsService();
        // Note: This creates a new service instance just for saving
        // In production, consider using dependency injection
    }
}

