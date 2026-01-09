namespace MiniCAM.Core.Settings.Models;

/// <summary>
/// Spindle control settings.
/// </summary>
public class SpindleSettings
{
    public bool? AddSpindleCode { get; set; }
    public bool? SetSpindleSpeed { get; set; }
    public string? SpindleSpeed { get; set; }
    public bool? EnableSpindleBeforeOperations { get; set; }
    public string? SpindleEnableCommand { get; set; }
    public bool? AddSpindleDelayAfterEnable { get; set; }
    public string? SpindleDelayParameter { get; set; }
    public string? SpindleDelayValue { get; set; }
    public bool? DisableSpindleAfterOperations { get; set; }
}
