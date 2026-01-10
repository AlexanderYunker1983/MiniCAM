using System;

namespace MiniCAM.Core.Domain.Geometry;

/// <summary>
/// Represents a point in 3D space.
/// </summary>
public readonly struct Point3D : IEquatable<Point3D>
{
    public double X { get; }
    public double Y { get; }
    public double Z { get; }

    public Point3D(double x, double y, double z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    /// <summary>
    /// Calculates the distance to another point.
    /// </summary>
    public double DistanceTo(Point3D other)
    {
        var dx = X - other.X;
        var dy = Y - other.Y;
        var dz = Z - other.Z;
        return Math.Sqrt(dx * dx + dy * dy + dz * dz);
    }

    /// <summary>
    /// Calculates the squared distance to another point (faster, no square root).
    /// </summary>
    public double DistanceSquaredTo(Point3D other)
    {
        var dx = X - other.X;
        var dy = Y - other.Y;
        var dz = Z - other.Z;
        return dx * dx + dy * dy + dz * dz;
    }

    public static Point3D operator +(Point3D point, Vector3D vector)
    {
        return new Point3D(point.X + vector.X, point.Y + vector.Y, point.Z + vector.Z);
    }

    public static Point3D operator -(Point3D point, Vector3D vector)
    {
        return new Point3D(point.X - vector.X, point.Y - vector.Y, point.Z - vector.Z);
    }

    public static Vector3D operator -(Point3D a, Point3D b)
    {
        return new Vector3D(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    }

    public bool Equals(Point3D other)
    {
        return X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);
    }

    public override bool Equals(object? obj)
    {
        return obj is Point3D other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y, Z);
    }

    public static bool operator ==(Point3D left, Point3D right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Point3D left, Point3D right)
    {
        return !left.Equals(right);
    }

    public override string ToString()
    {
        return $"({X:F3}, {Y:F3}, {Z:F3})";
    }
}
