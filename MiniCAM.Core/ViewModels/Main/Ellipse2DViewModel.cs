using System;
using MiniCAM.Core.Domain.Primitives;
using MiniCAM.Core.Localization;

namespace MiniCAM.Core.ViewModels.Main;

/// <summary>
/// ViewModel for a 2D ellipse primitive.
/// </summary>
public partial class Ellipse2DViewModel : Primitive2DItemViewModel
{
    private readonly Ellipse2DPrimitive _ellipse;

    public Ellipse2DViewModel(Ellipse2DPrimitive ellipse)
        : base(ellipse)
    {
        _ellipse = ellipse;
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
    /// Gets or sets the X coordinate of the center point.
    /// </summary>
    public double CenterX
    {
        get => _ellipse.Center.X;
        set
        {
            if (Math.Abs(_ellipse.Center.X - value) > double.Epsilon)
            {
                _ellipse.Center = new Domain.Geometry.Point2D(value, _ellipse.Center.Y);
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayText));
                UpdateChildren();
            }
        }
    }

    /// <summary>
    /// Gets or sets the Y coordinate of the center point.
    /// </summary>
    public double CenterY
    {
        get => _ellipse.Center.Y;
        set
        {
            if (Math.Abs(_ellipse.Center.Y - value) > double.Epsilon)
            {
                _ellipse.Center = new Domain.Geometry.Point2D(_ellipse.Center.X, value);
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayText));
                UpdateChildren();
            }
        }
    }

    /// <summary>
    /// Gets or sets the radius along the X axis.
    /// </summary>
    public double RadiusX
    {
        get => _ellipse.RadiusX;
        set
        {
            if (Math.Abs(_ellipse.RadiusX - value) > double.Epsilon)
            {
                _ellipse.RadiusX = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayText));
                UpdateChildren();
            }
        }
    }

    /// <summary>
    /// Gets or sets the radius along the Y axis.
    /// </summary>
    public double RadiusY
    {
        get => _ellipse.RadiusY;
        set
        {
            if (Math.Abs(_ellipse.RadiusY - value) > double.Epsilon)
            {
                _ellipse.RadiusY = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayText));
                UpdateChildren();
            }
        }
    }

    /// <summary>
    /// Gets or sets the rotation angle in degrees.
    /// </summary>
    public double RotationAngleDegrees
    {
        get => _ellipse.RotationAngle * 180.0 / Math.PI;
        set
        {
            var radians = value * Math.PI / 180.0;
            if (Math.Abs(_ellipse.RotationAngle - radians) > 0.001)
            {
                _ellipse.RotationAngle = radians;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayText));
                UpdateChildren();
            }
        }
    }

    /// <summary>
    /// Gets the display text for the ellipse properties.
    /// </summary>
    public string DisplayText => $"Center: ({CenterX:F3}, {CenterY:F3}), Rx: {RadiusX:F3}, Ry: {RadiusY:F3}, Angle: {RotationAngleDegrees:F1}°";

    private void UpdateChildren()
    {
        Children.Clear();
        Children.Add(new Primitive2DChildItem($"{Resources.PrimitivePropertyCenter}: ({CenterX:F3}, {CenterY:F3})"));
        Children.Add(new Primitive2DChildItem($"{Resources.PrimitivePropertyRadiusX}: {RadiusX:F3}"));
        Children.Add(new Primitive2DChildItem($"{Resources.PrimitivePropertyRadiusY}: {RadiusY:F3}"));
        Children.Add(new Primitive2DChildItem($"{Resources.PrimitivePropertyRotation}: {RotationAngleDegrees:F1}°"));
    }
}
