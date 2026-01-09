using System;
using System.IO;
using System.Text.Json;

namespace MiniCAM.Core.Settings;

/// <summary>
/// Service implementation for managing application settings persistence.
/// </summary>
public class SettingsService : ISettingsService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private AppSettings _current;

    /// <summary>
    /// Initializes a new instance of the SettingsService class.
    /// </summary>
    public SettingsService()
    {
        _current = Load();
    }

    /// <summary>
    /// Gets the current application settings.
    /// </summary>
    public AppSettings Current => _current;

    /// <summary>
    /// Saves the current settings to disk.
    /// </summary>
    public void Save()
    {
        Save(_current);
    }

    /// <summary>
    /// Reloads settings from disk and updates Current.
    /// </summary>
    public void Reload()
    {
        _current = Load();
    }

    /// <summary>
    /// Saves the current settings to disk (alias for Save).
    /// </summary>
    public void SaveCurrent()
    {
        Save();
    }

    private static string GetSettingsDirectory()
    {
        // Use ApplicationData folder which works cross-platform:
        // Windows: %APPDATA%\MiniCAM
        // Linux: ~/.config/MiniCAM
        // macOS: ~/Library/Application Support/MiniCAM
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var settingsDir = Path.Combine(appDataPath, "MiniCAM");
        
        // Ensure directory exists
        if (!Directory.Exists(settingsDir))
        {
            Directory.CreateDirectory(settingsDir);
        }
        
        return settingsDir;
    }

    private static string GetSettingsFilePath()
    {
        return Path.Combine(GetSettingsDirectory(), "settings.json");
    }

    /// <summary>
    /// Loads application settings from disk.
    /// </summary>
    /// <returns>Loaded settings or default settings if file doesn't exist.</returns>
    private AppSettings Load()
    {
        var settingsPath = GetSettingsFilePath();
        
        if (!File.Exists(settingsPath))
        {
            return new AppSettings();
        }

        try
        {
            var json = File.ReadAllText(settingsPath);
            var settings = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions);
            return settings ?? new AppSettings();
        }
        catch (Exception)
        {
            // If loading fails, return default settings
            return new AppSettings();
        }
    }

    /// <summary>
    /// Saves application settings to disk.
    /// </summary>
    /// <param name="settings">Settings to save.</param>
    private void Save(AppSettings settings)
    {
        if (settings == null)
        {
            throw new ArgumentNullException(nameof(settings));
        }

        try
        {
            var settingsPath = GetSettingsFilePath();
            var json = JsonSerializer.Serialize(settings, JsonOptions);
            File.WriteAllText(settingsPath, json);
        }
        catch (Exception)
        {
            // Silently fail if saving is not possible
            // In production, you might want to log this
        }
    }
}
