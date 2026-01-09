using System;
using System.IO;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

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

    private readonly ILogger<SettingsService>? _logger;
    private AppSettings _current;

    /// <summary>
    /// Initializes a new instance of the SettingsService class.
    /// </summary>
    /// <param name="logger">Optional logger instance for error logging.</param>
    public SettingsService(ILogger<SettingsService>? logger = null)
    {
        _logger = logger;
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
            _logger?.LogDebug("Settings file not found at {SettingsPath}, using default settings", settingsPath);
            return new AppSettings();
        }

        try
        {
            var json = File.ReadAllText(settingsPath, Encoding.UTF8);
            var settings = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions);
            
            if (settings == null)
            {
                _logger?.LogWarning("Failed to deserialize settings from {SettingsPath}, using default settings", settingsPath);
                return new AppSettings();
            }
            
            _logger?.LogDebug("Settings loaded successfully from {SettingsPath}", settingsPath);
            return settings;
        }
        catch (JsonException ex)
        {
            _logger?.LogError(ex, "Invalid JSON format in settings file {SettingsPath}, using default settings", settingsPath);
            return new AppSettings();
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger?.LogError(ex, "Access denied when reading settings file {SettingsPath}, using default settings", settingsPath);
            return new AppSettings();
        }
        catch (IOException ex)
        {
            _logger?.LogError(ex, "IO error when reading settings file {SettingsPath}, using default settings", settingsPath);
            return new AppSettings();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Unexpected error when loading settings from {SettingsPath}, using default settings", settingsPath);
            return new AppSettings();
        }
    }

    /// <summary>
    /// Saves application settings to disk.
    /// </summary>
    /// <param name="settings">Settings to save.</param>
    /// <exception cref="ArgumentNullException">Thrown when settings is null.</exception>
    private void Save(AppSettings settings)
    {
        if (settings == null)
        {
            throw new ArgumentNullException(nameof(settings));
        }

        var settingsPath = GetSettingsFilePath();
        
        try
        {
            // Ensure directory exists before saving
            var settingsDir = GetSettingsDirectory();
            
            var json = JsonSerializer.Serialize(settings, JsonOptions);
            File.WriteAllText(settingsPath, json, Encoding.UTF8);
            _logger?.LogDebug("Settings saved successfully to {SettingsPath}", settingsPath);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger?.LogError(ex, "Access denied when saving settings to {SettingsPath}. Settings were not saved.", settingsPath);
            // Note: For backward compatibility, we don't throw here, but log the error.
            // In future versions, consider returning a result or throwing to allow caller to handle.
        }
        catch (DirectoryNotFoundException ex)
        {
            _logger?.LogError(ex, "Directory not found when saving settings to {SettingsPath}. Attempting to create directory.", settingsPath);
            try
            {
                var settingsDir = GetSettingsDirectory();
                var json = JsonSerializer.Serialize(settings, JsonOptions);
                File.WriteAllText(settingsPath, json, Encoding.UTF8);
                _logger?.LogInformation("Settings saved successfully after creating directory {SettingsPath}", settingsPath);
            }
            catch (Exception retryEx)
            {
                _logger?.LogError(retryEx, "Failed to save settings after directory creation attempt to {SettingsPath}", settingsPath);
            }
        }
        catch (IOException ex)
        {
            _logger?.LogError(ex, "IO error when saving settings to {SettingsPath}. Settings were not saved.", settingsPath);
            // Note: For backward compatibility, we don't throw here, but log the error.
        }
        catch (JsonException ex)
        {
            _logger?.LogError(ex, "JSON serialization error when saving settings to {SettingsPath}. This indicates a problem with the settings object.", settingsPath);
            // Note: This should not happen in normal operation. Consider throwing in future versions.
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Unexpected error when saving settings to {SettingsPath}. Settings were not saved.", settingsPath);
            // Note: For backward compatibility, we don't throw here, but log the error.
        }
    }
}
