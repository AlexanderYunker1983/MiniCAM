using System.Text.Json.Serialization;
using MiniCAM.Core.Settings.Models;

namespace MiniCAM.Core.Settings;

/// <summary>
/// Application settings model with grouped settings using composition.
/// For JSON serialization compatibility, properties are flattened - nested models are ignored during serialization,
/// and flat legacy properties are used for backward compatibility.
/// </summary>
public class AppSettings
{
    /// <summary>
    /// Constant for automatic culture detection based on system settings.
    /// </summary>
    public const string CultureAuto = "auto";

    /// <summary>
    /// Constant for English (United States) culture.
    /// </summary>
    public const string CultureEnglish = "en-US";

    /// <summary>
    /// Constant for Russian (Russia) culture.
    /// </summary>
    public const string CultureRussian = "ru-RU";

    /// <summary>
    /// Constant for automatic theme (follow system).
    /// </summary>
    public const string ThemeAuto = "auto";

    /// <summary>
    /// Constant for light theme.
    /// </summary>
    public const string ThemeLight = "light";

    /// <summary>
    /// Constant for dark theme.
    /// </summary>
    public const string ThemeDark = "dark";

    /// <summary>
    /// Gets or sets the application-level settings (culture, theme).
    /// This property is ignored during JSON serialization for backward compatibility.
    /// Values are automatically synchronized from flat properties via property setters during deserialization.
    /// </summary>
    [JsonIgnore]
    public ApplicationSettings Application { get; set; } = new();

    /// <summary>
    /// Gets or sets the code generation settings.
    /// This property is ignored during JSON serialization for backward compatibility.
    /// Values are automatically synchronized from flat properties via property setters during deserialization.
    /// </summary>
    [JsonIgnore]
    public CodeGenerationSettings CodeGeneration { get; set; } = new();

    /// <summary>
    /// Gets or sets the spindle control settings.
    /// This property is ignored during JSON serialization for backward compatibility.
    /// Values are automatically synchronized from flat properties via property setters during deserialization.
    /// </summary>
    [JsonIgnore]
    public SpindleSettings Spindle { get; set; } = new();

    /// <summary>
    /// Gets or sets the coolant control settings.
    /// This property is ignored during JSON serialization for backward compatibility.
    /// Values are automatically synchronized from flat properties via property setters during deserialization.
    /// </summary>
    [JsonIgnore]
    public CoolantSettings Coolant { get; set; } = new();

    // Legacy flat properties for backward compatibility with JSON serialization
    // These properties delegate to the nested models
    
    /// <summary>
    /// Gets or sets the selected culture name (legacy property for JSON compatibility).
    /// </summary>
    public string? Culture
    {
        get => Application.Culture;
        set => Application.Culture = value;
    }

    /// <summary>
    /// Gets or sets the selected theme (legacy property for JSON compatibility).
    /// </summary>
    public string? Theme
    {
        get => Application.Theme;
        set => Application.Theme = value;
    }

    // Code Generation Settings (legacy properties)
    public bool? UseLineNumbers
    {
        get => CodeGeneration.UseLineNumbers;
        set => CodeGeneration.UseLineNumbers = value;
    }

    public string? StartLineNumber
    {
        get => CodeGeneration.StartLineNumber;
        set => CodeGeneration.StartLineNumber = value;
    }

    public string? LineNumberStep
    {
        get => CodeGeneration.LineNumberStep;
        set => CodeGeneration.LineNumberStep = value;
    }

    public bool? GenerateComments
    {
        get => CodeGeneration.GenerateComments;
        set => CodeGeneration.GenerateComments = value;
    }

    public bool? AllowArcs
    {
        get => CodeGeneration.AllowArcs;
        set => CodeGeneration.AllowArcs = value;
    }

    public bool? FormatCommands
    {
        get => CodeGeneration.FormatCommands;
        set => CodeGeneration.FormatCommands = value;
    }

    public bool? SetWorkCoordinateSystem
    {
        get => CodeGeneration.SetWorkCoordinateSystem;
        set => CodeGeneration.SetWorkCoordinateSystem = value;
    }

    public string? CoordinateSystem
    {
        get => CodeGeneration.CoordinateSystem;
        set => CodeGeneration.CoordinateSystem = value;
    }

