using System;
using MiniCAM.Core.Domain.Primitives;
using MiniCAM.Core.Localization;

namespace MiniCAM.Core.ViewModels.Main;

/// <summary>
/// ViewModel for a 2D line primitive.
/// </summary>
public partial class Line2DViewModel : Primitive2DItemViewModel
{
    private readonly Line2DPrimitive _line;

    public Line2DViewModel(Line2DPrimitive line)
        : base(line)
    {
        _line = line;
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
    /// Gets or sets the X coordinate of the start point.
    /// </summary>
    public double StartX
    {
        get => _line.Start.X;
        set
        {
            if (Math.Abs(_line.Start.X - value) > double.Epsilon)
            {
                _line.Start = new Domain.Geometry.Point2D(value, _line.Start.Y);
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayText));
                UpdateChildren();
            }
        }
    }

    /// <summary>
    /// Gets or sets the Y coordinate of the start point.
    /// </summary>
    public double StartY
    {
        get => _line.Start.Y;
        set
        {
            if (Math.Abs(_line.Start.Y - value) > double.Epsilon)
            {
                _line.Start = new Domain.Geometry.Point2D(_line.Start.X, value);
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayText));
                UpdateChildren();
            }
        }
    }

    /// <summary>
    /// Gets or sets the X coordinate of the end point.
    /// </summary>
    public double EndX
    {
        get => _line.End.X;
        set
        {
            if (Math.Abs(_line.End.X - value) > double.Epsilon)
            {
                _line.End = new Domain.Geometry.Point2D(value, _line.End.Y);
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayText));
                UpdateChildren();
            }
        }
    }

    /// <summary>
    /// Gets or sets the Y coordinate of the end point.
    /// </summary>
    public double EndY
    {
        get => _line.End.Y;
        set
        {
            if (Math.Abs(_line.End.Y - value) > double.Epsilon)
            {
                _line.End = new Domain.Geometry.Point2D(_line.End.X, value);
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayText));
                UpdateChildren();
            }
        }
    }

    /// <summary>
    /// Gets the display text for the line coordinates.
    /// </summary>
    public string DisplayText => $"({StartX:F3}, {StartY:F3}) - ({EndX:F3}, {EndY:F3})";

    /// <summary>
    /// Gets the length of the line.
    /// </summary>
    public double Length => _line.Length;

    private void UpdateChildren()
    {
        Children.Clear();
        Children.Add(new Primitive2DChildItem($"{Resources.PrimitivePropertyStart}: ({StartX:F3}, {StartY:F3})"));
        Children.Add(new Primitive2DChildItem($"{Resources.PrimitivePropertyEnd}: ({EndX:F3}, {EndY:F3})"));
        Children.Add(new Primitive2DChildItem($"{Resources.PrimitivePropertyLength}: {Length:F3}"));
    }
}
