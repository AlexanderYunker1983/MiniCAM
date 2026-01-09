using System;
using System.Globalization;
using System.Resources;
using MiniCAM.Core.Settings;

namespace MiniCAM.Core.Localization;

/// <summary>
/// Provides access to localized string resources.
/// </summary>
public static class Resources
{
    private static readonly ResourceManager ResourceManager = new("MiniCAM.Core.Localization.Resources", typeof(Resources).Assembly);

    /// <summary>
    /// Event that is raised when the culture changes.
    /// </summary>
    public static event EventHandler<CultureChangedEventArgs>? CultureChanged;

    /// <summary>
    /// Gets the current culture for localization.
    /// </summary>
    public static CultureInfo? CurrentCulture
    {
        get => CultureInfo.CurrentUICulture;
        set
        {
            var oldCulture = CultureInfo.CurrentUICulture;
            CultureInfo.CurrentUICulture = value ?? CultureInfo.InvariantCulture;
            
            // Raise event if culture actually changed
            if (oldCulture.Name != CultureInfo.CurrentUICulture.Name)
            {
                CultureChanged?.Invoke(null, new CultureChangedEventArgs(oldCulture, CultureInfo.CurrentUICulture));
            }
        }
    }

    /// <summary>
    /// Gets the localized string for the specified resource name.
    /// </summary>
    /// <param name="name">The name of the resource to retrieve.</param>
    /// <returns>The localized string, or null if not found.</returns>
    public static string? GetString(string name)
    {
        return ResourceManager.GetString(name, CurrentCulture);
    }

    /// <summary>
    /// Gets the localized string for the specified resource name.
    /// </summary>
    /// <param name="name">The name of the resource to retrieve.</param>
    /// <param name="defaultValue">The default value to return if the resource is not found.</param>
    /// <returns>The localized string, or the default value if not found.</returns>
    public static string GetString(string name, string defaultValue)
    {
        return ResourceManager.GetString(name, CurrentCulture) ?? defaultValue;
    }

    /// <summary>
    /// Sets the application culture to the specified language.
    /// </summary>
    /// <param name="cultureName">Culture name (e.g., "en-US", "ru-RU").</param>
    public static void SetCulture(string cultureName)
    {
        try
        {
            CurrentCulture = new CultureInfo(cultureName);
        }
        catch (CultureNotFoundException)
        {
            // Fallback to English culture if specified culture is not found
            // This ensures localized strings are still available
            try
            {
                CurrentCulture = new CultureInfo(Settings.AppSettings.CultureEnglish);
            }
            catch
            {
                // Last resort: use InvariantCulture only if English also fails
                CurrentCulture = CultureInfo.InvariantCulture;
            }
        }
    }

    // Example properties for common strings
    public static string AppName => GetString(nameof(AppName), "MiniCAM");
    public static string Welcome => GetString(nameof(Welcome), "Welcome");
    public static string WindowTitle => GetString(nameof(WindowTitle), "MiniCAM");
    
    // Menu strings
    public static string MenuSettings => GetString(nameof(MenuSettings), "Settings");
    public static string MenuApplicationSettings => GetString(nameof(MenuApplicationSettings), "Application Settings");
    public static string MenuCodeGenerationSettings => GetString(nameof(MenuCodeGenerationSettings), "Code Generation Settings");

    // Ribbon tab headers
    public static string RibbonTabDrilling => GetString(nameof(RibbonTabDrilling), "Drilling");
    public static string RibbonTabPocket => GetString(nameof(RibbonTabPocket), "Pocket");
    public static string RibbonTabProfile => GetString(nameof(RibbonTabProfile), "Profile");
    public static string RibbonTabOther => GetString(nameof(RibbonTabOther), "Other");

    // Application settings window strings
    public static string ApplicationSettingsTitle => GetString(nameof(ApplicationSettingsTitle), "Application settings");
    public static string LanguageLabel => GetString(nameof(LanguageLabel), "Application language");
    public static string ButtonApply => GetString(nameof(ButtonApply), "Apply");
    public static string ButtonReset => GetString(nameof(ButtonReset), "Reset changes");
    public static string LanguageAuto => GetString(nameof(LanguageAuto), "Auto (system)");
    public static string LanguageEnglish => GetString(nameof(LanguageEnglish), "English");
    public static string LanguageRussian => GetString(nameof(LanguageRussian), "Russian");

    public static string ThemeLabel => GetString(nameof(ThemeLabel), "Application theme");
    public static string ThemeAuto => GetString(nameof(ThemeAuto), "System");
    public static string ThemeLight => GetString(nameof(ThemeLight), "Light");
    public static string ThemeDark => GetString(nameof(ThemeDark), "Dark");

    // Code generation settings window strings
    public static string CodeGenerationSettingsTitle => GetString(nameof(CodeGenerationSettingsTitle), "Settings");
    public static string TabCodeGeneration => GetString(nameof(TabCodeGeneration), "Code Generation");
    public static string TabSpindle => GetString(nameof(TabSpindle), "Spindle");
    public static string TabCoolant => GetString(nameof(TabCoolant), "Coolant");
    public static string CoolantAddCode => GetString(nameof(CoolantAddCode), "Add coolant control code");
    public static string CoolantEnableAtStart => GetString(nameof(CoolantEnableAtStart), "Enable coolant at program start (M8)");
    public static string CoolantDisableAtEnd => GetString(nameof(CoolantDisableAtEnd), "Disable coolant at program end (M9)");
    