    public bool? SetAbsoluteCoordinates
    {
        get => CodeGeneration.SetAbsoluteCoordinates;
        set => CodeGeneration.SetAbsoluteCoordinates = value;
    }

    public bool? AllowRelativeCoordinates
    {
        get => CodeGeneration.AllowRelativeCoordinates;
        set => CodeGeneration.AllowRelativeCoordinates = value;
    }

    public bool? SetZerosAtStart
    {
        get => CodeGeneration.SetZerosAtStart;
        set => CodeGeneration.SetZerosAtStart = value;
    }

    public string? X0
    {
        get => CodeGeneration.X0;
        set => CodeGeneration.X0 = value;
    }

    public string? Y0
    {
        get => CodeGeneration.Y0;
        set => CodeGeneration.Y0 = value;
    }

    public string? Z0
    {
        get => CodeGeneration.Z0;
        set => CodeGeneration.Z0 = value;
    }

    public bool? MoveToPointAtEnd
    {
        get => CodeGeneration.MoveToPointAtEnd;
        set => CodeGeneration.MoveToPointAtEnd = value;
    }

    public string? X
    {
        get => CodeGeneration.X;
        set => CodeGeneration.X = value;
    }

    public string? Y
    {
        get => CodeGeneration.Y;
        set => CodeGeneration.Y = value;
    }

    public string? Z
    {
        get => CodeGeneration.Z;
        set => CodeGeneration.Z = value;
    }

    public int? DecimalPlaces
    {
        get => CodeGeneration.DecimalPlaces;
        set => CodeGeneration.DecimalPlaces = value;
    }

    // Spindle Settings (legacy properties)
    public bool? AddSpindleCode
    {
        get => Spindle.AddSpindleCode;
        set => Spindle.AddSpindleCode = value;
    }

    public bool? SetSpindleSpeed
    {
        get => Spindle.SetSpindleSpeed;
        set => Spindle.SetSpindleSpeed = value;
    }

    public string? SpindleSpeed
    {
        get => Spindle.SpindleSpeed;
        set => Spindle.SpindleSpeed = value;
    }

    public bool? EnableSpindleBeforeOperations
    {
        get => Spindle.EnableSpindleBeforeOperations;
        set => Spindle.EnableSpindleBeforeOperations = value;
    }

    public string? SpindleEnableCommand
    {
        get => Spindle.SpindleEnableCommand;
        set => Spindle.SpindleEnableCommand = value;
    }

    public bool? AddSpindleDelayAfterEnable
    {
        get => Spindle.AddSpindleDelayAfterEnable;
        set => Spindle.AddSpindleDelayAfterEnable = value;
    }

    public string? SpindleDelayParameter
    {
        get => Spindle.SpindleDelayParameter;
        set => Spindle.SpindleDelayParameter = value;
    }

    public string? SpindleDelayValue
    {
        get => Spindle.SpindleDelayValue;
        set => Spindle.SpindleDelayValue = value;
    }

    public bool? DisableSpindleAfterOperations
    {
        get => Spindle.DisableSpindleAfterOperations;
        set => Spindle.DisableSpindleAfterOperations = value;
    }

    // Coolant Settings (legacy properties)
    public bool? AddCoolantCode
    {
        get => Coolant.AddCoolantCode;
        set => Coolant.AddCoolantCode = value;
    }

    public bool? EnableCoolantAtStart
    {
        get => Coolant.EnableCoolantAtStart;
        set => Coolant.EnableCoolantAtStart = value;
    }

    public bool? DisableCoolantAtEnd
    {
        get => Coolant.DisableCoolantAtEnd;
        set => Coolant.DisableCoolantAtEnd = value;
    }

    /// <summary>
    /// Synchronizes nested models from flat properties.
    /// Note: This method is currently not needed because property setters automatically
    /// synchronize values to nested models. It is kept for future extensibility if needed.
    /// </summary>
    internal void SyncNestedModelsFromFlatProperties()
    {
        // Property setters automatically update nested models during JSON deserialization
        // System.Text.Json calls setters for each property during deserialization,
        // which ensures nested models are automatically populated from flat properties.
        // No manual synchronization is required.
    }
}

