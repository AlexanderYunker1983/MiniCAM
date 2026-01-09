namespace MiniCAM.Core.Settings.Models;

/// <summary>
/// Code generation settings.
/// </summary>
public class CodeGenerationSettings
{
    public bool? UseLineNumbers { get; set; }
    public string? StartLineNumber { get; set; }
    public string? LineNumberStep { get; set; }
    public bool? GenerateComments { get; set; }
    public bool? AllowArcs { get; set; }
    public bool? FormatCommands { get; set; }
    public bool? SetWorkCoordinateSystem { get; set; }
    public string? CoordinateSystem { get; set; }
    public bool? SetAbsoluteCoordinates { get; set; }
    public bool? AllowRelativeCoordinates { get; set; }
    public bool? SetZerosAtStart { get; set; }
    public string? X0 { get; set; }
    public string? Y0 { get; set; }
    public string? Z0 { get; set; }
    public bool? MoveToPointAtEnd { get; set; }
    public string? X { get; set; }
    public string? Y { get; set; }
    public string? Z { get; set; }
    public int? DecimalPlaces { get; set; }
}
