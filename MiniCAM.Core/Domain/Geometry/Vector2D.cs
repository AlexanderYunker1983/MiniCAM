using System;

namespace MiniCAM.Core.Domain.Geometry;

/// <summary>
/// Represents a 2D vector.
/// </summary>
public readonly struct Vector2D : IEquatable<Vector2D>
{
    public double X { get; }
    public double Y { get; }

    public Vector2D(double x, double y)
    {
        X = x;
        Y = y;
    }

    /// <summary>
    /// Gets the length (magnitude) of the vector.
    /// </summary>
    public double Length => Math.Sqrt(X * X + Y * Y);

    /// <summary>
    /// Gets the squared length (faster, no square root).
    /// </summary>
    public double LengthSquared => X * X + Y * Y;

    /// <summary>
    /// Gets a normalized vector (unit vector) in the same direction.
    /// </summary>
    public Vector2D Normalized
    {
        get
        {
            var length = Length;
            if (length < double.Epsilon)
                return new Vector2D(0, 0);
            return new Vector2D(X / length, Y / length);
        }
    }

    public static Vector2D operator +(Vector2D a, Vector2D b)
    {
        return new Vector2D(a.X + b.X, a.Y + b.Y);
    }

    public static Vector2D operator -(Vector2D a, Vector2D b)
    {
        return new Vector2D(a.X - b.X, a.Y - b.Y);
    }

    public static Vector2D operator *(Vector2D vector, double scalar)
    {
        return new Vector2D(vector.X * scalar, vector.Y * scalar);
    }

    public static Vector2D operator *(double scalar, Vector2D vector)
    {
        return vector * scalar;
    }

    public static Vector2D operator /(Vector2D vector, double scalar)
    {
        return new Vector2D(vector.X / scalar, vector.Y / scalar);
    }

    public static Vector2D operator -(Vector2D vector)
    {
        return new Vector2D(-vector.X, -vector.Y);
    }

    /// <summary>
    /// Calculates the dot product of two vectors.
    /// </summary>
    public static double Dot(Vector2D a, Vector2D b)
    {
        return a.X * b.X + a.Y * b.Y;
    }

    /// <summary>
    /// Calculates the cross product (Z component) of two vectors.
    /// </summary>
    public static double Cross(Vector2D a, Vector2D b)
    {
        return a.X * b.Y - a.Y * b.X;
    }

    public bool Equals(Vector2D other)
    {
        return X.Equals(other.X) && Y.Equals(other.Y);
    }

    public override bool Equals(object? obj)
    {
        return obj is Vector2D other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y);
    }

    public static bool operator ==(Vector2D left, Vector2D right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Vector2D left, Vector2D right)
    {
        return !left.Equals(right);
    }

    public override string ToString()
    {
        return $"<{X:F3}, {Y:F3}>";
    }
}
