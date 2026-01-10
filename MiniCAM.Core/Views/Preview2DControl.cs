using System;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Input;
using Avalonia.Styling;
using MiniCAM.Core.ViewModels;
using MiniCAM.Core.ViewModels.Main;

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
    private Point _snappedWorldPos;

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
            
            _mainViewModel = value;
            
            // Subscribe to new collection
            if (_mainViewModel?.Primitives2DViewModel?.Primitives is INotifyCollectionChanged newCollection)
            {
                newCollection.CollectionChanged += Primitives_CollectionChanged;
            }
            
            InvalidateVisual();
        }
    }

    public void SetObjectSnapInfo(Point2DPrimitive? snappedXPoint, Point2DPrimitive? snappedYPoint, Point snappedWorldPos)
    {
        _snappedXPoint = snappedXPoint;
        _snappedYPoint = snappedYPoint;
        _snappedWorldPos = snappedWorldPos;
        InvalidateVisual();
    }

    private void Primitives_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        InvalidateVisual();
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

            // Draw operations (will be implemented later)
            // DrawOperations(context, bounds, _viewModel);
        }

        // Draw axes outside of transformation so they maintain constant pixel size
        DrawAxes(context, bounds, _viewModel);
        
        // Draw primitives outside of transformation so they maintain constant pixel size
        DrawPrimitives(context, bounds, _viewModel);
        
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

    private (Point snappedPoint, Point2DPrimitive? snappedXPoint, Point2DPrimitive? snappedYPoint) SnapToObjects(Point worldPoint, MainViewModel mainViewModel, Preview2DViewModel vm)
    {
        const double snapTolerance = 5.0; // pixels tolerance for snapping
        
        Point2DPrimitive? nearestXPoint = null;
        Point2DPrimitive? nearestYPoint = null;
        double minXDistanceScreen = snapTolerance;
        double minYDistanceScreen = snapTolerance;
        
        var worldPosScreen = vm.WorldToScreen(worldPoint);
        
        // Find nearest points on X and Y axes
        foreach (var primitive in mainViewModel.Primitives2DViewModel.Primitives)
        {
            if (primitive is Point2DPrimitive point)
            {
                // Convert object point to screen coordinates
                var pointScreen = vm.WorldToScreen(new Point(point.X, point.Y));
                
                // Check distance in screen pixels
                var distanceXScreen = Math.Abs(pointScreen.X - worldPosScreen.X);
                var distanceYScreen = Math.Abs(pointScreen.Y - worldPosScreen.Y);
                
                // Check if point is close enough on X axis (within 5 pixels)
                if (distanceXScreen <= snapTolerance && distanceXScreen < minXDistanceScreen)
                {
                    minXDistanceScreen = distanceXScreen;
                    nearestXPoint = point;
                }
                
                // Check if point is close enough on Y axis (within 5 pixels)
                if (distanceYScreen <= snapTolerance && distanceYScreen < minYDistanceScreen)
                {
                    minYDistanceScreen = distanceYScreen;
                    nearestYPoint = point;
                }
            }
        }
        
        // Apply snapping
        var snappedX = nearestXPoint != null ? nearestXPoint.X : worldPoint.X;
        var snappedY = nearestYPoint != null ? nearestYPoint.Y : worldPoint.Y;
        
        return (new Point(snappedX, snappedY), nearestXPoint, nearestYPoint);
    }

    private void DrawPrimitives(DrawingContext context, Rect bounds, Preview2DViewModel vm)
    {
        if (_mainViewModel == null || vm == null) return;

        // Draw point primitives
        var pointBrush = new SolidColorBrush(Color.FromRgb(0, 0, 255)); // Blue
        const double pointRadius = 3.0; // pixels (constant)
        const double pointThickness = 1.0; // pixels (constant)
        var pointPen = new Pen(pointBrush, pointThickness);

        foreach (var primitive in _mainViewModel.Primitives2DViewModel.Primitives)
        {
            if (primitive is Point2DPrimitive point)
            {
                var pointScreen = vm.WorldToScreen(new Point(point.X, point.Y));
                
                // Draw point as circle (constant size in pixels, drawn in screen coordinates)
                context.DrawEllipse(null, pointPen, pointScreen, pointRadius, pointRadius);
            }
        }
    }

    private void DrawCursorCross(DrawingContext context, Rect bounds, Preview2DViewModel vm)
    {
        if (_mainViewModel == null || vm == null) return;
        if (!_mainViewModel.IsPointModeActive) return;
        
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
        if (_snappedXPoint == null && _snappedYPoint == null) return;
        
        // Check if we have valid world position (not default/zero)
        if (_snappedWorldPos.X == 0 && _snappedWorldPos.Y == 0) return;

        // Draw snap lines in screen coordinates
        var snapLineBrush = new SolidColorBrush(Color.FromRgb(0, 0, 255)); // Blue
        const double snapLineThickness = 1.0; // pixels (constant)
        
        // Create dashed pen: 5 pixels dash, 3 pixels gap
        var dashStyle = new DashStyle(new[] { 5.0, 3.0 }, 0);
        var snapLinePen = new Pen(snapLineBrush, snapLineThickness, dashStyle: dashStyle);

        var snappedScreen = vm.WorldToScreen(_snappedWorldPos);

        // Draw vertical line (X snap) - when snapping to X coordinate, draw vertical line
        if (_snappedXPoint != null)
        {
            var snapPointScreen = vm.WorldToScreen(new Point(_snappedXPoint.X, _snappedXPoint.Y));
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
        if (_snappedYPoint != null)
        {
            var snapPointScreen = vm.WorldToScreen(new Point(_snappedYPoint.X, _snappedYPoint.Y));
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
            // Check if point mode is active
            if (_mainViewModel?.IsPointModeActive == true)
            {
                // Add point at clicked location
                var mousePos = e.GetPosition(this);
                var worldPos = _viewModel.ScreenToWorld(mousePos);
                
                // Apply object snapping based on button state and Ctrl key
                var keyModifiers = e.KeyModifiers;
                var shouldObjectSnap = ShouldObjectSnap(_mainViewModel.IsObjectSnapEnabled, keyModifiers.HasFlag(KeyModifiers.Control));
                
                if (shouldObjectSnap)
                {
                    var snapResult = SnapToObjects(worldPos, _mainViewModel, _viewModel);
                    worldPos = snapResult.snappedPoint;
                }
                
                // Apply grid snapping based on button state and Shift key
                var shouldSnap = ShouldSnapToGrid(_mainViewModel.IsGridSnapEnabled, keyModifiers.HasFlag(KeyModifiers.Shift));
                
                if (shouldSnap)
                {
                    worldPos = SnapToGrid(worldPos, _mainViewModel.GridSnapStep);
                }
                
                _mainViewModel.AddPointPrimitive(worldPos.X, worldPos.Y);
            }
            else
            {
                // Normal pan mode
                _isPanning = true;
                _lastPanPoint = e.GetPosition(this);
                e.Pointer.Capture(this);
                Focus();
            }
        }
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        if (_isPanning && _viewModel != null)
        {
            var currentPoint = e.GetPosition(this);
            var delta = currentPoint - _lastPanPoint;
            
            // Convert screen delta to world delta
            // Note: Y axis is flipped (screen Y down, world Y up)
            _viewModel.PanX += delta.X / _viewModel.Zoom;
            _viewModel.PanY -= delta.Y / _viewModel.Zoom; // Flip Y delta
            
            _lastPanPoint = currentPoint;
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        
        if (_isPanning)
        {
            _isPanning = false;
            e.Pointer.Capture(null);
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
