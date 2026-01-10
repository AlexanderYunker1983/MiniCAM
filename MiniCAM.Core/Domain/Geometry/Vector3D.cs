using System;

namespace MiniCAM.Core.Domain.Geometry;

/// <summary>
/// Represents a 3D vector.
/// </summary>
public readonly struct Vector3D : IEquatable<Vector3D>
{
    public double X { get; }
    public double Y { get; }
    public double Z { get; }

    public Vector3D(double x, double y, double z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    /// <summary>
    /// Gets the length (magnitude) of the vector.
    /// </summary>
    public double Length => Math.Sqrt(X * X + Y * Y + Z * Z);

    /// <summary>
    /// Gets the squared length (faster, no square root).
    /// </summary>
    public double LengthSquared => X * X + Y * Y + Z * Z;

    public static Vector3D operator +(Vector3D a, Vector3D b)
    {
        return new Vector3D(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    }

    public static Vector3D operator -(Vector3D a, Vector3D b)
    {
        return new Vector3D(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    }

    public static Vector3D operator *(Vector3D vector, double scalar)
    {
        return new Vector3D(vector.X * scalar, vector.Y * scalar, vector.Z * scalar);
    }

    public static Vector3D operator *(double scalar, Vector3D vector)
    {
        return vector * scalar;
    }

    public static Vector3D operator /(Vector3D vector, double scalar)
    {
        return new Vector3D(vector.X / scalar, vector.Y / scalar, vector.Z / scalar);
    }

    public static Vector3D operator -(Vector3D vector)
    {
        return new Vector3D(-vector.X, -vector.Y, -vector.Z);
    }

    public bool Equals(Vector3D other)
    {
        return X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);
    }

    public override bool Equals(object? obj)
    {
        return obj is Vector3D other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y, Z);
    }

    public static bool operator ==(Vector3D left, Vector3D right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Vector3D left, Vector3D right)
    {
        return !left.Equals(right);
    }

    public override string ToString()
    {
        return $"<{X:F3}, {Y:F3}, {Z:F3}>";
    }
}