    // Spindle settings strings
    public static string SpindleAddCode => GetString(nameof(SpindleAddCode), "Add spindle control code");
    public static string SpindleSetSpeed => GetString(nameof(SpindleSetSpeed), "Set spindle speed");
    public static string SpindleSpeedLabel => GetString(nameof(SpindleSpeedLabel), "Spindle rotation speed, rpm");
    public static string SpindleEnableBeforeOperations => GetString(nameof(SpindleEnableBeforeOperations), "Enable spindle before operations");
    public static string SpindleEnableCommandLabel => GetString(nameof(SpindleEnableCommandLabel), "Enable command");
    public static string SpindleEnableCommandM3 => GetString(nameof(SpindleEnableCommandM3), "M3");
    public static string SpindleEnableCommandM4 => GetString(nameof(SpindleEnableCommandM4), "M4");
    public static string SpindleAddDelayAfterEnable => GetString(nameof(SpindleAddDelayAfterEnable), "Add delay after enable (G4)");
    public static string SpindleDelayParameterLabel => GetString(nameof(SpindleDelayParameterLabel), "Delay parameter");
    public static string SpindleDelayParameterF => GetString(nameof(SpindleDelayParameterF), "F");
    public static string SpindleDelayParameterP => GetString(nameof(SpindleDelayParameterP), "P");
    public static string SpindleDelayParameterPxx => GetString(nameof(SpindleDelayParameterPxx), "Pxx.");
    public static string SpindleDelayValueLabel => GetString(nameof(SpindleDelayValueLabel), "Parameter value");
    public static string SpindleDisableAfterOperations => GetString(nameof(SpindleDisableAfterOperations), "Disable spindle after operations (M5)");
    
    // Code generation settings strings
    public static string CodeGenUseLineNumbers => GetString(nameof(CodeGenUseLineNumbers), "Use line numbers (N...)");
    public static string CodeGenStartLineNumber => GetString(nameof(CodeGenStartLineNumber), "Start line number");
    public static string CodeGenLineNumberStep => GetString(nameof(CodeGenLineNumberStep), "Line number step");
    public static string CodeGenGenerateComments => GetString(nameof(CodeGenGenerateComments), "Generate comments");
    public static string CodeGenAllowArcs => GetString(nameof(CodeGenAllowArcs), "Allow arcs (G2/G3)");
    public static string CodeGenFormatCommands => GetString(nameof(CodeGenFormatCommands), "Format commands G01/G00 instead of G1/G0");
    public static string CodeGenSetWorkCoordinateSystem => GetString(nameof(CodeGenSetWorkCoordinateSystem), "Set work coordinate system at program start");
    public static string CodeGenCoordinateSystemLabel => GetString(nameof(CodeGenCoordinateSystemLabel), "Coordinate system");
    public static string CodeGenCoordinateSystemG54 => GetString(nameof(CodeGenCoordinateSystemG54), "G54");
    public static string CodeGenCoordinateSystemG55 => GetString(nameof(CodeGenCoordinateSystemG55), "G55");
    public static string CodeGenCoordinateSystemG56 => GetString(nameof(CodeGenCoordinateSystemG56), "G56");
    public static string CodeGenCoordinateSystemG57 => GetString(nameof(CodeGenCoordinateSystemG57), "G57");
    public static string CodeGenCoordinateSystemG58 => GetString(nameof(CodeGenCoordinateSystemG58), "G58");
    public static string CodeGenCoordinateSystemG59 => GetString(nameof(CodeGenCoordinateSystemG59), "G59");
    public static string CodeGenSetAbsoluteCoordinates => GetString(nameof(CodeGenSetAbsoluteCoordinates), "Set absolute coordinate system G90");
    public static string CodeGenAllowRelativeCoordinates => GetString(nameof(CodeGenAllowRelativeCoordinates), "Allow relative coordinate system within individual operations (G91)");
    public static string CodeGenSetZerosAtStart => GetString(nameof(CodeGenSetZerosAtStart), "Set zeros at program start (G92)");
    public static string CodeGenX0 => GetString(nameof(CodeGenX0), "X0");
    public static string CodeGenY0 => GetString(nameof(CodeGenY0), "Y0");
    public static string CodeGenZ0 => GetString(nameof(CodeGenZ0), "Z0");
    public static string CodeGenMoveToPointAtEnd => GetString(nameof(CodeGenMoveToPointAtEnd), "Move to point at program end");
    public static string CodeGenX => GetString(nameof(CodeGenX), "X");
    public static string CodeGenY => GetString(nameof(CodeGenY), "Y");
    public static string CodeGenZ => GetString(nameof(CodeGenZ), "Z");
    
    // Main view strings
    public static string OperationProperties => GetString(nameof(OperationProperties), "Operation Properties");
    public static string Preview2D => GetString(nameof(Preview2D), "2D Preview");
    public static string OperationsList => GetString(nameof(OperationsList), "Operations List");
    public static string GCode => GetString(nameof(GCode), "G-code");
}

/// <summary>
/// Event arguments for culture change events.
/// </summary>
public class CultureChangedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the previous culture.
    /// </summary>
    public CultureInfo OldCulture { get; }

    /// <summary>
    /// Gets the new culture.
    /// </summary>
    public CultureInfo NewCulture { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CultureChangedEventArgs"/> class.
    /// </summary>
    /// <param name="oldCulture">The previous culture.</param>
    /// <param name="newCulture">The new culture.</param>
    public CultureChangedEventArgs(CultureInfo oldCulture, CultureInfo newCulture)
    {
        OldCulture = oldCulture;
        NewCulture = newCulture;
    }
}

