using System;
using System.Collections.Generic;
using System.Linq;
using MiniCAM.Core.Domain.Geometry;

namespace MiniCAM.Core.Domain.ToolPath;

/// <summary>
/// Represents a complete tool path consisting of multiple movement segments.
/// </summary>
public class ToolPath
{
    private readonly List<ToolPathSegment> _segments = new();
    private Point3D _currentPosition = new Point3D(0, 0, 0);

    /// <summary>
    /// Gets all segments of the tool path.
    /// </summary>
    public IReadOnlyList<ToolPathSegment> Segments => _segments;

    /// <summary>
    /// Gets the current tool position.
    /// </summary>
    public Point3D CurrentPosition => _currentPosition;

    /// <summary>
    /// Adds a move command to the tool path and updates the current position.
    /// </summary>
    /// <param name="move">The move command to add.</param>
    public void AddMove(MoveCommand move)
    {
        if (move == null)
            throw new ArgumentNullException(nameof(move));

        var startPoint = _currentPosition;
        var endPoint = CalculateEndPoint(startPoint, move);
        
        var segment = new ToolPathSegment(move, startPoint, endPoint);
        _segments.Add(segment);
        
        _currentPosition = endPoint;
    }

    /// <summary>
    /// Calculates the end point of a move command from the current position.
    /// </summary>
    private Point3D CalculateEndPoint(Point3D start, MoveCommand move)
    {
        var x = move.X ?? start.X;
        var y = move.Y ?? start.Y;
        var z = move.Z ?? start.Z;
        return new Point3D(x, y, z);
    }

    /// <summary>
    /// Returns all points in the tool path for visualization.
    /// </summary>
    public IEnumerable<Point3D> GetPoints()
    {
        if (_segments.Count == 0)
            yield break;

        yield return _segments[0].StartPoint;
        
        foreach (var segment in _segments)
        {
            yield return segment.EndPoint;
        }
    }

    /// <summary>
    /// Gets the bounding box of the tool path.
    /// </summary>
    public BoundingBox3D GetBounds()
    {
        if (_segments.Count == 0)
            return new BoundingBox3D(new Point3D(0, 0, 0), new Point3D(0, 0, 0));

        var points = GetPoints().ToList();
        if (points.Count == 0)
            return new BoundingBox3D(new Point3D(0, 0, 0), new Point3D(0, 0, 0));

        var minX = points.Min(p => p.X);
        var maxX = points.Max(p => p.X);
        var minY = points.Min(p => p.Y);
        var maxY = points.Max(p => p.Y);
        var minZ = points.Min(p => p.Z);
        var maxZ = points.Max(p => p.Z);

        return new BoundingBox3D(
            new Point3D(minX, minY, minZ),
            new Point3D(maxX, maxY, maxZ));
    }

    /// <summary>
    /// Resets the tool path and current position.
    /// </summary>
    public void Reset()
    {
        _segments.Clear();
        _currentPosition = new Point3D(0, 0, 0);
    }

    /// <summary>
    /// Sets the initial tool position.
    /// </summary>
    public void SetInitialPosition(Point3D position)
    {
        _currentPosition = position;
    }
}
