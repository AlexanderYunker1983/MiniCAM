namespace MiniCAM.Core.Settings;

/// <summary>
/// Constants for spindle control commands.
/// </summary>
public static class SpindleCommands
{
    /// <summary>
    /// M3 - Spindle clockwise rotation.
    /// </summary>
    public const string M3 = "M3";

    /// <summary>
    /// M4 - Spindle counterclockwise rotation.
    /// </summary>
    public const string M4 = "M4";

    /// <summary>
    /// M5 - Spindle stop.
    /// </summary>
    public const string M5 = "M5";

    /// <summary>
    /// Default spindle enable command.
    /// </summary>
    public const string DefaultEnableCommand = M3;
}
