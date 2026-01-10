using System;
using MiniCAM.Core.Domain.Geometry;

namespace MiniCAM.Core.Domain.Primitives;

/// <summary>
/// Represents a point in 2D space.
/// </summary>
public class Point2DPrimitive : Primitive2D
{
    /// <summary>
    /// Gets or sets the X coordinate.
    /// </summary>
    public double X { get; set; }

    /// <summary>
    /// Gets or sets the Y coordinate.
    /// </summary>
    public double Y { get; set; }

    /// <summary>
    /// Initializes a new instance of the Point2DPrimitive class.
    /// </summary>
    /// <param name="x">The X coordinate.</param>
    /// <param name="y">The Y coordinate.</param>
    /// <param name="name">The name of the point.</param>
    public Point2DPrimitive(double x, double y, string name = "Point")
        : base(name)
    {
        X = x;
        Y = y;
    }

    /// <summary>
    /// Gets the point as a Point2D value type.
    /// </summary>
    public Point2D AsPoint2D => new Point2D(X, Y);

    public override Rect2D GetBounds()
    {
        // A point has zero-size bounds, but we return a minimal rectangle
        return new Rect2D(X, Y, 0, 0);
    }

    public override Primitive2D Transform(Transform2D transform)
    {
        var transformedPoint = transform.Transform(new Point2D(X, Y));
        return new Point2DPrimitive(transformedPoint.X, transformedPoint.Y, Name)
        {
            Id = Id // Preserve ID
        };
    }

    public override Primitive2D Clone()
    {
        return new Point2DPrimitive(X, Y, Name)
        {
            Id = Guid.NewGuid() // New ID for clone
        };
    }
}
