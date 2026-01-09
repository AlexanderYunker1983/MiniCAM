namespace MiniCAM.Core.Settings;

/// <summary>
/// Constants for spindle delay parameters (G4 command parameters).
/// </summary>
public static class SpindleDelayParameters
{
    /// <summary>
    /// F - Feed rate parameter (time in seconds).
    /// </summary>
    public const string F = "F";

    /// <summary>
    /// P - Programmed pause parameter (time in milliseconds).
    /// </summary>
    public const string P = "P";

    /// <summary>
    /// Pxx. - Programmed pause with decimal point (time in seconds).
    /// </summary>
    public const string Pxx = "Pxx.";

    /// <summary>
    /// Default delay parameter.
    /// </summary>
    public const string Default = F;
}
