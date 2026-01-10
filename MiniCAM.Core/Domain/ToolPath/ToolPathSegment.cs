using System;
using MiniCAM.Core.Domain.Geometry;

namespace MiniCAM.Core.Domain.ToolPath;

/// <summary>
/// Represents a segment of the tool path with start and end points.
/// </summary>
public class ToolPathSegment
{
    /// <summary>
    /// Gets the movement command for this segment.
    /// </summary>
    public MoveCommand Command { get; }

    /// <summary>
    /// Gets the start point of this segment.
    /// </summary>
    public Point3D StartPoint { get; }

    /// <summary>
    /// Gets the end point of this segment.
    /// </summary>
    public Point3D EndPoint { get; }

    /// <summary>
    /// Gets the type of movement.
    /// </summary>
    public MoveType MoveType => Command.Type;

    /// <summary>
    /// Gets the feed rate for this segment (null for rapid movements).
    /// </summary>
    public double? FeedRate => Command.FeedRate;

    /// <summary>
    /// Initializes a new instance of the ToolPathSegment class.
    /// </summary>
    /// <param name="command">The movement command.</param>
    /// <param name="startPoint">The start point of the segment.</param>
    /// <param name="endPoint">The end point of the segment.</param>
    public ToolPathSegment(MoveCommand command, Point3D startPoint, Point3D endPoint)
    {
        Command = command ?? throw new ArgumentNullException(nameof(command));
        StartPoint = startPoint;
        EndPoint = endPoint;
    }
}
