using System;
using System.IO;
using System.Text.Json;

namespace MiniCAM.Core.Settings;

/// <summary>
/// Manages application settings persistence.
/// </summary>
public static class SettingsManager
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

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
    public static AppSettings Load()
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
    public static void Save(AppSettings settings)
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

    private static AppSettings _current = Load();

    /// <summary>
    /// Gets the current settings.
    /// </summary>
    public static AppSettings Current
    {
        get => _current;
        private set => _current = value;
    }

    /// <summary>
    /// Reloads settings from disk and updates Current.
    /// </summary>
    public static void Reload()
    {
        Current = Load();
    }

    /// <summary>
    /// Saves the current settings to disk.
    /// </summary>
    public static void SaveCurrent()
    {
        Save(Current);
    }
}

