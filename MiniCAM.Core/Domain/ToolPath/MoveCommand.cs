using System;

namespace MiniCAM.Core.Domain.ToolPath;

/// <summary>
/// Represents a single tool movement command.
/// </summary>
public class MoveCommand
{
    /// <summary>
    /// Gets the type of movement.
    /// </summary>
    public MoveType Type { get; }

    /// <summary>
    /// Gets the target X coordinate (null if unchanged).
    /// </summary>
    public double? X { get; }

    /// <summary>
    /// Gets the target Y coordinate (null if unchanged).
    /// </summary>
    public double? Y { get; }

    /// <summary>
    /// Gets the target Z coordinate (null if unchanged).
    /// </summary>
    public double? Z { get; }

    /// <summary>
    /// Gets the feed rate for linear movements (null for rapid movements).
    /// </summary>
    public double? FeedRate { get; }

    /// <summary>
    /// Gets the I parameter for arc center offset (for arc movements).
    /// </summary>
    public double? I { get; }

    /// <summary>
    /// Gets the J parameter for arc center offset (for arc movements).
    /// </summary>
    public double? J { get; }

    /// <summary>
    /// Gets the K parameter for arc center offset (for arc movements in 3D).
    /// </summary>
    public double? K { get; }

    /// <summary>
    /// Gets the R parameter for arc radius (alternative to I/J/K).
    /// </summary>
    public double? R { get; }

    /// <summary>
    /// Initializes a new instance of the MoveCommand class for rapid or linear movement.
    /// </summary>
    /// <param name="type">The type of movement (Rapid or Linear).</param>
    /// <param name="x">Target X coordinate, or null if unchanged.</param>
    /// <param name="y">Target Y coordinate, or null if unchanged.</param>
    /// <param name="z">Target Z coordinate, or null if unchanged.</param>
    /// <param name="feedRate">Feed rate for linear movements, or null for rapid movements.</param>
    public MoveCommand(MoveType type, double? x = null, double? y = null, double? z = null, double? feedRate = null)
    {
        Type = type;
        X = x;
        Y = y;
        Z = z;
        FeedRate = feedRate;
    }

    /// <summary>
    /// Initializes a new instance of the MoveCommand class for arc movement.
    /// </summary>
    /// <param name="type">The type of arc movement (ArcCW or ArcCCW).</param>
    /// <param name="x">Target X coordinate.</param>
    /// <param name="y">Target Y coordinate.</param>
    /// <param name="z">Target Z coordinate, or null if unchanged.</param>
    /// <param name="i">I parameter for arc center offset.</param>
    /// <param name="j">J parameter for arc center offset.</param>
    /// <param name="k">K parameter for arc center offset (for 3D arcs).</param>
    /// <param name="feedRate">Feed rate for the arc movement.</param>
    public MoveCommand(MoveType type, double x, double y, double? z, double i, double j, double? k = null, double? feedRate = null)
    {
        if (type != MoveType.ArcCW && type != MoveType.ArcCCW)
            throw new ArgumentException("Arc constructor can only be used for ArcCW or ArcCCW move types.", nameof(type));

        Type = type;
        X = x;
        Y = y;
        Z = z;
        I = i;
        J = j;
        K = k;
        FeedRate = feedRate;
    }

    /// <summary>
    /// Initializes a new instance of the MoveCommand class for arc movement using radius.
    /// </summary>
    /// <param name="type">The type of arc movement (ArcCW or ArcCCW).</param>
    /// <param name="x">Target X coordinate.</param>
    /// <param name="y">Target Y coordinate.</param>
    /// <param name="z">Target Z coordinate, or null if unchanged.</param>
    /// <param name="r">Arc radius.</param>
    /// <param name="feedRate">Feed rate for the arc movement.</param>
    public MoveCommand(MoveType type, double x, double y, double? z, double r, double? feedRate = null)
    {
        if (type != MoveType.ArcCW && type != MoveType.ArcCCW)
            throw new ArgumentException("Arc radius constructor can only be used for ArcCW or ArcCCW move types.", nameof(type));

        Type = type;
        X = x;
        Y = y;
        Z = z;
        R = r;
        FeedRate = feedRate;
    }
}
