using System;

namespace MiniCAM.Core.Domain.Geometry;

/// <summary>
/// Represents a point in 2D space.
/// </summary>
public readonly struct Point2D : IEquatable<Point2D>
{
    public double X { get; }
    public double Y { get; }

    public Point2D(double x, double y)
    {
        X = x;
        Y = y;
    }

    /// <summary>
    /// Calculates the distance to another point.
    /// </summary>
    public double DistanceTo(Point2D other)
    {
        var dx = X - other.X;
        var dy = Y - other.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }

    /// <summary>
    /// Calculates the squared distance to another point (faster, no square root).
    /// </summary>
    public double DistanceSquaredTo(Point2D other)
    {
        var dx = X - other.X;
        var dy = Y - other.Y;
        return dx * dx + dy * dy;
    }

    public static Point2D operator +(Point2D point, Vector2D vector)
    {
        return new Point2D(point.X + vector.X, point.Y + vector.Y);
    }

    public static Point2D operator -(Point2D point, Vector2D vector)
    {
        return new Point2D(point.X - vector.X, point.Y - vector.Y);
    }

    public static Vector2D operator -(Point2D a, Point2D b)
    {
        return new Vector2D(a.X - b.X, a.Y - b.Y);
    }

    public bool Equals(Point2D other)
    {
        return X.Equals(other.X) && Y.Equals(other.Y);
    }

    public override bool Equals(object? obj)
    {
        return obj is Point2D other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y);
    }

    public static bool operator ==(Point2D left, Point2D right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Point2D left, Point2D right)
    {
        return !left.Equals(right);
    }

    public override string ToString()
    {
        return $"({X:F3}, {Y:F3})";
    }
}
