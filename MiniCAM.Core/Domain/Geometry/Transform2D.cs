using System;

namespace MiniCAM.Core.Domain.Geometry;

/// <summary>
/// Represents a 2D transformation (translation, rotation, scale, etc.).
/// Uses a 3x3 homogeneous transformation matrix.
/// </summary>
public class Transform2D
{
    private readonly double[,] _matrix;

    private Transform2D(double[,] matrix)
    {
        _matrix = matrix;
    }

    /// <summary>
    /// Gets the identity transformation (no transformation).
    /// </summary>
    public static Transform2D Identity => new Transform2D(new double[,]
    {
        { 1, 0, 0 },
        { 0, 1, 0 },
        { 0, 0, 1 }
    });

    /// <summary>
    /// Creates a translation transformation.
    /// </summary>
    public static Transform2D Translation(double dx, double dy)
    {
        return new Transform2D(new double[,]
        {
            { 1, 0, dx },
            { 0, 1, dy },
            { 0, 0, 1 }
        });
    }

    /// <summary>
    /// Creates a rotation transformation around the origin.
    /// </summary>
    /// <param name="angle">Rotation angle in radians.</param>
    public static Transform2D Rotation(double angle)
    {
        var cos = Math.Cos(angle);
        var sin = Math.Sin(angle);
        return new Transform2D(new double[,]
        {
            { cos, -sin, 0 },
            { sin, cos, 0 },
            { 0, 0, 1 }
        });
    }

    /// <summary>
    /// Creates a rotation transformation around a specified center point.
    /// </summary>
    /// <param name="angle">Rotation angle in radians.</param>
    /// <param name="center">Center point of rotation.</param>
    public static Transform2D Rotation(double angle, Point2D center)
    {
        // Translate to origin, rotate, translate back
        var t1 = Translation(-center.X, -center.Y);
        var r = Rotation(angle);
        var t2 = Translation(center.X, center.Y);
        return t2.Combine(r.Combine(t1));
    }

    /// <summary>
    /// Creates a scale transformation around the origin.
    /// </summary>
    public static Transform2D Scale(double sx, double sy)
    {
        return new Transform2D(new double[,]
        {
            { sx, 0, 0 },
            { 0, sy, 0 },
            { 0, 0, 1 }
        });
    }

    /// <summary>
    /// Creates a scale transformation around a specified center point.
    /// </summary>
    public static Transform2D Scale(double sx, double sy, Point2D center)
    {
        var t1 = Translation(-center.X, -center.Y);
        var s = Scale(sx, sy);
        var t2 = Translation(center.X, center.Y);
        return t2.Combine(s.Combine(t1));
    }

    /// <summary>
    /// Creates a mirror transformation along the X axis.
    /// </summary>
    public static Transform2D MirrorX()
    {
        return new Transform2D(new double[,]
        {
            { -1, 0, 0 },
            { 0, 1, 0 },
            { 0, 0, 1 }
        });
    }

    /// <summary>
    /// Creates a mirror transformation along the Y axis.
    /// </summary>
    public static Transform2D MirrorY()
    {
        return new Transform2D(new double[,]
        {
            { 1, 0, 0 },
            { 0, -1, 0 },
            { 0, 0, 1 }
        });
    }

    /// <summary>
    /// Combines this transformation with another (this * other).
    /// </summary>
    public Transform2D Combine(Transform2D other)
    {
        var result = new double[3, 3];
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                result[i, j] = 0;
                for (int k = 0; k < 3; k++)
                {
                    result[i, j] += _matrix[i, k] * other._matrix[k, j];
                }
            }
        }
        return new Transform2D(result);
    }

    /// <summary>
    /// Transforms a point using this transformation.
    /// </summary>
    public Point2D Transform(Point2D point)
    {
        var x = _matrix[0, 0] * point.X + _matrix[0, 1] * point.Y + _matrix[0, 2];
        var y = _matrix[1, 0] * point.X + _matrix[1, 1] * point.Y + _matrix[1, 2];
        return new Point2D(x, y);
    }

    /// <summary>
    /// Transforms a vector using this transformation (ignores translation).
    /// </summary>
    public Vector2D Transform(Vector2D vector)
    {
        var x = _matrix[0, 0] * vector.X + _matrix[0, 1] * vector.Y;
        var y = _matrix[1, 0] * vector.X + _matrix[1, 1] * vector.Y;
        return new Vector2D(x, y);
    }
}
