using System;

namespace MiniCAM.Core.Domain.Geometry;

/// <summary>
/// Represents a rectangle in 2D space.
/// </summary>
public readonly struct Rect2D : IEquatable<Rect2D>
{
    public double X { get; }
    public double Y { get; }
    public double Width { get; }
    public double Height { get; }

    public Rect2D(double x, double y, double width, double height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public Rect2D(Point2D topLeft, double width, double height)
        : this(topLeft.X, topLeft.Y, width, height)
    {
    }

    public Rect2D(Point2D topLeft, Point2D bottomRight)
        : this(
            Math.Min(topLeft.X, bottomRight.X),
            Math.Min(topLeft.Y, bottomRight.Y),
            Math.Abs(bottomRight.X - topLeft.X),
            Math.Abs(bottomRight.Y - topLeft.Y))
    {
    }

    public double Left => X;
    public double Top => Y;
    public double Right => X + Width;
    public double Bottom => Y + Height;

    public Point2D TopLeft => new Point2D(Left, Top);
    public Point2D TopRight => new Point2D(Right, Top);
    public Point2D BottomLeft => new Point2D(Left, Bottom);
    public Point2D BottomRight => new Point2D(Right, Bottom);
    public Point2D Center => new Point2D(X + Width / 2, Y + Height / 2);

    /// <summary>
    /// Checks if the rectangle contains the specified point.
    /// </summary>
    public bool Contains(Point2D point)
    {
        return point.X >= Left && point.X <= Right &&
               point.Y >= Top && point.Y <= Bottom;
    }

    /// <summary>
    /// Checks if the rectangle contains another rectangle.
    /// </summary>
    public bool Contains(Rect2D other)
    {
        return Left <= other.Left && Right >= other.Right &&
               Top <= other.Top && Bottom >= other.Bottom;
    }

    /// <summary>
    /// Checks if the rectangle intersects with another rectangle.
    /// </summary>
    public bool IntersectsWith(Rect2D other)
    {
        return Left < other.Right && Right > other.Left &&
               Top < other.Bottom && Bottom > other.Top;
    }

    /// <summary>
    /// Creates a rectangle that is the union of this rectangle and another.
    /// </summary>
    public Rect2D Union(Rect2D other)
    {
        var left = Math.Min(Left, other.Left);
        var top = Math.Min(Top, other.Top);
        var right = Math.Max(Right, other.Right);
        var bottom = Math.Max(Bottom, other.Bottom);
        return new Rect2D(left, top, right - left, bottom - top);
    }

    /// <summary>
    /// Creates a rectangle that is the intersection of this rectangle and another.
    /// </summary>
    public Rect2D Intersect(Rect2D other)
    {
        var left = Math.Max(Left, other.Left);
        var top = Math.Max(Top, other.Top);
        var right = Math.Min(Right, other.Right);
        var bottom = Math.Min(Bottom, other.Bottom);

        if (right < left || bottom < top)
            return new Rect2D(0, 0, 0, 0); // Empty rectangle

        return new Rect2D(left, top, right - left, bottom - top);
    }

    /// <summary>
    /// Creates an empty rectangle.
    /// </summary>
    public static Rect2D Empty => new Rect2D(0, 0, 0, 0);

    public bool IsEmpty => Width <= 0 || Height <= 0;

    public bool Equals(Rect2D other)
    {
        return X.Equals(other.X) && Y.Equals(other.Y) &&
               Width.Equals(other.Width) && Height.Equals(other.Height);
    }

    public override bool Equals(object? obj)
    {
        return obj is Rect2D other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y, Width, Height);
    }

    public static bool operator ==(Rect2D left, Rect2D right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Rect2D left, Rect2D right)
    {
        return !left.Equals(right);
    }

    public override string ToString()
    {
        return $"({X:F3}, {Y:F3}, {Width:F3}, {Height:F3})";
    }
}
