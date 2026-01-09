namespace MiniCAM.Core.Settings;

/// <summary>
/// Default values for spindle settings.
/// </summary>
public static class SpindleDefaults
{
    /// <summary>
    /// Default spindle speed in RPM.
    /// </summary>
    public const string Speed = "1000";

    /// <summary>
    /// Default delay value after spindle enable.
    /// </summary>
    public const string DelayValue = "2";

    /// <summary>
    /// Default value for AddSpindleCode setting.
    /// </summary>
    public const bool AddSpindleCode = false;

    /// <summary>
    /// Default value for SetSpindleSpeed setting.
    /// </summary>
    public const bool SetSpindleSpeed = false;

    /// <summary>
    /// Default value for EnableSpindleBeforeOperations setting.
    /// </summary>
    public const bool EnableSpindleBeforeOperations = false;

    /// <summary>
    /// Default value for AddSpindleDelayAfterEnable setting.
    /// </summary>
    public const bool AddSpindleDelayAfterEnable = false;

    /// <summary>
    /// Default value for DisableSpindleAfterOperations setting.
    /// </summary>
    public const bool DisableSpindleAfterOperations = false;
}
