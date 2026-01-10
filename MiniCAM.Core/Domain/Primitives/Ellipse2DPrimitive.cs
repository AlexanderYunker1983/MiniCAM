using System;
using MiniCAM.Core.Domain.Geometry;

namespace MiniCAM.Core.Domain.Primitives;

/// <summary>
/// Represents an ellipse in 2D space.
/// </summary>
public class Ellipse2DPrimitive : Primitive2D
{
    /// <summary>
    /// Gets or sets the center point of the ellipse.
    /// </summary>
    public Point2D Center { get; set; }

    /// <summary>
    /// Gets or sets the radius along the X axis.
    /// </summary>
    public double RadiusX { get; set; }

    /// <summary>
    /// Gets or sets the radius along the Y axis.
    /// </summary>
    public double RadiusY { get; set; }

    /// <summary>
    /// Gets or sets the rotation angle in radians (counter-clockwise from positive X axis).
    /// </summary>
    public double RotationAngle { get; set; }

    /// <summary>
    /// Initializes a new instance of the Ellipse2DPrimitive class.
    /// </summary>
    /// <param name="center">The center point of the ellipse.</param>
    /// <param name="radiusX">The radius along the X axis.</param>
    /// <param name="radiusY">The radius along the Y axis.</param>
    /// <param name="rotationAngle">The rotation angle in radians (default: 0).</param>
    /// <param name="name">The name of the ellipse.</param>
    public Ellipse2DPrimitive(Point2D center, double radiusX, double radiusY, double rotationAngle = 0, string name = "Ellipse")
        : base(name)
    {
        Center = center;
        RadiusX = radiusX;
        RadiusY = radiusY;
        RotationAngle = rotationAngle;
    }

    public override Rect2D GetBounds()
    {
        // For a rotated ellipse, we need to calculate the bounding box
        // This is a simplified version - for exact bounds, we'd need to find the extrema
        // For now, use an axis-aligned bounding box that contains the ellipse
        
        // If not rotated, simple calculation
        if (Math.Abs(RotationAngle) < 0.001)
        {
            return new Rect2D(
                Center.X - RadiusX,
                Center.Y - RadiusY,
                RadiusX * 2,
                RadiusY * 2);
        }

        // For rotated ellipse, approximate with bounding box of unrotated ellipse
        // This is not exact but sufficient for most cases
        // Exact calculation would require finding extrema of rotated ellipse
        var cos = Math.Abs(Math.Cos(RotationAngle));
        var sin = Math.Abs(Math.Sin(RotationAngle));
        var width = 2 * (RadiusX * cos + RadiusY * sin);
        var height = 2 * (RadiusX * sin + RadiusY * cos);
        
        return new Rect2D(
            Center.X - width / 2,
            Center.Y - height / 2,
            width,
            height);
    }

    public override Primitive2D Transform(Transform2D transform)
    {
        var transformedCenter = transform.Transform(Center);
        
        // For transformation, we'll apply it to the center and adjust radii if scaling is involved
        // This is a simplified version - full implementation would handle rotation properly
        return new Ellipse2DPrimitive(transformedCenter, RadiusX, RadiusY, RotationAngle, Name)
        {
            Id = Id // Preserve ID
        };
    }

    public override Primitive2D Clone()
    {
        return new Ellipse2DPrimitive(Center, RadiusX, RadiusY, RotationAngle, Name)
        {
            Id = Guid.NewGuid() // New ID for clone
        };
    }
}
