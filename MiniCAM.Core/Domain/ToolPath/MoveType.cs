namespace MiniCAM.Core.Domain.ToolPath;

/// <summary>
/// Represents the type of tool movement.
/// </summary>
public enum MoveType
{
    /// <summary>
    /// Rapid movement (G0) - fastest movement, no cutting.
    /// </summary>
    Rapid,

    /// <summary>
    /// Linear movement (G1) - straight line movement with feed rate.
    /// </summary>
    Linear,

    /// <summary>
    /// Clockwise arc movement (G2).
    /// </summary>
    ArcCW,

    /// <summary>
    /// Counter-clockwise arc movement (G3).
    /// </summary>
    ArcCCW
}
