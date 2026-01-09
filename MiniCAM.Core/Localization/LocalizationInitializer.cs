using System;
using System.Globalization;
using MiniCAM.Core.Settings;

namespace MiniCAM.Core.Localization;

/// <summary>
/// Initializes application localization at startup.
/// </summary>
public static class LocalizationInitializer
{
    // Capture system UI culture once so "auto" is stable even after we change thread culture
    private static readonly CultureInfo SystemUICulture = CultureInfo.InstalledUICulture;

    /// <summary>
    /// Detects the system culture and returns the appropriate culture name.
    /// </summary>
    /// <returns>Culture name based on system settings ("ru-RU" or "en-US").</returns>
    private static string DetectSystemCulture()
    {
        // Use captured OS culture instead of the current thread culture
        var systemCulture = SystemUICulture ?? CultureInfo.CurrentUICulture;
        
        // Check if we have resources for the system culture
        // Use Russian if the system culture is Russian, otherwise English
        if (systemCulture.TwoLetterISOLanguageName == "ru")
        {
            return AppSettings.CultureRussian;
        }
        else
        {
            // Default to English
            return AppSettings.CultureEnglish;
        }
    }

    /// <summary>
    /// Initializes the application culture based on saved settings, system settings, or default.
    /// </summary>
    /// <param name="settingsService">Settings service instance.</param>
    public static void Initialize(ISettingsService settingsService)
    {
        settingsService.Reload();
        var settings = settingsService.Current;
        
        string cultureToUse;
        
        // Determine which culture to use
        if (string.IsNullOrWhiteSpace(settings.Culture))
        {
            // No setting saved, use auto (system detection)
            cultureToUse = DetectSystemCulture();
            
            // Save auto as default setting
            settings.Culture = AppSettings.CultureAuto;
            settingsService.SaveCurrent();
        }
        else if (settings.Culture == AppSettings.CultureAuto)
        {
            // Auto mode - detect from system
            cultureToUse = DetectSystemCulture();
        }
        else
        {
            // Explicit culture setting
            cultureToUse = settings.Culture;
        }
        
        // Apply the culture
        try
        {
            Resources.SetCulture(cultureToUse);
        }
        catch (CultureNotFoundException ex)
        {
            // If culture is invalid, fall back to system detection
            cultureToUse = DetectSystemCulture();
            try
            {
                Resources.SetCulture(cultureToUse);
            }
            catch (Exception fallbackEx)
            {
                // If even fallback fails, use English as last resort
                // Don't throw here - we need application to start even if culture setting fails
                Resources.SetCulture(AppSettings.CultureEnglish);
                System.Diagnostics.Debug.WriteLine($"Failed to set culture. Requested: {cultureToUse}, Fallback failed: {fallbackEx.Message}");
            }
        }
    }

    /// <summary>
    /// Initializes the application culture with the specified culture name.
    /// </summary>
    /// <param name="cultureName">Culture name (e.g., "auto", "en-US", "ru-RU").</param>
    /// <param name="settingsService">Settings service instance.</param>
    public static void Initialize(string cultureName, ISettingsService settingsService)
    {
        SwitchCulture(cultureName, settingsService);
    }

    /// <summary>
    /// Switches the application culture at runtime.
    /// This method can be called during application execution to change the language.
    /// </summary>
    /// <param name="cultureName">Culture name (e.g., "auto", "en-US", "ru-RU").</param>
    /// <param name="settingsService">Settings service instance.</param>
    public static void SwitchCulture(string cultureName, ISettingsService settingsService)
    {
        string cultureToUse;
        
        if (cultureName == AppSettings.CultureAuto)
        {
            // Auto mode - detect from system
            cultureToUse = DetectSystemCulture();
        }
        else
        {
            // Explicit culture setting
            cultureToUse = cultureName;
        }
        
        // Apply the culture (this will raise CultureChanged event)
        try
        {
            Resources.SetCulture(cultureToUse);
        }
        catch (CultureNotFoundException ex)
        {
            // If culture is invalid, fall back to system detection
            cultureToUse = DetectSystemCulture();
            try
            {
                Resources.SetCulture(cultureToUse);
            }
            catch (Exception fallbackEx)
            {
                // If even fallback fails, use English as last resort
                Resources.SetCulture(AppSettings.CultureEnglish);
                // Don't throw here - continue with English culture
            }
        }
        
        // Save to settings (save the original value, not the resolved one)
        try
        {
            var settings = settingsService.Current;
            settings.Culture = cultureName;
            settingsService.SaveCurrent();
        }
        catch (Exception ex)
        {
            // Log error but don't prevent culture change
            // Settings will be saved on next successful save operation
            System.Diagnostics.Debug.WriteLine($"Failed to save culture setting: {ex.Message}");
        }
    }
}

