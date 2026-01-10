using System;
using System.Collections.ObjectModel;
using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MiniCAM.Core.ViewModels.Main;

/// <summary>
/// ViewModel for 2D preview control.
/// Manages viewport transformation (pan, zoom) and operations display.
/// </summary>
public partial class Preview2DViewModel : ViewModelBase
{
    // Viewport transformation
    [ObservableProperty]
    private double _panX = 0; // Pan offset in world coordinates (mm)

    [ObservableProperty]
    private double _panY = 0; // Pan offset in world coordinates (mm)

    [ObservableProperty]
    private double _zoom = 1.0; // Zoom factor (1.0 = 100%)

    // Viewport size in pixels (updated from view)
    [ObservableProperty]
    private double _viewportWidth = 800;

    [ObservableProperty]
    private double _viewportHeight = 600;

    // Operations to display
    public ObservableCollection<OperationItem> Operations { get; } = new();

    // Default visible area: 50x50 mm with small margin
    private const double DefaultVisibleWidth = 55.0; // 50mm + 5mm margin
    private const double DefaultVisibleHeight = 55.0; // 50mm + 5mm margin

    public Preview2DViewModel()
    {
        // Initialize viewport to show default area centered at origin
        ResetView();
    }

    /// <summary>
    /// Resets the view to show default 50x50mm area centered at origin.
    /// </summary>
    public void ResetView()
    {
        PanX = 0;
        PanY = 0;
        // Calculate zoom to fit 55x55mm area in viewport
        // Use the smaller dimension to ensure everything fits
        var scaleX = ViewportWidth / DefaultVisibleWidth;
        var scaleY = ViewportHeight / DefaultVisibleHeight;
        Zoom = Math.Min(scaleX, scaleY);
    }

    /// <summary>
    /// Updates viewport size and adjusts zoom to maintain visible area.
    /// </summary>
    public void UpdateViewportSize(double width, double height)
    {
        if (width <= 0 || height <= 0) return;

        var oldZoom = Zoom;
        ViewportWidth = width;
        ViewportHeight = height;

        // Adjust zoom to maintain the same visible area
        var scaleX = width / DefaultVisibleWidth;
        var scaleY = height / DefaultVisibleHeight;
        var newZoom = Math.Min(scaleX, scaleY);

        // Only auto-adjust if zoom was at default (user hasn't manually zoomed)
        if (Math.Abs(oldZoom - (ViewportWidth / DefaultVisibleWidth)) < 0.01 ||
            Math.Abs(oldZoom - (ViewportHeight / DefaultVisibleHeight)) < 0.01)
        {
            Zoom = newZoom;
        }
    }

    /// <summary>
    /// Converts screen coordinates to world coordinates (mm).
    /// Note: Screen Y increases downward, world Y increases upward.
    /// </summary>
    public Point ScreenToWorld(Point screenPoint)
    {
        var centerX = ViewportWidth / 2;
        var centerY = ViewportHeight / 2;
        var worldX = ((screenPoint.X - centerX) / Zoom) - PanX;
        var worldY = -((screenPoint.Y - centerY) / Zoom) - PanY; // Flip Y axis
        return new Point(worldX, worldY);
    }

    /// <summary>
    /// Converts world coordinates (mm) to screen coordinates.
    /// Note: Screen Y increases downward, world Y increases upward.
    /// </summary>
    public Point WorldToScreen(Point worldPoint)
    {
        var centerX = ViewportWidth / 2;
        var centerY = ViewportHeight / 2;
        var screenX = (worldPoint.X + PanX) * Zoom + centerX;
        var screenY = -(worldPoint.Y + PanY) * Zoom + centerY; // Flip Y axis
        return new Point(screenX, screenY);
    }

    /// <summary>
    /// Gets the visible world bounds in mm.
    /// </summary>
    public Rect GetVisibleWorldBounds()
    {
        // Convert all four corners to world coordinates
        var topLeft = ScreenToWorld(new Point(0, 0));
        var topRight = ScreenToWorld(new Point(ViewportWidth, 0));
        var bottomLeft = ScreenToWorld(new Point(0, ViewportHeight));
        var bottomRight = ScreenToWorld(new Point(ViewportWidth, ViewportHeight));
        
        // Find min/max coordinates (accounting for flipped Y axis)
        var minX = Math.Min(Math.Min(topLeft.X, topRight.X), Math.Min(bottomLeft.X, bottomRight.X));
        var maxX = Math.Max(Math.Max(topLeft.X, topRight.X), Math.Max(bottomLeft.X, bottomRight.X));
        var minY = Math.Min(Math.Min(topLeft.Y, topRight.Y), Math.Min(bottomLeft.Y, bottomRight.Y));
        var maxY = Math.Max(Math.Max(topLeft.Y, topRight.Y), Math.Max(bottomLeft.Y, bottomRight.Y));
        
        return new Rect(minX, minY, maxX - minX, maxY - minY);
    }
}
