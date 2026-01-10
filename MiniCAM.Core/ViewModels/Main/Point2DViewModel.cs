using System;
using MiniCAM.Core.Domain.Primitives;

namespace MiniCAM.Core.ViewModels.Main;

/// <summary>
/// ViewModel for a 2D point primitive.
/// </summary>
public partial class Point2DViewModel : Primitive2DItemViewModel
{
    private readonly Point2DPrimitive _point;

    public Point2DViewModel(Point2DPrimitive point)
        : base(point)
    {
        _point = point;
        
        // Initialize child items for tree view
        UpdateChildren();
    }

    /// <summary>
    /// Updates the children collection with localized property names.
    /// Called when culture changes.
    /// </summary>
    public void UpdateLocalizedChildren()
    {
        UpdateChildren();
    }

    /// <summary>
    /// Gets or sets the X coordinate.
    /// </summary>
    public double X
    {
        get => _point.X;
        set
        {
            if (Math.Abs(_point.X - value) > double.Epsilon)
            {
                _point.X = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayText));
                UpdateChildren();
            }
        }
    }

    /// <summary>
    /// Gets or sets the Y coordinate.
    /// </summary>
    public double Y
    {
        get => _point.Y;
        set
        {
            if (Math.Abs(_point.Y - value) > double.Epsilon)
            {
                _point.Y = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayText));
                UpdateChildren();
            }
        }
    }

    /// <summary>
    /// Gets the display text for the point coordinates.
    /// </summary>
    public string DisplayText => $"X: {X:F3}, Y: {Y:F3}";

    private void UpdateChildren()
    {
        Children.Clear();
        Children.Add(new Primitive2DChildItem($"X: {X:F3}"));
        Children.Add(new Primitive2DChildItem($"Y: {Y:F3}"));
    }
}
