namespace MiniCAM.Core.Settings.Models;

/// <summary>
/// Coolant control settings.
/// </summary>
public class CoolantSettings
{
    public bool? AddCoolantCode { get; set; }
    public bool? EnableCoolantAtStart { get; set; }
    public bool? DisableCoolantAtEnd { get; set; }
}
