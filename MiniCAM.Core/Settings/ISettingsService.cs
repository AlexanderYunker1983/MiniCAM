namespace MiniCAM.Core.Settings;

/// <summary>
/// Service interface for managing application settings persistence.
/// </summary>
public interface ISettingsService
{
    /// <summary>
    /// Gets the current application settings.
    /// </summary>
    AppSettings Current { get; }

    /// <summary>
    /// Saves the current settings to disk.
    /// </summary>
    void Save();

    /// <summary>
    /// Reloads settings from disk and updates Current.
    /// </summary>
    void Reload();

    /// <summary>
    /// Saves the current settings to disk (alias for Save).
    /// </summary>
    void SaveCurrent();
}
