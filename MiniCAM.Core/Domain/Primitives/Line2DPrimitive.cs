using System;
using MiniCAM.Core.Domain.Geometry;

namespace MiniCAM.Core.Domain.Primitives;

/// <summary>
/// Represents a line in 2D space.
/// </summary>
public class Line2DPrimitive : Primitive2D
{
    /// <summary>
    /// Gets or sets the start point of the line.
    /// </summary>
    public Point2D Start { get; set; }

    /// <summary>
    /// Gets or sets the end point of the line.
    /// </summary>
    public Point2D End { get; set; }

    /// <summary>
    /// Initializes a new instance of the Line2DPrimitive class.
    /// </summary>
    /// <param name="start">The start point of the line.</param>
    /// <param name="end">The end point of the line.</param>
    /// <param name="name">The name of the line.</param>
    public Line2DPrimitive(Point2D start, Point2D end, string name = "Line")
        : base(name)
    {
        Start = start;
        End = end;
    }

    /// <summary>
    /// Gets the length of the line.
    /// </summary>
    public double Length => Start.DistanceTo(End);

    public override Rect2D GetBounds()
    {
        var minX = Math.Min(Start.X, End.X);
        var maxX = Math.Max(Start.X, End.X);
        var minY = Math.Min(Start.Y, End.Y);
        var maxY = Math.Max(Start.Y, End.Y);
        
        return new Rect2D(minX, minY, maxX - minX, maxY - minY);
    }

    public override Primitive2D Transform(Transform2D transform)
    {
        var transformedStart = transform.Transform(Start);
        var transformedEnd = transform.Transform(End);
        return new Line2DPrimitive(transformedStart, transformedEnd, Name)
        {
            Id = Id // Preserve ID
        };
    }

    public override Primitive2D Clone()
    {
        return new Line2DPrimitive(Start, End, Name)
        {
            Id = Guid.NewGuid() // New ID for clone
        };
    }
}
