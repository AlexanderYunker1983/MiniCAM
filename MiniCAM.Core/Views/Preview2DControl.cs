using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Input;
using MiniCAM.Core.Domain.Primitives;
using MiniCAM.Core.ViewModels.Main;
using MainViewModel = MiniCAM.Core.ViewModels.Main.MainViewModel;

namespace MiniCAM.Core.Views;

/// <summary>
/// Custom control for rendering 2D preview with grid, axes, and operations.
/// </summary>
public class Preview2DControl : Control
{
    private Preview2DViewModel? _viewModel;
    private Point _lastPanPoint;
    private bool _isPanning = false;
    private MainViewModel? _mainViewModel;
    
    // Object snap info for drawing snap lines
    private Point2DPrimitive? _snappedXPoint;
    private Point2DPrimitive? _snappedYPoint;
    // Coordinates of snap points (for lines and ellipses, these may not be Point2DPrimitive)
    private Point? _snappedXPointCoords;
    private Point? _snappedYPointCoords;
    private Point _snappedWorldPos;
    
    // Point under cursor for hover effect (when not in point mode)
    private Point2DPrimitive? _hoveredPoint;
    
    // Primitive under cursor for hover effect (lines and ellipses)
    private Primitive2D? _hoveredPrimitive;
    
    // Point moving mode
    private bool _isMovingPoint = false;
    private Point2DPrimitive? _movingPoint;
    
    // Line handle moving mode
    private bool _isMovingLineHandle = false;
    private Line2DPrimitive? _movingLine;
    private bool _isMovingStartHandle = false; // true for start, false for end
    
    // Ellipse handle moving mode
    private bool _isMovingEllipseHandle = false;
    private Ellipse2DPrimitive? _movingEllipse;
    private int _ellipseHandleType = 0; // 0 = center, 1 = major axis, 2 = minor axis
    
    // Current mouse position in world coordinates (for line preview)
    private Point _currentMouseWorldPos;

    public static readonly StyledProperty<Preview2DViewModel?> ViewModelProperty =
        AvaloniaProperty.Register<Preview2DControl, Preview2DViewModel?>(nameof(ViewModel));

    public Preview2DViewModel? ViewModel
    {
        get => GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    public MainViewModel? MainViewModel
    {
        get => _mainViewModel;
        set
        {
            // Unsubscribe from old collection
            if (_mainViewModel?.Primitives2DViewModel?.Primitives is INotifyCollectionChanged oldCollection)
            {
                oldCollection.CollectionChanged -= Primitives_CollectionChanged;
            }
            
            // Unsubscribe from PropertyChanged events for all old primitives
            if (_mainViewModel?.Primitives2DViewModel?.Primitives != null)
            {
                foreach (var item in _mainViewModel.Primitives2DViewModel.Primitives)
                {
                    if (item is System.ComponentModel.INotifyPropertyChanged notifyItem)
                    {
                        notifyItem.PropertyChanged -= PrimitiveViewModel_PropertyChanged;
                    }
                }
            }
            
            // Unsubscribe from old Primitives2DViewModel property changes
            if (_mainViewModel?.Primitives2DViewModel != null)
            {
                _mainViewModel.Primitives2DViewModel.PropertyChanged -= Primitives2DViewModel_PropertyChanged;
            }
            
            _mainViewModel = value;
            
            // Subscribe to new collection
            if (_mainViewModel?.Primitives2DViewModel?.Primitives is INotifyCollectionChanged newCollection)
            {
                newCollection.CollectionChanged += Primitives_CollectionChanged;
            }
            
            // Subscribe to PropertyChanged events for all existing primitives
            if (_mainViewModel?.Primitives2DViewModel?.Primitives != null)
            {
                foreach (var item in _mainViewModel.Primitives2DViewModel.Primitives)
                {
                    if (item is System.ComponentModel.INotifyPropertyChanged notifyItem)
                    {
                        notifyItem.PropertyChanged += PrimitiveViewModel_PropertyChanged;
                    }
                }
            }
            
            // Subscribe to Primitives2DViewModel property changes (e.g., SelectedPrimitive)
            if (_mainViewModel?.Primitives2DViewModel != null)
            {
                _mainViewModel.Primitives2DViewModel.PropertyChanged += Primitives2DViewModel_PropertyChanged;
            }
            
            InvalidateVisual();
        }
    }

    public void SetObjectSnapInfo(Point2DPrimitive? snappedXPoint, Point2DPrimitive? snappedYPoint, Point snappedWorldPos)
    {
        _snappedXPoint = snappedXPoint;
        _snappedYPoint = snappedYPoint;
        _snappedWorldPos = snappedWorldPos;
        // Don't call InvalidateVisual() here - let the caller do it after all updates
    }

    /// <summary>
    /// Sets object snap info with coordinates (for line endpoints and ellipse centers).
    /// </summary>
    public void SetObjectSnapInfoWithCoords(Point? snappedXPointCoords, Point? snappedYPointCoords, Point snappedWorldPos)
    {
        _snappedXPointCoords = snappedXPointCoords;
        _snappedYPointCoords = snappedYPointCoords;
        _snappedWorldPos = snappedWorldPos;
    }

    private void Primitives_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // Subscribe to PropertyChanged events for new items to enable real-time redraw
        if (e.NewItems != null)
        {
            foreach (var item in e.NewItems)
            {
                if (item is System.ComponentModel.INotifyPropertyChanged notifyItem)
                {
                    notifyItem.PropertyChanged += PrimitiveViewModel_PropertyChanged;
                }
            }
        }
        
        // Unsubscribe from PropertyChanged events for removed items
        if (e.OldItems != null)
        {
            foreach (var item in e.OldItems)
            {
                if (item is System.ComponentModel.INotifyPropertyChanged notifyItem)
                {
                    notifyItem.PropertyChanged -= PrimitiveViewModel_PropertyChanged;
                }
            }
        }
        
        // Also subscribe to all existing items if this is a reset
        if (e.Action == NotifyCollectionChangedAction.Reset && _mainViewModel?.Primitives2DViewModel?.Primitives != null)
        {
            foreach (var item in _mainViewModel.Primitives2DViewModel.Primitives)
            {
                if (item is System.ComponentModel.INotifyPropertyChanged notifyItem)
                {
                    notifyItem.PropertyChanged -= PrimitiveViewModel_PropertyChanged; // Remove old subscription first
                    notifyItem.PropertyChanged += PrimitiveViewModel_PropertyChanged;
                }
            }
        }
        
        InvalidateVisual();
    }
    
