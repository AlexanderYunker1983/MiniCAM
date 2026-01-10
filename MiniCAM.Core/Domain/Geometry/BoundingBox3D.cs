namespace MiniCAM.Core.Domain.Geometry;

/// <summary>
/// Represents a 3D bounding box.
/// </summary>
public readonly struct BoundingBox3D
{
    public Point3D Min { get; }
    public Point3D Max { get; }

    public BoundingBox3D(Point3D min, Point3D max)
    {
        Min = min;
        Max = max;
    }

    public double Width => Max.X - Min.X;
    public double Height => Max.Y - Min.Y;
    public double Depth => Max.Z - Min.Z;

    public Point3D Center => new Point3D(
        (Min.X + Max.X) / 2,
        (Min.Y + Max.Y) / 2,
        (Min.Z + Max.Z) / 2);

    public bool Contains(Point3D point)
    {
        return point.X >= Min.X && point.X <= Max.X &&
               point.Y >= Min.Y && point.Y <= Max.Y &&
               point.Z >= Min.Z && point.Z <= Max.Z;
    }
}
