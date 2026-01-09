namespace MiniCAM.Core.Settings;

/// <summary>
/// Default values for code generation settings.
/// </summary>
public static class CodeGenerationDefaults
{
    /// <summary>
    /// Default start line number.
    /// </summary>
    public const string StartLineNumber = "10";

    /// <summary>
    /// Default line number step.
    /// </summary>
    public const string LineNumberStep = "10";

    /// <summary>
    /// Default coordinate value (X, Y, Z, X0, Y0, Z0).
    /// </summary>
    public const string Coordinate = "0";

    /// <summary>
    /// Default value for UseLineNumbers setting.
    /// </summary>
    public const bool UseLineNumbers = true;

    /// <summary>
    /// Default value for SetAbsoluteCoordinates setting.
    /// </summary>
    public const bool SetAbsoluteCoordinates = true;

    /// <summary>
    /// Default value for SetZerosAtStart setting.
    /// </summary>
    public const bool SetZerosAtStart = true;

    /// <summary>
    /// Default value for GenerateComments setting.
    /// </summary>
    public const bool GenerateComments = false;

    /// <summary>
    /// Default value for AllowArcs setting.
    /// </summary>
    public const bool AllowArcs = false;

    /// <summary>
    /// Default value for FormatCommands setting.
    /// </summary>
    public const bool FormatCommands = false;

    /// <summary>
    /// Default value for SetWorkCoordinateSystem setting.
    /// </summary>
    public const bool SetWorkCoordinateSystem = false;

    /// <summary>
    /// Default value for AllowRelativeCoordinates setting.
    /// </summary>
    public const bool AllowRelativeCoordinates = false;

    /// <summary>
    /// Default value for MoveToPointAtEnd setting.
    /// </summary>
    public const bool MoveToPointAtEnd = false;
}