    private void PrimitiveViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        // Redraw when any property of a primitive changes (coordinates, radii, rotation, etc.)
        InvalidateVisual();
    }

    private void Primitives2DViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        // Redraw when SelectedPrimitive changes to update point colors
        if (e.PropertyName == nameof(Primitives2DViewModel.SelectedPrimitive))
        {
            InvalidateVisual();
        }
    }

    public Preview2DControl()
    {
        ClipToBounds = true;
        Focusable = true;
        
        // Subscribe to theme changes
        if (Application.Current != null)
        {
            Application.Current.PropertyChanged += Application_PropertyChanged;
        }
    }

    private void Application_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        // When theme changes, invalidate visual to redraw with new colors
        if (e.Property == Application.RequestedThemeVariantProperty || 
            e.Property == Application.ActualThemeVariantProperty)
        {
            InvalidateVisual();
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        
        // Unsubscribe from theme changes
        if (Application.Current != null)
        {
            Application.Current.PropertyChanged -= Application_PropertyChanged;
        }
        
        // Unsubscribe from primitives collection changes
        if (_mainViewModel?.Primitives2DViewModel?.Primitives is INotifyCollectionChanged collection)
        {
            collection.CollectionChanged -= Primitives_CollectionChanged;
        }
        
        // Unsubscribe from PropertyChanged events for all primitives
        if (_mainViewModel?.Primitives2DViewModel?.Primitives != null)
        {
            foreach (var item in _mainViewModel.Primitives2DViewModel.Primitives)
            {
                if (item is System.ComponentModel.INotifyPropertyChanged notifyItem)
                {
                    notifyItem.PropertyChanged -= PrimitiveViewModel_PropertyChanged;
                }
            }
        }
        
        // Unsubscribe from Primitives2DViewModel property changes
        if (_mainViewModel?.Primitives2DViewModel != null)
        {
            _mainViewModel.Primitives2DViewModel.PropertyChanged -= Primitives2DViewModel_PropertyChanged;
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ViewModelProperty)
        {
            if (_viewModel != null)
            {
                _viewModel.PropertyChanged -= ViewModel_PropertyChanged;
            }

            _viewModel = change.NewValue as Preview2DViewModel;

            if (_viewModel != null)
            {
                _viewModel.PropertyChanged += ViewModel_PropertyChanged;
            }

            InvalidateVisual();
        }
        else if (change.Property == BoundsProperty)
        {
            if (_viewModel != null && change.NewValue is Rect newBounds)
            {
                _viewModel.UpdateViewportSize(newBounds.Width, newBounds.Height);
                InvalidateVisual();
            }
        }
    }

    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Preview2DViewModel.PanX) ||
            e.PropertyName == nameof(Preview2DViewModel.PanY) ||
            e.PropertyName == nameof(Preview2DViewModel.Zoom) ||
            e.PropertyName == nameof(Preview2DViewModel.ViewportWidth) ||
            e.PropertyName == nameof(Preview2DViewModel.ViewportHeight))
        {
            InvalidateVisual();
        }
    }

    public override void Render(DrawingContext context)
    {
        if (_viewModel == null) return;

        var bounds = Bounds;
        if (bounds.Width <= 0 || bounds.Height <= 0) return;

        // Update viewport size if changed
        if (Math.Abs(_viewModel.ViewportWidth - bounds.Width) > 0.1 ||
            Math.Abs(_viewModel.ViewportHeight - bounds.Height) > 0.1)
        {
            _viewModel.UpdateViewportSize(bounds.Width, bounds.Height);
        }

        // Draw background using theme resource (get fresh resource each time to respect theme changes)
        IBrush? backgroundBrush = null;
        if (Application.Current != null)
        {
            var themeVariant = Application.Current.ActualThemeVariant;
            if (Application.Current.TryFindResource("Preview2DBackgroundBrush", themeVariant, out var bgRes) && bgRes is IBrush bgBrush)
            {
                backgroundBrush = bgBrush;
            }
        }
        context.FillRectangle(backgroundBrush ?? Brushes.White, bounds);

        // Transform to world coordinates
        // Note: In screen coordinates, Y increases downward, but in world coordinates (CAD), Y increases upward
        // So we need to flip Y axis
        // Center of viewport is at (bounds.Width/2, bounds.Height/2)
        // PanX and PanY are offsets in world coordinates from center
        var centerX = bounds.Width / 2;
        var centerY = bounds.Height / 2;
        var transform = new Matrix(_viewModel.Zoom, 0, 0, -_viewModel.Zoom,
            centerX + _viewModel.PanX * _viewModel.Zoom,
            centerY - _viewModel.PanY * _viewModel.Zoom);

        using (context.PushTransform(transform))
        {
            // Draw grid
            DrawGrid(context, bounds, _viewModel);

            // Draw primitives (lines, ellipses) - these scale with zoom
            DrawPrimitivesInWorld(context, bounds, _viewModel);

            // Draw temporary line preview when creating line
            DrawLinePreview(context, bounds, _viewModel);
            
            // Draw temporary ellipse preview when creating ellipse
            DrawEllipsePreview(context, bounds, _viewModel);

            // Draw operations (will be implemented later)
            // DrawOperations(context, bounds, _viewModel);
        }

        // Draw axes outside of transformation so they maintain constant pixel size
        DrawAxes(context, bounds, _viewModel);
        
        // Draw points outside of transformation so they maintain constant pixel size
        DrawPoints(context, bounds, _viewModel);
        
        // Draw line handles (for selected lines) outside of transformation so they maintain constant pixel size
        DrawLineHandles(context, bounds, _viewModel);
        
        // Draw ellipse handles (for selected ellipses) outside of transformation so they maintain constant pixel size
        DrawEllipseHandles(context, bounds, _viewModel);
        
        // Draw object snap lines if active
        DrawObjectSnapLines(context, bounds, _viewModel);
        
        // Draw cursor cross in point mode
        DrawCursorCross(context, bounds, _viewModel);
    }

    private void DrawGrid(DrawingContext context, Rect bounds, Preview2DViewModel vm)
    {
        var visibleBounds = vm.GetVisibleWorldBounds();
        
        // Grid spacing: 10mm for major lines, 1mm for minor lines
        const double majorGridSpacing = 10.0;
        const double minorGridSpacing = 1.0;

        // Use very thin lines in world coordinates (they will be scaled by zoom)
        // Thickness in mm: 0.05mm for minor, 0.1mm for major
        // Get brushes from theme resources (get fresh resources each time to respect theme changes)
        IBrush? minorGridBrush = null;
        IBrush? majorGridBrush = null;
        
        if (Application.Current != null)
        {
            var themeVariant = Application.Current.ActualThemeVariant;
            
            if (Application.Current.TryFindResource("Preview2DMinorGridBrush", themeVariant, out var minorRes) && minorRes is IBrush minorBrush)
            {
                minorGridBrush = minorBrush;
            }
            
            if (Application.Current.TryFindResource("Preview2DMajorGridBrush", themeVariant, out var majorRes) && majorRes is IBrush majorBrush)
            {
                majorGridBrush = majorBrush;
            }
        }
        
        var gridBrush = minorGridBrush ?? new SolidColorBrush(Color.FromRgb(220, 220, 220));
        var majorGridBrushFinal = majorGridBrush ?? new SolidColorBrush(Color.FromRgb(200, 200, 200));

        // Calculate grid line positions
        var startX = Math.Floor(visibleBounds.X / minorGridSpacing) * minorGridSpacing;
        var endX = Math.Ceiling(visibleBounds.Right / minorGridSpacing) * minorGridSpacing;
        var startY = Math.Floor(visibleBounds.Y / minorGridSpacing) * minorGridSpacing;
        var endY = Math.Ceiling(visibleBounds.Bottom / minorGridSpacing) * minorGridSpacing;

        // Constant thickness in pixels (same mechanism as axes)
        // Since grid is drawn inside transform, we need to divide by zoom to compensate for scaling
        const double minorThicknessPixels = 0.5; // pixels (constant, like axes)
        const double majorThicknessPixels = 1.0; // pixels (constant, like axes)
        
        // Compensate for transform scaling: divide by zoom so thickness stays constant in screen pixels
        var minorThickness = minorThicknessPixels / vm.Zoom;
        var majorThickness = majorThicknessPixels / vm.Zoom;

        // Grid collapsing: hide minor grid if spacing becomes less than 4 pixels
        // Distance between minor grid lines in world coordinates = minorGridSpacing (1mm)
        // In screen pixels = minorGridSpacing * zoom
        const double minMinorGridSpacingPixels = 4.0;
        var minorGridSpacingPixels = minorGridSpacing * vm.Zoom;
        var showMinorGrid = minorGridSpacingPixels >= minMinorGridSpacingPixels;

        // Draw minor grid lines only if spacing is sufficient
        if (showMinorGrid)
        {
            var minorPen = new Pen(gridBrush, minorThickness);

            // Vertical minor lines
            for (double x = startX; x <= endX; x += minorGridSpacing)
            {
                // Skip major grid lines
                var remainder = Math.Abs(x % majorGridSpacing);
                if (remainder < 0.001 || remainder > majorGridSpacing - 0.001) continue;
                
                context.DrawLine(minorPen, new Point(x, visibleBounds.Y), new Point(x, visibleBounds.Bottom));
            }

            // Horizontal minor lines
            for (double y = startY; y <= endY; y += minorGridSpacing)
            {
                // Skip major grid lines
                var remainder = Math.Abs(y % majorGridSpacing);
                if (remainder < 0.001 || remainder > majorGridSpacing - 0.001) continue;
                
                context.DrawLine(minorPen, new Point(visibleBounds.X, y), new Point(visibleBounds.Right, y));
            }
        }

        // Draw major grid lines with slightly thicker pen (constant pixel thickness, like axes)
        var majorPen = new Pen(majorGridBrushFinal, majorThickness);
        var majorStartX = Math.Floor(visibleBounds.X / majorGridSpacing) * majorGridSpacing;
        var majorEndX = Math.Ceiling(visibleBounds.Right / majorGridSpacing) * majorGridSpacing;
        var majorStartY = Math.Floor(visibleBounds.Y / majorGridSpacing) * majorGridSpacing;
        var majorEndY = Math.Ceiling(visibleBounds.Bottom / majorGridSpacing) * majorGridSpacing;

        // Vertical major lines
        for (double x = majorStartX; x <= majorEndX; x += majorGridSpacing)
        {
            context.DrawLine(majorPen, new Point(x, visibleBounds.Y), new Point(x, visibleBounds.Bottom));
        }

        // Horizontal major lines
        for (double y = majorStartY; y <= majorEndY; y += majorGridSpacing)
        {
            context.DrawLine(majorPen, new Point(visibleBounds.X, y), new Point(visibleBounds.Right, y));
        }
    }

    private void DrawAxes(DrawingContext context, Rect bounds, Preview2DViewModel vm)
    {
        // Draw axes in screen coordinates to maintain constant pixel size
        // Convert world coordinates (0,0) to screen coordinates
        var originScreen = vm.WorldToScreen(new Point(0, 0));
        
        // Constant sizes in pixels (independent of zoom)
        const double axisLineThickness = 1.5; // pixels
        const double arrowSize = 8.0; // pixels
        const double originRadius = 3.0; // pixels
        
        // Get axis brush from theme resources (get fresh resource each time to respect theme changes)
        IBrush? axisBrush = null;
        if (Application.Current != null)
        {
            var themeVariant = Application.Current.ActualThemeVariant;
            if (Application.Current.TryFindResource("Preview2DAxisBrush", themeVariant, out var axisRes) && axisRes is IBrush axisBrushRes)
            {
                axisBrush = axisBrushRes;
            }
        }
        var axisBrushFinal = axisBrush ?? Brushes.Black;
        var axisPen = new Pen(axisBrushFinal, axisLineThickness);

        // Draw X axis (horizontal line at y=0 in world coordinates)
        var visibleBounds = vm.GetVisibleWorldBounds();
        if (visibleBounds.Y <= 0 && visibleBounds.Bottom >= 0)
        {
            // Convert world bounds to screen coordinates for X axis
            var leftPoint = vm.WorldToScreen(new Point(visibleBounds.X, 0));
            var rightPoint = vm.WorldToScreen(new Point(visibleBounds.Right, 0));
            
            // Draw horizontal line
            context.DrawLine(axisPen, leftPoint, rightPoint);
            
            // Draw arrow at the right end
            var arrowAngle = Math.Atan2(0, 1); // Right direction
            var arrowX1 = rightPoint.X - arrowSize * Math.Cos(arrowAngle - Math.PI / 6);
            var arrowY1 = rightPoint.Y - arrowSize * Math.Sin(arrowAngle - Math.PI / 6);
            var arrowX2 = rightPoint.X - arrowSize * Math.Cos(arrowAngle + Math.PI / 6);
            var arrowY2 = rightPoint.Y - arrowSize * Math.Sin(arrowAngle + Math.PI / 6);
            
            context.DrawLine(axisPen, rightPoint, new Point(arrowX1, arrowY1));
            context.DrawLine(axisPen, rightPoint, new Point(arrowX2, arrowY2));
        }

        // Draw Y axis (vertical line at x=0 in world coordinates)
        if (visibleBounds.X <= 0 && visibleBounds.Right >= 0)
        {
            // Convert world bounds to screen coordinates for Y axis
            var topPoint = vm.WorldToScreen(new Point(0, visibleBounds.Bottom)); // Note: Bottom is top in screen coords
            var bottomPoint = vm.WorldToScreen(new Point(0, visibleBounds.Y)); // Note: Y is bottom in screen coords
            
            // Draw vertical line
            context.DrawLine(axisPen, topPoint, bottomPoint);
            
            // Draw arrow at the top end (Y increases upward in world, but screen Y increases downward)
            var arrowAngle = Math.Atan2(-1, 0); // Up direction in screen coordinates
            var arrowX1 = topPoint.X - arrowSize * Math.Cos(arrowAngle - Math.PI / 6);
            var arrowY1 = topPoint.Y - arrowSize * Math.Sin(arrowAngle - Math.PI / 6);
            var arrowX2 = topPoint.X - arrowSize * Math.Cos(arrowAngle + Math.PI / 6);
            var arrowY2 = topPoint.Y - arrowSize * Math.Sin(arrowAngle + Math.PI / 6);
            
            context.DrawLine(axisPen, topPoint, new Point(arrowX1, arrowY1));
            context.DrawLine(axisPen, topPoint, new Point(arrowX2, arrowY2));
        }

        // Draw origin point
        if (visibleBounds.Contains(new Point(0, 0)))
        {
            context.DrawEllipse(axisBrushFinal, null, originScreen, originRadius, originRadius);
        }
    }

    private Point SnapToGrid(Point worldPoint, double snapStep)
    {
        var snappedX = Math.Round(worldPoint.X / snapStep) * snapStep;
        var snappedY = Math.Round(worldPoint.Y / snapStep) * snapStep;
        return new Point(snappedX, snappedY);
    }

    private bool ShouldSnapToGrid(bool isGridSnapEnabled, bool isShiftPressed)
    {
        // If button is enabled: snap is always on, but Shift temporarily disables it
        if (isGridSnapEnabled)
        {
            return !isShiftPressed;
        }
        // If button is disabled: Shift temporarily enables snap
        else
        {
            return isShiftPressed;
        }
    }

    private bool ShouldObjectSnap(bool isObjectSnapEnabled, bool isCtrlPressed)
    {
        // If button is enabled: snap is always on, but Ctrl temporarily disables it
        if (isObjectSnapEnabled)
        {
            return !isCtrlPressed;
        }
        // If button is disabled: Ctrl temporarily enables snap
        else
        {
            return isCtrlPressed;
        }
    }

    private Point2DPrimitive? FindPointUnderCursor(Point worldPoint, Point screenPoint, Preview2DViewModel vm)
    {
        if (_mainViewModel == null) return null;
        
        const double pointRadius = 3.0; // pixels - same as point radius
        const double hoverTolerance = pointRadius; // Consider point hovered if cursor is within point radius
        
        Point2DPrimitive? nearestPoint = null;
        double minDistanceScreen = hoverTolerance;
        
        // Get all snap points (from point primitives, line endpoints, ellipse centers)
        var snapPoints = GetAllSnapPoints(_mainViewModel);
        
        // Find nearest point within hover tolerance
        foreach (var (snapPoint, sourcePrimitive) in snapPoints)
        {
            // Only consider actual point primitives for hover (not line endpoints or ellipse centers)
            if (sourcePrimitive is Point2DPrimitive point)
            {
                // Convert point to screen coordinates
                var pointScreen = vm.WorldToScreen(new Point(snapPoint.X, snapPoint.Y));
                
                // Calculate distance in screen pixels
                var dx = pointScreen.X - screenPoint.X;
                var dy = pointScreen.Y - screenPoint.Y;
                var distanceScreen = Math.Sqrt(dx * dx + dy * dy);
                
                // Check if point is within hover tolerance
                if (distanceScreen <= hoverTolerance && distanceScreen < minDistanceScreen)
                {
                    minDistanceScreen = distanceScreen;
                    nearestPoint = point;
                }
            }
        }
        
        return nearestPoint;
    }

    /// <summary>
    /// Finds which line handle (start or end) is under the cursor, if any.
    /// Returns the line and a flag indicating if it's the start handle (true) or end handle (false).
    /// </summary>
    private (Line2DPrimitive line, bool isStartHandle)? FindLineHandleUnderCursor(Point worldPoint, Point screenPoint, Preview2DViewModel vm)
    {
        if (_mainViewModel == null) return null;
        
        const double handleRadius = 4.0; // pixels - same as handle radius
        const double hoverTolerance = handleRadius;
        
        var selectedPrimitive = _mainViewModel.Primitives2DViewModel.SelectedPrimitive;
        
        // Only check handles of selected lines
        if (selectedPrimitive?.Primitive is Line2DPrimitive selectedLine)
        {
            // Check start handle
            var startScreen = vm.WorldToScreen(new Point(selectedLine.Start.X, selectedLine.Start.Y));
            var dxStart = startScreen.X - screenPoint.X;
            var dyStart = startScreen.Y - screenPoint.Y;
            var distanceStart = Math.Sqrt(dxStart * dxStart + dyStart * dyStart);
            
            if (distanceStart <= hoverTolerance)
            {
                return (selectedLine, true);
            }
            
            // Check end handle
            var endScreen = vm.WorldToScreen(new Point(selectedLine.End.X, selectedLine.End.Y));
            var dxEnd = endScreen.X - screenPoint.X;
            var dyEnd = endScreen.Y - screenPoint.Y;
            var distanceEnd = Math.Sqrt(dxEnd * dxEnd + dyEnd * dyEnd);
            
            if (distanceEnd <= hoverTolerance)
            {
                return (selectedLine, false);
            }
        }
        
        return null;
    }

    /// <summary>
    /// Finds which ellipse handle (center, major axis, or minor axis) is under the cursor, if any.
    /// Returns the ellipse and handle type (0 = center, 1 = major axis, 2 = minor axis).
    /// </summary>
    private (Ellipse2DPrimitive ellipse, int handleType)? FindEllipseHandleUnderCursor(Point worldPoint, Point screenPoint, Preview2DViewModel vm)
    {
        if (_mainViewModel == null) return null;
        
        const double handleRadius = 4.0; // pixels - same as handle radius
        const double hoverTolerance = handleRadius;
        
        var selectedPrimitive = _mainViewModel.Primitives2DViewModel.SelectedPrimitive;
        
        // Only check handles of selected ellipses
        if (selectedPrimitive?.Primitive is Ellipse2DPrimitive selectedEllipse)
        {
            // Calculate the three handle points
            var center = selectedEllipse.Center;
            var cos = Math.Cos(selectedEllipse.RotationAngle);
            var sin = Math.Sin(selectedEllipse.RotationAngle);
            
            // Major axis point
            var majorAxisPoint = new Domain.Geometry.Point2D(
                center.X + selectedEllipse.RadiusX * cos,
                center.Y + selectedEllipse.RadiusX * sin);
            
            // Minor axis point
            var minorAxisPoint = new Domain.Geometry.Point2D(
                center.X - selectedEllipse.RadiusY * sin,
                center.Y + selectedEllipse.RadiusY * cos);
            
            // Convert to screen coordinates
            var centerScreen = vm.WorldToScreen(new Point(center.X, center.Y));
            var majorAxisScreen = vm.WorldToScreen(new Point(majorAxisPoint.X, majorAxisPoint.Y));
            var minorAxisScreen = vm.WorldToScreen(new Point(minorAxisPoint.X, minorAxisPoint.Y));
            
            // Check center handle
            var dxCenter = centerScreen.X - screenPoint.X;
            var dyCenter = centerScreen.Y - screenPoint.Y;
            var distanceCenter = Math.Sqrt(dxCenter * dxCenter + dyCenter * dyCenter);
            
            if (distanceCenter <= hoverTolerance)
            {
                return (selectedEllipse, 0); // center
            }
            
            // Check major axis handle
            var dxMajor = majorAxisScreen.X - screenPoint.X;
            var dyMajor = majorAxisScreen.Y - screenPoint.Y;
            var distanceMajor = Math.Sqrt(dxMajor * dxMajor + dyMajor * dyMajor);
            
            if (distanceMajor <= hoverTolerance)
            {
                return (selectedEllipse, 1); // major axis
            }
            
            // Check minor axis handle
            var dxMinor = minorAxisScreen.X - screenPoint.X;
            var dyMinor = minorAxisScreen.Y - screenPoint.Y;
            var distanceMinor = Math.Sqrt(dxMinor * dxMinor + dyMinor * dyMinor);
            
            if (distanceMinor <= hoverTolerance)
            {
                return (selectedEllipse, 2); // minor axis
            }
        }
        
        return null;
    }

    /// <summary>
    /// Gets all snap points from all primitives (point primitives, line endpoints, ellipse centers).
    /// </summary>
    private List<(Domain.Geometry.Point2D Point, Primitive2D? SourcePrimitive)> GetAllSnapPoints(MainViewModel mainViewModel, Point2DPrimitive? excludePoint = null)
    {
        var snapPoints = new List<(Domain.Geometry.Point2D, Primitive2D?)>();
        
        foreach (var primitiveViewModel in mainViewModel.Primitives2DViewModel.Primitives)
        {
            var primitive = primitiveViewModel.Primitive;
            
            if (primitive is Point2DPrimitive point)
            {
                if (point != excludePoint)
                {
                    snapPoints.Add((new Domain.Geometry.Point2D(point.X, point.Y), primitive));
                }
            }
            else if (primitive is Line2DPrimitive line)
            {
                // Add start and end points of the line
                snapPoints.Add((line.Start, primitive));
                snapPoints.Add((line.End, primitive));
            }
            else if (primitive is Ellipse2DPrimitive ellipse)
            {
                // Add center point of the ellipse
                snapPoints.Add((ellipse.Center, primitive));
            }
        }
        
        return snapPoints;
    }

    private (Point snappedPoint, Point2DPrimitive? snappedXPoint, Point2DPrimitive? snappedYPoint, Domain.Geometry.Point2D? snappedXPointCoords, Domain.Geometry.Point2D? snappedYPointCoords) SnapToObjectsExcluding(Point worldPoint, MainViewModel mainViewModel, Preview2DViewModel vm, Point2DPrimitive? excludePoint)
    {
        const double snapTolerance = 5.0; // pixels tolerance for snapping
        
        Domain.Geometry.Point2D? nearestXPoint = null;
        Domain.Geometry.Point2D? nearestYPoint = null;
        double minXDistanceScreen = snapTolerance;
        double minYDistanceScreen = snapTolerance;
        
        var worldPosScreen = vm.WorldToScreen(worldPoint);
        
        // Get all snap points (from point primitives, line endpoints, ellipse centers)
        var snapPoints = GetAllSnapPoints(mainViewModel, excludePoint);
        
        // Find nearest points on X and Y axes
        foreach (var (snapPoint, _) in snapPoints)
        {
            // Convert snap point to screen coordinates
            var pointScreen = vm.WorldToScreen(new Point(snapPoint.X, snapPoint.Y));
            
            // Check distance in screen pixels
            var distanceXScreen = Math.Abs(pointScreen.X - worldPosScreen.X);
            var distanceYScreen = Math.Abs(pointScreen.Y - worldPosScreen.Y);
            
            // Check if point is close enough on X axis (within 5 pixels)
            if (distanceXScreen <= snapTolerance && distanceXScreen < minXDistanceScreen)
            {
                minXDistanceScreen = distanceXScreen;
                nearestXPoint = snapPoint;
            }
            
            // Check if point is close enough on Y axis (within 5 pixels)
            if (distanceYScreen <= snapTolerance && distanceYScreen < minYDistanceScreen)
            {
                minYDistanceScreen = distanceYScreen;
                nearestYPoint = snapPoint;
            }
        }
        
        // Apply snapping
        var snappedX = nearestXPoint.HasValue ? nearestXPoint.Value.X : worldPoint.X;
        var snappedY = nearestYPoint.HasValue ? nearestYPoint.Value.Y : worldPoint.Y;
        
        // Convert back to Point2DPrimitive for compatibility (find the source primitive if it exists)
        Point2DPrimitive? snappedXPointPrimitive = null;
        Point2DPrimitive? snappedYPointPrimitive = null;
        
        if (nearestXPoint.HasValue)
        {
            // Try to find the source point primitive
            foreach (var (snapPoint, sourcePrimitive) in snapPoints)
            {
                if (snapPoint.X == nearestXPoint.Value.X && snapPoint.Y == nearestXPoint.Value.Y && sourcePrimitive is Point2DPrimitive pointPrim)
                {
                    snappedXPointPrimitive = pointPrim;
                    break;
                }
            }
        }
        
        if (nearestYPoint.HasValue)
        {
            // Try to find the source point primitive
            foreach (var (snapPoint, sourcePrimitive) in snapPoints)
            {
                if (snapPoint.X == nearestYPoint.Value.X && snapPoint.Y == nearestYPoint.Value.Y && sourcePrimitive is Point2DPrimitive pointPrim)
                {
                    snappedYPointPrimitive = pointPrim;
                    break;
                }
            }
        }
        
        return (new Point(snappedX, snappedY), snappedXPointPrimitive, snappedYPointPrimitive, nearestXPoint, nearestYPoint);
    }

    private (Point snappedPoint, Point2DPrimitive? snappedXPoint, Point2DPrimitive? snappedYPoint, Domain.Geometry.Point2D? snappedXPointCoords, Domain.Geometry.Point2D? snappedYPointCoords) SnapToObjects(Point worldPoint, MainViewModel mainViewModel, Preview2DViewModel vm)
    {
        return SnapToObjectsExcluding(worldPoint, mainViewModel, vm, null);
    }

    /// <summary>
    /// Finds a primitive (line or ellipse) under the cursor.
    /// </summary>
    private Primitive2D? FindPrimitiveUnderCursor(Point worldPoint, Point screenPoint, Preview2DViewModel vm)
    {
        if (_mainViewModel == null) return null;
        
        const double hoverTolerance = 5.0; // pixels tolerance for hover
        
        Primitive2D? nearestPrimitive = null;
        double minDistanceScreen = hoverTolerance;
        
        foreach (var primitiveViewModel in _mainViewModel.Primitives2DViewModel.Primitives)
        {
            var primitive = primitiveViewModel.Primitive;
            
            if (primitive is Line2DPrimitive line)
            {
                // Check distance from cursor to line segment
                var startScreen = vm.WorldToScreen(new Point(line.Start.X, line.Start.Y));
                var endScreen = vm.WorldToScreen(new Point(line.End.X, line.End.Y));
                
                // Calculate distance from point to line segment
                var distance = DistanceToLineSegment(screenPoint, startScreen, endScreen);
                
                if (distance <= hoverTolerance && distance < minDistanceScreen)
                {
                    minDistanceScreen = distance;
                    nearestPrimitive = line;
                }
            }
            else if (primitive is Ellipse2DPrimitive ellipse)
            {
                // Check distance from cursor to ellipse edge
                var centerScreen = vm.WorldToScreen(new Point(ellipse.Center.X, ellipse.Center.Y));
                
                // Convert ellipse radii to screen coordinates
                var radiusXScreen = ellipse.RadiusX * vm.Zoom;
                var radiusYScreen = ellipse.RadiusY * vm.Zoom;
                
                // Calculate distance from point to ellipse edge
                var dx = screenPoint.X - centerScreen.X;
                var dy = screenPoint.Y - centerScreen.Y;
                
                // For axis-aligned ellipse: (x/a)^2 + (y/b)^2 = 1
                // Distance from point to ellipse edge
                var normalizedX = radiusXScreen > 0 ? dx / radiusXScreen : 0;
                var normalizedY = radiusYScreen > 0 ? dy / radiusYScreen : 0;
                var distanceFromCenter = Math.Sqrt(normalizedX * normalizedX + normalizedY * normalizedY);
                
                // Distance to ellipse edge
                var distance = Math.Abs(distanceFromCenter - 1.0) * Math.Min(radiusXScreen, radiusYScreen);
                
                if (distance <= hoverTolerance && distance < minDistanceScreen)
                {
                    minDistanceScreen = distance;
                    nearestPrimitive = ellipse;
                }
            }
        }
        
        return nearestPrimitive;
    }

    /// <summary>
    /// Calculates the distance from a point to a line segment.
    /// </summary>
    private double DistanceToLineSegment(Point point, Point lineStart, Point lineEnd)
    {
        var A = point.X - lineStart.X;
        var B = point.Y - lineStart.Y;
        var C = lineEnd.X - lineStart.X;
        var D = lineEnd.Y - lineStart.Y;

        var dot = A * C + B * D;
        var lenSq = C * C + D * D;
        var param = lenSq != 0 ? dot / lenSq : -1;

        double xx, yy;

        if (param < 0)
        {
            xx = lineStart.X;
            yy = lineStart.Y;
        }
        else if (param > 1)
        {
            xx = lineEnd.X;
            yy = lineEnd.Y;
        }
        else
        {
            xx = lineStart.X + param * C;
            yy = lineStart.Y + param * D;
        }

        var dx = point.X - xx;
        var dy = point.Y - yy;
        return Math.Sqrt(dx * dx + dy * dy);
    }

    /// <summary>
    /// Draws primitives that scale with zoom (lines, ellipses) in world coordinates.
    /// </summary>
    private void DrawPrimitivesInWorld(DrawingContext context, Rect bounds, Preview2DViewModel vm)
    {
        if (_mainViewModel == null || vm == null) return;

        const double lineThickness = 0.1; // mm in world coordinates (will scale with zoom)
        var selectedPrimitive = _mainViewModel.Primitives2DViewModel.SelectedPrimitive;

        foreach (var primitiveViewModel in _mainViewModel.Primitives2DViewModel.Primitives)
        {
            var primitive = primitiveViewModel.Primitive;
            var isSelected = primitiveViewModel == selectedPrimitive;
            var isHovered = !_mainViewModel.IsPointModeActive && 
                           !_mainViewModel.IsLineModeActive && 
                           !_mainViewModel.IsEllipseModeActive &&
                           primitive == _hoveredPrimitive && 
                           !isSelected;
            
            IBrush? brush = null;
            IPen? pen = null;

            // Determine colors based on selection and hover state
            if (isSelected)
            {
                brush = new SolidColorBrush(Color.FromRgb(0, 255, 0)); // Green for selected
                pen = new Pen(brush, lineThickness);
            }
            else if (isHovered)
            {
                brush = new SolidColorBrush(Color.FromRgb(0, 255, 0)); // Green for hovered
                pen = new Pen(brush, lineThickness * 1.5); // Slightly thicker when hovered
            }
            else
            {
                brush = new SolidColorBrush(Color.FromRgb(0, 0, 255)); // Blue for normal
                pen = new Pen(brush, lineThickness);
            }

            // Draw line
            if (primitive is Line2DPrimitive line)
            {
                var startPoint = new Point(line.Start.X, line.Start.Y);
                var endPoint = new Point(line.End.X, line.End.Y);
                context.DrawLine(pen, startPoint, endPoint);
            }
            // Draw ellipse
            else if (primitive is Ellipse2DPrimitive ellipse)
            {
                var center = new Point(ellipse.Center.X, ellipse.Center.Y);
                
                // Create rectangle for ellipse (in local coordinates, centered at origin)
                var rect = new Rect(
                    -ellipse.RadiusX,
                    -ellipse.RadiusY,
                    ellipse.RadiusX * 2,
                    ellipse.RadiusY * 2);
                
                // Apply transformation: rotate around center point
                // We're inside a transform that inverts Y axis (m22 = -Zoom)
                // The ellipse rect is centered at (0, 0), so we need to:
                // 1. Translate to center (cx, cy) - this moves ellipse from (0,0) to center
                // 2. Rotate around center
                // Since parent transform inverts Y, we need to use standard rotation matrix
                // Standard rotation matrix: [cos -sin]
                //                          [sin  cos]
                var cos = Math.Cos(ellipse.RotationAngle);
                var sin = Math.Sin(ellipse.RotationAngle);
                var cx = center.X;
                var cy = center.Y;
                
                // For rotation around point (cx, cy):
                // The ellipse rect is at (0,0), we need to move it to (cx,cy) and rotate
                // The correct approach: draw ellipse at (0,0), then apply transformation
                // that moves it to (cx,cy) and rotates around (cx,cy)
                // Since we're in Y-inverted coordinates, we need to be careful
                // Let's use the standard formula for rotation around a point:
                // First translate to center, then rotate, then translate back
                // But since ellipse starts at (0,0), we simplify:
                // The transformation matrix for rotation around (cx,cy) when starting at (0,0):
                // We need to ensure that at θ=0, the ellipse is at (cx,cy)
                // So: offset = (cx, cy) when θ=0
                // And when rotating: we rotate around (cx,cy)
                // The correct formula: T(cx,cy) * R(θ) * T(-cx,-cy) applied to (0,0)
                // This gives: [cos -sin  cx*(1-cos) + cy*sin]
                //            [sin  cos  cy*(1-cos) - cx*sin]
                // But at θ=0: offset = (0,0), which is wrong!
                // The issue is that T(-cx,-cy) moves (0,0) to (-cx,-cy), then R rotates, then T(cx,cy) moves back
                // So at θ=0, (0,0) -> (-cx,-cy) -> (-cx,-cy) -> (0,0)
                // We need (0,0) -> (cx,cy) at θ=0
                // So we need: T(cx,cy) * R(θ) applied to (0,0), which gives offset = (cx,cy) always
                // But then rotation is around origin, not around (cx,cy)
                // The correct solution: use T(cx,cy) * R(θ) * T(-cx,-cy), but the offset formula is:
                // offsetX = cx*(1-cos) + cy*sin, but we need to add cx to move from (0,0) to center
                // Actually, the formula T(cx,cy) * R(θ) * T(-cx,-cy) applied to point p gives:
                // p' = (cx,cy) + R(θ) * (p - (cx,cy))
                // For p = (0,0): p' = (cx,cy) + R(θ) * (-cx,-cy)
                // = (cx,cy) + (cos*(-cx) - sin*(-cy), sin*(-cx) + cos*(-cy))
                // = (cx,cy) + (-cx*cos + cy*sin, -cx*sin - cy*cos)
                // = (cx - cx*cos + cy*sin, cy - cx*sin - cy*cos)
                // At θ=0: p' = (cx - cx + 0, cy - 0 - cy) = (0, 0) - WRONG!
                // The problem is that T(-cx,-cy) moves (0,0) to (-cx,-cy), which is wrong
                // We need to think differently: the ellipse is drawn at (0,0) in local coordinates
                // We want it to appear at (cx,cy) and rotated
                // So we need: T(cx,cy) * R(θ), which gives offset = (cx,cy) always
                // But then rotation is around origin
                // To rotate around (cx,cy), we need: T(cx,cy) * R(θ) * T(-cx,-cy)
                // But this doesn't work for (0,0) starting point
                // The solution: draw ellipse centered at (cx,cy) in local coords, then rotate
                // Or: use T(cx,cy) * R(θ) and adjust the rect to be centered at (-cx,-cy) in local coords
                // Actually, the simplest: just use T(cx,cy) * R(θ) with rect at (0,0)
                // This gives rotation around origin, but offset = (cx,cy)
                // To rotate around (cx,cy), we need to adjust: T(cx,cy) * R(θ) * T(-cx,-cy)
                // For point (0,0): T(-cx,-cy) moves it to (-cx,-cy)
                // Then R(θ) rotates around origin: (-cx*cos + cy*sin, -cx*sin - cy*cos)
                // Then T(cx,cy) moves it: (cx - cx*cos + cy*sin, cy - cx*sin - cy*cos)
                // At θ=0: (cx - cx + 0, cy - 0 - cy) = (0, 0)
                // So the formula is wrong for (0,0) starting point
                // The fix: we need to use T(cx,cy) * R(θ) instead, which gives:
                // For point (0,0): R(θ) * (0,0) = (0,0), then T(cx,cy) gives (cx,cy)
                // But rotation is around origin, not around (cx,cy)
                // To rotate around (cx,cy), we need to first move to origin, rotate, then move back
                // But for (0,0) starting point, T(-cx,-cy) moves it to (-cx,-cy)
                // So we need: T(cx,cy) * R(θ) * T(-cx,-cy) applied to (0,0)
                // = T(cx,cy) * R(θ) * (-cx,-cy)
                // = T(cx,cy) * (-cx*cos + cy*sin, -cx*sin - cy*cos)
                // = (cx - cx*cos + cy*sin, cy - cx*sin - cy*cos)
                // At θ=0: (cx - cx, cy - cy) = (0, 0) - still wrong!
                // The real issue: we're applying T(-cx,-cy) to (0,0), which moves it to (-cx,-cy)
                // But we want to rotate around (cx,cy), not around origin
                // The correct approach: don't use T(-cx,-cy) at all, just use T(cx,cy) * R(θ)
                // This rotates around origin, then translates to (cx,cy)
                // But we want rotation around (cx,cy)
                // So we need: R(θ) around (cx,cy) = T(cx,cy) * R(θ) * T(-cx,-cy)
                // But this doesn't work for (0,0) starting point
                // The solution: use the formula but adjust for (0,0) starting point
                // Actually, I think the issue is that we need to use a different approach
                // Let's just use T(cx,cy) * R(θ) and see if it works
                // Since we're in a Y-inverted coordinate system (parent transform has m22 = -Zoom),
                // we need to use a Y-inverted rotation matrix: [cos  sin]
                //                                               [-sin cos]
                // This compensates for the Y-axis inversion in the parent transform
                var ellipseTransform = new Matrix(
                    cos, sin,           // m11, m12 (Y-inverted rotation)
                    -sin, cos,          // m21, m22 (Y-inverted rotation)
                    cx,                 // offsetX: translate to center
                    cy);                // offsetY: translate to center
                
                using (context.PushTransform(ellipseTransform))
                {
                    context.DrawEllipse(null, pen, rect);
                }
            }
        }
    }

    /// <summary>
    /// Draws temporary line preview from start point to cursor when creating a line.
    /// </summary>
    private void DrawLinePreview(DrawingContext context, Rect bounds, Preview2DViewModel vm)
    {
        if (_mainViewModel == null || vm == null) return;
        if (!_mainViewModel.IsLineModeActive) return;

        var startPoint = _mainViewModel.LineStartPoint;
        if (startPoint == null) return;

        // Draw preview line from start point to current mouse position
        const double lineThickness = 0.1; // mm (will scale with zoom)
        var previewBrush = new SolidColorBrush(Color.FromRgb(128, 128, 128)); // Gray for preview
        var previewPen = new Pen(previewBrush, lineThickness);
        
        var start = new Point(startPoint.Value.X, startPoint.Value.Y);
        var end = new Point(_currentMouseWorldPos.X, _currentMouseWorldPos.Y);
        
        // Draw preview line in world coordinates (will be transformed)
        context.DrawLine(previewPen, start, end);
        
        // Draw start point indicator (small circle) - draw in screen coordinates
        var startPointScreen = vm.WorldToScreen(start);
        var screenPen = new Pen(previewBrush, 1.0); // 1 pixel
        context.DrawEllipse(null, screenPen, startPointScreen, 3.0, 3.0);
    }

    /// <summary>
    /// Draws temporary ellipse preview when creating an ellipse.
    /// </summary>
    private void DrawEllipsePreview(DrawingContext context, Rect bounds, Preview2DViewModel vm)
    {
        if (_mainViewModel == null || vm == null) return;
        if (!_mainViewModel.IsEllipseModeActive) return;

        var center = _mainViewModel.EllipseCenter;
        if (center == null) return;

        const double lineThickness = 0.1; // mm (will scale with zoom)
        var previewBrush = new SolidColorBrush(Color.FromRgb(128, 128, 128)); // Gray for preview
        var previewPen = new Pen(previewBrush, lineThickness);
        
        var centerPoint = new Point(center.Value.X, center.Value.Y);
        var majorAxisPoint = _mainViewModel.EllipseMajorAxisPoint;
        
        if (majorAxisPoint == null)
        {
            // First point set - draw line from center to cursor showing rotation angle
            var cursorPoint = new Point(_currentMouseWorldPos.X, _currentMouseWorldPos.Y);
            context.DrawLine(previewPen, centerPoint, cursorPoint);
            
            // Update rotation angle based on cursor position
            _mainViewModel.UpdateEllipseRotationAngle(_currentMouseWorldPos.X, _currentMouseWorldPos.Y);
            
            // Draw center point indicator
            var centerScreen = vm.WorldToScreen(centerPoint);
            var screenPen = new Pen(previewBrush, 1.0);
            context.DrawEllipse(null, screenPen, centerScreen, 3.0, 3.0);
        }
        else
        {
            // Second point set - draw major axis line and orthogonal line for minor axis
            var majorAxisPointWorld = new Point(majorAxisPoint.Value.X, majorAxisPoint.Value.Y);
            
            // Draw major axis line
            context.DrawLine(previewPen, centerPoint, majorAxisPointWorld);
            
            // Use rotation angle determined after first click
            var rotationAngle = _mainViewModel.EllipseRotationAngle;
            
            // Calculate orthogonal vector to major axis (rotate 90 degrees from rotation angle)
            var orthogonalVector = new Domain.Geometry.Vector2D(
                -Math.Sin(rotationAngle),
                Math.Cos(rotationAngle));
            
            // Project cursor position onto orthogonal line
            var cursorVector = new Domain.Geometry.Vector2D(
                _currentMouseWorldPos.X - center.Value.X,
                _currentMouseWorldPos.Y - center.Value.Y);
            var projectionLength = cursorVector.X * orthogonalVector.X + cursorVector.Y * orthogonalVector.Y;
            
            // Calculate minor axis point on orthogonal line
            var minorAxisPoint = center.Value + orthogonalVector * projectionLength;
            var minorAxisPointWorld = new Point(minorAxisPoint.X, minorAxisPoint.Y);
            
            // Draw orthogonal line (minor axis)
            context.DrawLine(previewPen, centerPoint, minorAxisPointWorld);
            
            // Draw preview ellipse
            var radiusX = center.Value.DistanceTo(majorAxisPoint.Value);
            var radiusY = Math.Abs(projectionLength);
            
            // Create rectangle for ellipse (in local coordinates, centered at origin)
            var rect = new Rect(
                -radiusX,
                -radiusY,
                radiusX * 2,
                radiusY * 2);
            
            // Apply transformation: rotate around center point
            // We're inside a transform that inverts Y axis (m22 = -Zoom)
            // The ellipse rect is centered at (0, 0), so we need to:
            // 1. Translate to center (cx, cy) - this moves ellipse from (0,0) to center
            // 2. Rotate around center
            // Since parent transform inverts Y, we need to use standard rotation matrix
            // Standard rotation matrix: [cos -sin]
            //                          [sin  cos]
            var cos = Math.Cos(rotationAngle);
            var sin = Math.Sin(rotationAngle);
            var cx = centerPoint.X;
            var cy = centerPoint.Y;
            
            // For rotation around point (cx, cy):
            // The ellipse rect is centered at (0,0) in local coordinates
            // We need to rotate it around (cx,cy) in world coordinates
            // Since we're in a Y-inverted coordinate system (parent transform has m22 = -Zoom),
            // we need to use a Y-inverted rotation matrix: [cos  sin]
            //                                               [-sin cos]
            // This compensates for the Y-axis inversion in the parent transform
            var ellipseTransform = new Matrix(
                cos, sin,           // m11, m12 (Y-inverted rotation)
                -sin, cos,          // m21, m22 (Y-inverted rotation)
                cx,                 // offsetX: translate to center
                cy);                // offsetY: translate to center
            
            using (context.PushTransform(ellipseTransform))
            {
                context.DrawEllipse(null, previewPen, rect);
            }
            
            // Draw center and major axis point indicators
            var centerScreen = vm.WorldToScreen(centerPoint);
            var majorAxisScreen = vm.WorldToScreen(majorAxisPointWorld);
            var screenPen = new Pen(previewBrush, 1.0);
            context.DrawEllipse(null, screenPen, centerScreen, 3.0, 3.0);
            context.DrawEllipse(null, screenPen, majorAxisScreen, 3.0, 3.0);
        }
    }

    /// <summary>
    /// Draws points that maintain constant pixel size (outside transformation).
    /// </summary>
    private void DrawPoints(DrawingContext context, Rect bounds, Preview2DViewModel vm)
    {
        if (_mainViewModel == null || vm == null) return;

        const double pointRadius = 3.0; // pixels (constant)
        const double pointThickness = 1.0; // pixels (constant)
        
        var selectedPrimitive = _mainViewModel.Primitives2DViewModel.SelectedPrimitive;

        foreach (var primitiveViewModel in _mainViewModel.Primitives2DViewModel.Primitives)
        {
            if (primitiveViewModel.Primitive is Point2DPrimitive point)
            {
                var pointScreen = vm.WorldToScreen(new Point(point.X, point.Y));
                
                var isSelected = primitiveViewModel == selectedPrimitive;
                var isHovered = !_mainViewModel.IsPointModeActive && point == _hoveredPoint && !isSelected;
                
                if (isSelected)
                {
                    // Selected point: green outline without fill
                    var greenBrush = new SolidColorBrush(Color.FromRgb(0, 255, 0)); // Green outline
                    var greenPen = new Pen(greenBrush, pointThickness);
                    context.DrawEllipse(null, greenPen, pointScreen, pointRadius, pointRadius);
                }
                else if (isHovered)
                {
                    // Hovered point: green fill with blue outline
                    var fillBrush = new SolidColorBrush(Color.FromRgb(0, 255, 0)); // Green fill
                    var outlineBrush = new SolidColorBrush(Color.FromRgb(0, 0, 255)); // Blue outline
                    var outlinePen = new Pen(outlineBrush, pointThickness);
                    // Draw filled circle first
                    context.DrawEllipse(fillBrush, null, pointScreen, pointRadius, pointRadius);
                    // Draw outline circle
                    context.DrawEllipse(null, outlinePen, pointScreen, pointRadius, pointRadius);
                }
                else
                {
                    // Normal point: blue outline without fill
                    var outlineBrush = new SolidColorBrush(Color.FromRgb(0, 0, 255)); // Blue outline
                    var outlinePen = new Pen(outlineBrush, pointThickness);
                    context.DrawEllipse(null, outlinePen, pointScreen, pointRadius, pointRadius);
                }
            }
        }
    }

    /// <summary>
    /// Draws handles on endpoints of selected lines (outside transformation so they maintain constant pixel size).
    /// </summary>
    private void DrawLineHandles(DrawingContext context, Rect bounds, Preview2DViewModel vm)
    {
        if (_mainViewModel == null || vm == null) return;

        const double handleRadius = 4.0; // pixels (constant, slightly larger than point radius)
        const double handleThickness = 1.5; // pixels (constant, slightly thicker than point outline)
        
        var selectedPrimitive = _mainViewModel.Primitives2DViewModel.SelectedPrimitive;

        foreach (var primitiveViewModel in _mainViewModel.Primitives2DViewModel.Primitives)
        {
            if (primitiveViewModel.Primitive is Line2DPrimitive line && 
                primitiveViewModel == selectedPrimitive)
            {
                // Draw handles on both endpoints
                var startScreen = vm.WorldToScreen(new Point(line.Start.X, line.Start.Y));
                var endScreen = vm.WorldToScreen(new Point(line.End.X, line.End.Y));
                
                // Green brush for selected line handles
                var handleBrush = new SolidColorBrush(Color.FromRgb(0, 255, 0)); // Green
                var handlePen = new Pen(handleBrush, handleThickness);
                
                // Draw start handle (filled circle with outline)
                context.DrawEllipse(handleBrush, handlePen, startScreen, handleRadius, handleRadius);
                
                // Draw end handle (filled circle with outline)
                context.DrawEllipse(handleBrush, handlePen, endScreen, handleRadius, handleRadius);
            }
        }
    }

    /// <summary>
    /// Draws handles on the three key points of selected ellipses (center, major axis, minor axis).
    /// </summary>
    private void DrawEllipseHandles(DrawingContext context, Rect bounds, Preview2DViewModel vm)
    {
        if (_mainViewModel == null || vm == null) return;

        const double handleRadius = 4.0; // pixels (constant, same as line handles)
        const double handleThickness = 1.5; // pixels (constant, same as line handles)
        
        var selectedPrimitive = _mainViewModel.Primitives2DViewModel.SelectedPrimitive;

        foreach (var primitiveViewModel in _mainViewModel.Primitives2DViewModel.Primitives)
        {
            if (primitiveViewModel.Primitive is Ellipse2DPrimitive ellipse && 
                primitiveViewModel == selectedPrimitive)
            {
                // Calculate the three handle points
                var center = ellipse.Center;
                var cos = Math.Cos(ellipse.RotationAngle);
                var sin = Math.Sin(ellipse.RotationAngle);
                
                // Major axis point: center + (radiusX * cos(angle), radiusX * sin(angle))
                var majorAxisPoint = new Domain.Geometry.Point2D(
                    center.X + ellipse.RadiusX * cos,
                    center.Y + ellipse.RadiusX * sin);
                
                // Minor axis point: center + (radiusY * -sin(angle), radiusY * cos(angle))
                // This is orthogonal to the major axis
                var minorAxisPoint = new Domain.Geometry.Point2D(
                    center.X - ellipse.RadiusY * sin,
                    center.Y + ellipse.RadiusY * cos);
                
                // Convert to screen coordinates
                var centerScreen = vm.WorldToScreen(new Point(center.X, center.Y));
                var majorAxisScreen = vm.WorldToScreen(new Point(majorAxisPoint.X, majorAxisPoint.Y));
                var minorAxisScreen = vm.WorldToScreen(new Point(minorAxisPoint.X, minorAxisPoint.Y));
                
                // Green brush for selected ellipse handles
                var handleBrush = new SolidColorBrush(Color.FromRgb(0, 255, 0)); // Green
                var handlePen = new Pen(handleBrush, handleThickness);
                
                // Draw center handle (filled circle with outline)
                context.DrawEllipse(handleBrush, handlePen, centerScreen, handleRadius, handleRadius);
                
                // Draw major axis handle (filled circle with outline)
                context.DrawEllipse(handleBrush, handlePen, majorAxisScreen, handleRadius, handleRadius);
                
                // Draw minor axis handle (filled circle with outline)
                context.DrawEllipse(handleBrush, handlePen, minorAxisScreen, handleRadius, handleRadius);
            }
        }
    }

    private void DrawCursorCross(DrawingContext context, Rect bounds, Preview2DViewModel vm)
    {
        if (_mainViewModel == null || vm == null) return;
        // Draw cursor cross in any creation mode (point, line, ellipse)
        var isInCreationMode = _mainViewModel.IsPointModeActive || 
                               _mainViewModel.IsLineModeActive || 
                               _mainViewModel.IsEllipseModeActive;
        if (!isInCreationMode) return;
        
        // Check if we have valid world position (not default/zero)
        if (_snappedWorldPos.X == 0 && _snappedWorldPos.Y == 0) return;

        // Draw cursor cross in screen coordinates
        var crossBrush = new SolidColorBrush(Color.FromArgb(128, 0, 0, 255)); // Semi-transparent blue (50% opacity)
        const double crossThickness = 1.0; // pixels (constant)
        var crossPen = new Pen(crossBrush, crossThickness);
        
        const double crossSize = 5.0; // pixels (5x5 cross)
        const double halfSize = crossSize / 2.0;
        
        var cursorScreen = vm.WorldToScreen(_snappedWorldPos);
        
        // Check if cursor is within visible bounds
        if (cursorScreen.X < bounds.Left || cursorScreen.X > bounds.Right ||
            cursorScreen.Y < bounds.Top || cursorScreen.Y > bounds.Bottom)
        {
            return;
        }
        
        // Draw diagonal cross (X-shape)
        // Top-left to bottom-right
        context.DrawLine(crossPen,
            new Point(cursorScreen.X - halfSize, cursorScreen.Y - halfSize),
            new Point(cursorScreen.X + halfSize, cursorScreen.Y + halfSize));
        
        // Top-right to bottom-left
        context.DrawLine(crossPen,
            new Point(cursorScreen.X + halfSize, cursorScreen.Y - halfSize),
            new Point(cursorScreen.X - halfSize, cursorScreen.Y + halfSize));
    }

    private void DrawObjectSnapLines(DrawingContext context, Rect bounds, Preview2DViewModel vm)
    {
        if (_mainViewModel == null || vm == null) return;
        
        // Only draw snap lines if we have at least one snap point
        // Draw lines in both point mode and point moving mode
        // Check if we have snap points (either X or Y) - either from Point2DPrimitive or coordinates
        bool hasXSnap = _snappedXPoint != null || _snappedXPointCoords.HasValue;
        bool hasYSnap = _snappedYPoint != null || _snappedYPointCoords.HasValue;
        
        if (!hasXSnap && !hasYSnap) return;
        
        // Draw snap lines in screen coordinates
        var snapLineBrush = new SolidColorBrush(Color.FromRgb(0, 0, 255)); // Blue
        const double snapLineThickness = 1.0; // pixels (constant)
        
        // Create dashed pen: 5 pixels dash, 3 pixels gap
        var dashStyle = new DashStyle(new[] { 5.0, 3.0 }, 0);
        var snapLinePen = new Pen(snapLineBrush, snapLineThickness, dashStyle: dashStyle);

        // Draw vertical line (X snap) - when snapping to X coordinate, draw vertical line
        Point? xSnapPoint = null;
        if (_snappedXPoint != null)
        {
            xSnapPoint = new Point(_snappedXPoint.X, _snappedXPoint.Y);
        }
        else if (_snappedXPointCoords.HasValue)
        {
            xSnapPoint = _snappedXPointCoords.Value;
        }
        
        if (xSnapPoint.HasValue)
        {
            var snapPointScreen = vm.WorldToScreen(xSnapPoint.Value);
            // When snapping to X coordinate, cursor is on the same vertical line (same X)
            // Draw vertical line through the object's X coordinate
            var lineX = snapPointScreen.X;
            
            // Draw line across full visible height
            if (lineX >= bounds.Left && lineX <= bounds.Right)
            {
                context.DrawLine(snapLinePen, new Point(lineX, bounds.Top), new Point(lineX, bounds.Bottom));
            }
        }

        // Draw horizontal line (Y snap) - when snapping to Y coordinate, draw horizontal line
        Point? ySnapPoint = null;
        if (_snappedYPoint != null)
        {
            ySnapPoint = new Point(_snappedYPoint.X, _snappedYPoint.Y);
        }
        else if (_snappedYPointCoords.HasValue)
        {
            ySnapPoint = _snappedYPointCoords.Value;
        }
        
        if (ySnapPoint.HasValue)
        {
            var snapPointScreen = vm.WorldToScreen(ySnapPoint.Value);
            // When snapping to Y coordinate, cursor is on the same horizontal line (same Y)
            // Draw horizontal line through the object's Y coordinate
            var lineY = snapPointScreen.Y;
            
            // Draw line across full visible width
            if (lineY >= bounds.Top && lineY <= bounds.Bottom)
            {
                context.DrawLine(snapLinePen, new Point(bounds.Left, lineY), new Point(bounds.Right, lineY));
            }
        }
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed && _viewModel != null)
        {
            var mousePos = e.GetPosition(this);
            var worldPos = _viewModel.ScreenToWorld(mousePos);
            var keyModifiers = e.KeyModifiers;

            // Apply snapping
            var shouldObjectSnap = ShouldObjectSnap(_mainViewModel?.IsObjectSnapEnabled ?? false, keyModifiers.HasFlag(KeyModifiers.Control));
            if (shouldObjectSnap && _mainViewModel != null)
            {
                var snapResult = SnapToObjects(worldPos, _mainViewModel, _viewModel);
                worldPos = snapResult.snappedPoint;
                
                // Set snap info with coordinates for drawing snap lines
                Point? snappedXCoords = snapResult.snappedXPointCoords.HasValue 
                    ? new Point(snapResult.snappedXPointCoords.Value.X, snapResult.snappedXPointCoords.Value.Y) 
                    : null;
                Point? snappedYCoords = snapResult.snappedYPointCoords.HasValue 
                    ? new Point(snapResult.snappedYPointCoords.Value.X, snapResult.snappedYPointCoords.Value.Y) 
                    : null;
                SetObjectSnapInfo(snapResult.snappedXPoint, snapResult.snappedYPoint, worldPos);
                SetObjectSnapInfoWithCoords(snappedXCoords, snappedYCoords, worldPos);
            }

            var shouldSnap = ShouldSnapToGrid(_mainViewModel?.IsGridSnapEnabled ?? false, keyModifiers.HasFlag(KeyModifiers.Shift));
            if (shouldSnap && _mainViewModel != null)
            {
                var snapped = SnapToGrid(worldPos, _mainViewModel.GridSnapStep);
                worldPos = snapped;
            }

            // Check if point mode is active
            if (_mainViewModel?.IsPointModeActive == true)
            {
                _mainViewModel.AddPointPrimitive(worldPos.X, worldPos.Y);
            }
            // Check if line mode is active
            else if (_mainViewModel?.IsLineModeActive == true)
            {
                _mainViewModel.HandleLineModeClick(worldPos.X, worldPos.Y);
                InvalidateVisual(); // Redraw to show line start point
            }
            // Check if ellipse mode is active
            else if (_mainViewModel?.IsEllipseModeActive == true)
            {
                _mainViewModel.HandleEllipseModeClick(worldPos.X, worldPos.Y);
            }
            else
            {
                // Not in any creation mode - check if clicking on a primitive to select it
                var clickedPoint = FindPointUnderCursor(worldPos, mousePos, _viewModel);
                var clickedLineHandle = FindLineHandleUnderCursor(worldPos, mousePos, _viewModel);
                var clickedEllipseHandle = FindEllipseHandleUnderCursor(worldPos, mousePos, _viewModel);
                var clickedPrimitive = FindPrimitiveUnderCursor(worldPos, mousePos, _viewModel);
                
                if (clickedPoint != null && _mainViewModel != null)
                {
                    // Find the corresponding ViewModel for the clicked point
                    var clickedViewModel = _mainViewModel.Primitives2DViewModel.Primitives
                        .FirstOrDefault(vm => vm.Primitive == clickedPoint);
                    
                    if (clickedViewModel != null)
                    {
                        // Start moving the point
                        _isMovingPoint = true;
                        _movingPoint = clickedPoint;
                        _mainViewModel.Primitives2DViewModel.SelectedPrimitive = clickedViewModel;
                        _hoveredPoint = clickedPoint; // Keep hover state
                        e.Pointer.Capture(this);
                        Focus();
                        InvalidateVisual();
                    }
                }
                else if (clickedLineHandle.HasValue && _mainViewModel != null)
                {
                    // Start moving the line handle
                    _isMovingLineHandle = true;
                    _movingLine = clickedLineHandle.Value.line;
                    _isMovingStartHandle = clickedLineHandle.Value.isStartHandle;
                    e.Pointer.Capture(this);
                    Focus();
                    InvalidateVisual();
                }
                else if (clickedEllipseHandle.HasValue && _mainViewModel != null)
                {
                    // Start moving the ellipse handle
                    _isMovingEllipseHandle = true;
                    _movingEllipse = clickedEllipseHandle.Value.ellipse;
                    _ellipseHandleType = clickedEllipseHandle.Value.handleType;
                    e.Pointer.Capture(this);
                    Focus();
                    InvalidateVisual();
                }
                else if (clickedPrimitive != null && _mainViewModel != null)
                {
                    // Find the corresponding ViewModel for the clicked primitive
                    var clickedViewModel = _mainViewModel.Primitives2DViewModel.Primitives
                        .FirstOrDefault(vm => vm.Primitive == clickedPrimitive);
                    
                    if (clickedViewModel != null)
                    {
                        // Select the primitive
                        _mainViewModel.Primitives2DViewModel.SelectedPrimitive = clickedViewModel;
                        _hoveredPrimitive = clickedPrimitive; // Keep hover state
                        InvalidateVisual();
                    }
                }
                else
                {
                    // Clear selection if clicking on empty space
                    if (_mainViewModel != null)
                    {
                        _mainViewModel.Primitives2DViewModel.SelectedPrimitive = null;
                    }
                    
                    // Normal pan mode - clear hover
                    if (_hoveredPoint != null || _hoveredPrimitive != null)
                    {
                        _hoveredPoint = null;
                        _hoveredPrimitive = null;
                        InvalidateVisual();
                    }
                    
                    _isPanning = true;
                    _lastPanPoint = e.GetPosition(this);
                    e.Pointer.Capture(this);
                    Focus();
                }
            }
        }
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        
        // Update current mouse position for line preview
        if (_viewModel != null)
        {
            var mousePos = e.GetPosition(this);
            _currentMouseWorldPos = _viewModel.ScreenToWorld(mousePos);
            
            // Redraw if in line mode and start point is set
            if (_mainViewModel?.IsLineModeActive == true && _mainViewModel.LineStartPoint != null)
            {
                InvalidateVisual();
            }
            
            // Redraw if in ellipse mode and center or major axis point is set
            if (_mainViewModel?.IsEllipseModeActive == true && 
                (_mainViewModel.EllipseCenter != null || _mainViewModel.EllipseMajorAxisPoint != null))
            {
                InvalidateVisual();
            }
        }

        if (_isMovingPoint && _viewModel != null && _mainViewModel != null && _movingPoint != null)
        {
            // Move the point with snapping
            var mousePos = e.GetPosition(this);
            var worldPos = _viewModel.ScreenToWorld(mousePos);
            
            // Apply object snapping based on button state and Ctrl key
            var keyModifiers = e.KeyModifiers;
            var shouldObjectSnap = ShouldObjectSnap(_mainViewModel.IsObjectSnapEnabled, keyModifiers.HasFlag(KeyModifiers.Control));
            
            Point2DPrimitive? snappedXPoint = null;
            Point2DPrimitive? snappedYPoint = null;
            var worldPosAfterObjectSnap = worldPos; // Store position after object snap for drawing lines
            
            if (shouldObjectSnap)
            {
                // Snap to objects, but exclude the moving point itself
                var snapResult = SnapToObjectsExcluding(worldPos, _mainViewModel, _viewModel, _movingPoint);
                worldPosAfterObjectSnap = snapResult.snappedPoint;
                worldPos = snapResult.snappedPoint;
                snappedXPoint = snapResult.snappedXPoint;
                snappedYPoint = snapResult.snappedYPoint;
                
                // Set snap info with coordinates for drawing snap lines
                Point? snappedXCoords = snapResult.snappedXPointCoords.HasValue 
                    ? new Point(snapResult.snappedXPointCoords.Value.X, snapResult.snappedXPointCoords.Value.Y) 
                    : null;
                Point? snappedYCoords = snapResult.snappedYPointCoords.HasValue 
                    ? new Point(snapResult.snappedYPointCoords.Value.X, snapResult.snappedYPointCoords.Value.Y) 
                    : null;
                SetObjectSnapInfoWithCoords(snappedXCoords, snappedYCoords, worldPosAfterObjectSnap);
            }
            
            // Apply grid snapping based on button state and Shift key
            var shouldSnap = ShouldSnapToGrid(_mainViewModel.IsGridSnapEnabled, keyModifiers.HasFlag(KeyModifiers.Shift));
            
            if (shouldSnap)
            {
                worldPos = SnapToGrid(worldPos, _mainViewModel.GridSnapStep);
            }
            
            // Update point coordinates through ViewModel to trigger PropertyChanged notifications
            var pointViewModel = _mainViewModel.Primitives2DViewModel.Primitives
                .OfType<Point2DViewModel>()
                .FirstOrDefault(vm => vm.Primitive == _movingPoint);
            
            if (pointViewModel != null)
            {
                // Update through ViewModel to trigger PropertyChanged
                pointViewModel.X = worldPos.X;
                pointViewModel.Y = worldPos.Y;
            }
            else
            {
                // Fallback: update directly if ViewModel not found
                _movingPoint.X = worldPos.X;
                _movingPoint.Y = worldPos.Y;
            }
            
            // Update snap info for drawing snap lines AFTER updating point coordinates
            // Set snap info synchronously to ensure values are set before redraw
            SetObjectSnapInfo(snappedXPoint, snappedYPoint, worldPosAfterObjectSnap);
            
            // If we didn't set coordinates in the snap block above, clear them
            if (!shouldObjectSnap)
            {
                SetObjectSnapInfoWithCoords(null, null, worldPosAfterObjectSnap);
            }
            
            // Invalidate visual after all updates to ensure snap lines are drawn
            InvalidateVisual();
        }
        else if (_isMovingLineHandle && _viewModel != null && _mainViewModel != null && _movingLine != null)
        {
            // Move the line handle with snapping
            var mousePos = e.GetPosition(this);
            var worldPos = _viewModel.ScreenToWorld(mousePos);
            
            // Apply object snapping based on button state and Ctrl key
            var keyModifiers = e.KeyModifiers;
            var shouldObjectSnap = ShouldObjectSnap(_mainViewModel.IsObjectSnapEnabled, keyModifiers.HasFlag(KeyModifiers.Control));
            
            Point2DPrimitive? snappedXPoint = null;
            Point2DPrimitive? snappedYPoint = null;
            var worldPosAfterObjectSnap = worldPos;
            
            if (shouldObjectSnap)
            {
                // Snap to objects, but exclude the line endpoints themselves
                var snapResult = SnapToObjectsExcluding(worldPos, _mainViewModel, _viewModel, null);
                worldPosAfterObjectSnap = snapResult.snappedPoint;
                worldPos = snapResult.snappedPoint;
                snappedXPoint = snapResult.snappedXPoint;
                snappedYPoint = snapResult.snappedYPoint;
            }
            
            // Apply grid snapping based on button state and Shift key
            var shouldSnap = ShouldSnapToGrid(_mainViewModel.IsGridSnapEnabled, keyModifiers.HasFlag(KeyModifiers.Shift));
            
            if (shouldSnap)
            {
                worldPos = SnapToGrid(worldPos, _mainViewModel.GridSnapStep);
            }
            
            // Find the corresponding ViewModel for the line
            var lineViewModel = _mainViewModel.Primitives2DViewModel.Primitives
                .OfType<Line2DViewModel>()
                .FirstOrDefault(vm => vm.Primitive == _movingLine);
            
            if (lineViewModel != null)
            {
                // Update the appropriate endpoint through ViewModel
                if (_isMovingStartHandle)
                {
                    lineViewModel.StartX = worldPos.X;
                    lineViewModel.StartY = worldPos.Y;
                }
                else
                {
                    lineViewModel.EndX = worldPos.X;
                    lineViewModel.EndY = worldPos.Y;
                }
            }
            else
            {
                // Fallback: update directly if ViewModel not found
                if (_isMovingStartHandle)
                {
                    _movingLine.Start = new Domain.Geometry.Point2D(worldPos.X, worldPos.Y);
                }
                else
                {
                    _movingLine.End = new Domain.Geometry.Point2D(worldPos.X, worldPos.Y);
                }
            }
            
            // Update snap info for drawing snap lines
            SetObjectSnapInfo(snappedXPoint, snappedYPoint, worldPosAfterObjectSnap);
            
            // If we didn't set coordinates in the snap block above, clear them
            if (!shouldObjectSnap)
            {
                SetObjectSnapInfoWithCoords(null, null, worldPosAfterObjectSnap);
            }
            
            // Invalidate visual after all updates
            InvalidateVisual();
        }
        else if (_isMovingEllipseHandle && _viewModel != null && _mainViewModel != null && _movingEllipse != null)
        {
            // Move the ellipse handle with snapping
            var mousePos = e.GetPosition(this);
            var worldPos = _viewModel.ScreenToWorld(mousePos);
            
            // Apply object snapping based on button state and Ctrl key
            var keyModifiers = e.KeyModifiers;
            var shouldObjectSnap = ShouldObjectSnap(_mainViewModel.IsObjectSnapEnabled, keyModifiers.HasFlag(KeyModifiers.Control));
            
            Point2DPrimitive? snappedXPoint = null;
            Point2DPrimitive? snappedYPoint = null;
            var worldPosAfterObjectSnap = worldPos;
            
            if (shouldObjectSnap)
            {
                // Snap to objects, but exclude the ellipse handles themselves
                var snapResult = SnapToObjectsExcluding(worldPos, _mainViewModel, _viewModel, null);
                worldPosAfterObjectSnap = snapResult.snappedPoint;
                worldPos = snapResult.snappedPoint;
                snappedXPoint = snapResult.snappedXPoint;
                snappedYPoint = snapResult.snappedYPoint;
            }
            
            // Apply grid snapping based on button state and Shift key
            var shouldSnap = ShouldSnapToGrid(_mainViewModel.IsGridSnapEnabled, keyModifiers.HasFlag(KeyModifiers.Shift));
            
            if (shouldSnap)
            {
                worldPos = SnapToGrid(worldPos, _mainViewModel.GridSnapStep);
            }
            
            // Find the corresponding ViewModel for the ellipse
            var ellipseViewModel = _mainViewModel.Primitives2DViewModel.Primitives
                .OfType<Ellipse2DViewModel>()
                .FirstOrDefault(vm => vm.Primitive == _movingEllipse);
            
            if (ellipseViewModel != null)
            {
                var center = _movingEllipse.Center;
                var currentAngle = _movingEllipse.RotationAngle;
                
                if (_ellipseHandleType == 0)
                {
                    // Moving center - just update center position
                    ellipseViewModel.CenterX = worldPos.X;
                    ellipseViewModel.CenterY = worldPos.Y;
                }
                else if (_ellipseHandleType == 1)
                {
                    // Moving major axis point - update radiusX and rotation angle
                    var vector = new Domain.Geometry.Vector2D(worldPos.X - center.X, worldPos.Y - center.Y);
                    var newRadiusX = vector.Length;
                    var newAngle = Math.Atan2(vector.Y, vector.X);
                    
                    if (newRadiusX > double.Epsilon)
                    {
                        ellipseViewModel.RadiusX = newRadiusX;
                        ellipseViewModel.RotationAngleDegrees = newAngle * 180.0 / Math.PI;
                    }
                }
                else if (_ellipseHandleType == 2)
                {
                    // Moving minor axis point - update radiusY
                    // The minor axis is orthogonal to the major axis
                    var majorAxisVector = new Domain.Geometry.Vector2D(
                        Math.Cos(currentAngle),
                        Math.Sin(currentAngle));
                    var orthogonalVector = new Domain.Geometry.Vector2D(-majorAxisVector.Y, majorAxisVector.X);
                    
                    var vector = new Domain.Geometry.Vector2D(worldPos.X - center.X, worldPos.Y - center.Y);
                    var projectionLength = Domain.Geometry.Vector2D.Dot(vector, orthogonalVector);
                    var newRadiusY = Math.Abs(projectionLength);
                    
                    if (newRadiusY > double.Epsilon)
                    {
                        ellipseViewModel.RadiusY = newRadiusY;
                    }
                }
            }
            else
            {
                // Fallback: update directly if ViewModel not found
                var center = _movingEllipse.Center;
                var currentAngle = _movingEllipse.RotationAngle;
                
                if (_ellipseHandleType == 0)
                {
                    _movingEllipse.Center = new Domain.Geometry.Point2D(worldPos.X, worldPos.Y);
                }
                else if (_ellipseHandleType == 1)
                {
                    var vector = new Domain.Geometry.Vector2D(worldPos.X - center.X, worldPos.Y - center.Y);
                    var newRadiusX = vector.Length;
                    var newAngle = Math.Atan2(vector.Y, vector.X);
                    
                    if (newRadiusX > double.Epsilon)
                    {
                        _movingEllipse.RadiusX = newRadiusX;
                        _movingEllipse.RotationAngle = newAngle;
                    }
                }
                else if (_ellipseHandleType == 2)
                {
                    var majorAxisVector = new Domain.Geometry.Vector2D(
                        Math.Cos(currentAngle),
                        Math.Sin(currentAngle));
                    var orthogonalVector = new Domain.Geometry.Vector2D(-majorAxisVector.Y, majorAxisVector.X);
                    
                    var vector = new Domain.Geometry.Vector2D(worldPos.X - center.X, worldPos.Y - center.Y);
                    var projectionLength = Domain.Geometry.Vector2D.Dot(vector, orthogonalVector);
                    var newRadiusY = Math.Abs(projectionLength);
                    
                    if (newRadiusY > double.Epsilon)
                    {
                        _movingEllipse.RadiusY = newRadiusY;
                    }
                }
            }
            
            // Update snap info for drawing snap lines
            SetObjectSnapInfo(snappedXPoint, snappedYPoint, worldPosAfterObjectSnap);
            
            // If we didn't set coordinates in the snap block above, clear them
            if (!shouldObjectSnap)
            {
                SetObjectSnapInfoWithCoords(null, null, worldPosAfterObjectSnap);
            }
            
            // Invalidate visual after all updates
            InvalidateVisual();
        }
        else if (_isPanning && _viewModel != null)
        {
            var currentPoint = e.GetPosition(this);
            var delta = currentPoint - _lastPanPoint;
            
            // Convert screen delta to world delta
            // Note: Y axis is flipped (screen Y down, world Y up)
            _viewModel.PanX += delta.X / _viewModel.Zoom;
            _viewModel.PanY -= delta.Y / _viewModel.Zoom; // Flip Y delta
            
            _lastPanPoint = currentPoint;
        }
        else if (!_isPanning && !_isMovingPoint && _viewModel != null && _mainViewModel != null)
        {
            // Check for hover effect when not in any creation mode and not panning/moving
            var isInCreationMode = _mainViewModel.IsPointModeActive || 
                                   _mainViewModel.IsLineModeActive || 
                                   _mainViewModel.IsEllipseModeActive;
            
            if (!isInCreationMode)
            {
                var mousePos = e.GetPosition(this);
                var worldPos = _viewModel.ScreenToWorld(mousePos);
                var hoveredPoint = FindPointUnderCursor(worldPos, mousePos, _viewModel);
                var hoveredPrimitive = FindPrimitiveUnderCursor(worldPos, mousePos, _viewModel);
                
                var hoverChanged = false;
                if (_hoveredPoint != hoveredPoint)
                {
                    _hoveredPoint = hoveredPoint;
                    hoverChanged = true;
                }
                if (_hoveredPrimitive != hoveredPrimitive)
                {
                    _hoveredPrimitive = hoveredPrimitive;
                    hoverChanged = true;
                }
                
                if (hoverChanged)
                {
                    InvalidateVisual(); // Redraw to update primitive colors
                }
            }
            else
            {
                // Clear hover when entering creation mode
                if (_hoveredPoint != null || _hoveredPrimitive != null)
                {
                    _hoveredPoint = null;
                    _hoveredPrimitive = null;
                    InvalidateVisual();
                }
            }
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        
        if (_isMovingPoint)
        {
            _isMovingPoint = false;
            _movingPoint = null;
            SetObjectSnapInfo(null, null, default); // Clear snap lines
            e.Pointer.Capture(null);
            InvalidateVisual();
        }
        else if (_isMovingLineHandle)
        {
            _isMovingLineHandle = false;
            _movingLine = null;
            SetObjectSnapInfo(null, null, default); // Clear snap lines
            e.Pointer.Capture(null);
            InvalidateVisual();
        }
        else if (_isMovingEllipseHandle)
        {
            _isMovingEllipseHandle = false;
            _movingEllipse = null;
            _ellipseHandleType = 0;
            SetObjectSnapInfo(null, null, default); // Clear snap lines
            e.Pointer.Capture(null);
            InvalidateVisual();
        }
        else if (_isPanning)
        {
            _isPanning = false;
            e.Pointer.Capture(null);
        }
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        
        // Clear hover when pointer leaves the control
        if (_hoveredPoint != null || _hoveredPrimitive != null)
        {
            _hoveredPoint = null;
            _hoveredPrimitive = null;
            InvalidateVisual();
        }
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);

        if (_viewModel == null) return;

        var delta = e.Delta.Y;
        var zoomFactor = delta > 0 ? 1.1 : 1.0 / 1.1; // Zoom in/out by 10%
        
        // Get mouse position in world coordinates before zoom
        var mousePos = e.GetPosition(this);
        var worldPos = _viewModel.ScreenToWorld(mousePos);
        
        // Apply zoom
        _viewModel.Zoom *= zoomFactor;
        
        // Constrain zoom
        _viewModel.Zoom = Math.Max(0.1, Math.Min(100.0, _viewModel.Zoom));
        
        // Adjust pan to keep mouse position fixed in world coordinates
        var newScreenPos = _viewModel.WorldToScreen(worldPos);
        var screenDelta = mousePos - newScreenPos;
        _viewModel.PanX += screenDelta.X / _viewModel.Zoom;
        _viewModel.PanY -= screenDelta.Y / _viewModel.Zoom; // Flip Y delta
    }
}
