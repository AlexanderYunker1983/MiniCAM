namespace MiniCAM.Core.Domain.Operations;

/// <summary>
/// Parameters for operation execution and tool path generation.
/// </summary>
public class OperationParameters
{
    /// <summary>
    /// Gets or sets the tool diameter in millimeters.
    /// </summary>
    public double ToolDiameter { get; set; }

    /// <summary>
    /// Gets or sets the safe Z height for rapid movements in millimeters.
    /// </summary>
    public double SafeHeight { get; set; } = 10.0;

    /// <summary>
    /// Gets or sets the default feed rate in millimeters per minute.
    /// </summary>
    public double DefaultFeedRate { get; set; } = 100.0;

    /// <summary>
    /// Gets or sets the default rapid feed rate in millimeters per minute.
    /// </summary>
    public double RapidFeedRate { get; set; } = 1000.0;
}
