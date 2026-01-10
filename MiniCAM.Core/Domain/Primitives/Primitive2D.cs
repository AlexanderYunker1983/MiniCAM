using System;
using MiniCAM.Core.Domain.Geometry;

namespace MiniCAM.Core.Domain.Primitives;

/// <summary>
/// Base class for all 2D primitives.
/// </summary>
public abstract class Primitive2D
{
    /// <summary>
    /// Gets the unique identifier of the primitive.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the name of the primitive.
    /// </summary>
    public string Name { get; set; }

    protected Primitive2D(string name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    /// <summary>
    /// Gets the bounding box of the primitive.
    /// </summary>
    public abstract Rect2D GetBounds();

    /// <summary>
    /// Transforms the primitive using the specified transformation.
    /// </summary>
    /// <param name="transform">The transformation to apply.</param>
    /// <returns>A new transformed primitive.</returns>
    public abstract Primitive2D Transform(Transform2D transform);

    /// <summary>
    /// Creates a deep copy of the primitive.
    /// </summary>
    /// <returns>A new primitive with the same properties.</returns>
    public abstract Primitive2D Clone();
}
