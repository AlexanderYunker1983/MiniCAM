using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using MiniCAM.Core.ViewModels.Main;
using MainViewModel = MiniCAM.Core.ViewModels.Main.MainViewModel;

namespace MiniCAM.Core.Views;

public partial class Preview2DView : UserControl
{
    private Border? _tooltip;
    private TextBlock? _coordinatesText;
    private MainViewModel? _mainViewModel;

    public Preview2DView()
    {
        InitializeComponent();
        
        _tooltip = this.FindControl<Border>("CoordinatesTooltip");
        _coordinatesText = this.FindControl<TextBlock>("CoordinatesText");
        
        // Subscribe to pointer events on this control
        PointerMoved += Preview2DView_PointerMoved;
        PointerExited += Preview2DView_PointerExited;
        
        // Make control focusable to receive key events
        Focusable = true;
        
        if (PreviewControl != null)
        {
            // Set MainViewModel when DataContext changes
            DataContextChanged += Preview2DView_DataContextChanged;
        }
    }

    private void Preview2DView_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (DataContext is not Preview2DViewModel vm || _tooltip == null || _coordinatesText == null) return;
        
        var mainViewModel = GetMainViewModel();
        
        // Check if point is being moved (handled by Preview2DControl)
        // If so, don't interfere with snap info set by Preview2DControl
        if (PreviewControl != null)
        {
            var currentPoint = e.GetCurrentPoint(PreviewControl);
            if (currentPoint.Properties.IsLeftButtonPressed)
            {
                // Point might be moving - let Preview2DControl handle it
                // Don't clear snap info here
                return;
            }
        }
        
        // Handle cursor cross and coordinates display for all creation modes
        var isInCreationMode = mainViewModel?.IsPointModeActive == true || 
                              mainViewModel?.IsLineModeActive == true || 
                              mainViewModel?.IsEllipseModeActive == true;
        
        if (isInCreationMode && mainViewModel != null)
        {
            var mousePos = e.GetPosition(this);
            var worldPos = vm.ScreenToWorld(e.GetPosition(PreviewControl));
            var originalWorldPos = worldPos;
            
            // Apply object snapping based on button state and Ctrl key
            var keyModifiers = e.KeyModifiers;
            var shouldObjectSnap = ShouldObjectSnap(mainViewModel.IsObjectSnapEnabled, keyModifiers.HasFlag(KeyModifiers.Control));
            
            Domain.Primitives.Point2DPrimitive? snappedXPoint = null;
            Domain.Primitives.Point2DPrimitive? snappedYPoint = null;
            
            Point? snappedXCoords = null;
            Point? snappedYCoords = null;
            
            if (shouldObjectSnap)
            {
                var snapResult = SnapToObjects(worldPos, mainViewModel, vm);
                worldPos = snapResult.snappedPoint;
                snappedXPoint = snapResult.snappedXPoint;
                snappedYPoint = snapResult.snappedYPoint;
                snappedXCoords = snapResult.snappedXPointCoords.HasValue 
                    ? new Point(snapResult.snappedXPointCoords.Value.X, snapResult.snappedXPointCoords.Value.Y) 
                    : null;
                snappedYCoords = snapResult.snappedYPointCoords.HasValue 
                    ? new Point(snapResult.snappedYPointCoords.Value.X, snapResult.snappedYPointCoords.Value.Y) 
                    : null;
            }
            
            // Apply grid snapping based on button state and Shift key
            var shouldSnap = ShouldSnapToGrid(mainViewModel.IsGridSnapEnabled, keyModifiers.HasFlag(KeyModifiers.Shift));
            
            if (shouldSnap)
            {
                worldPos = SnapToGrid(worldPos, mainViewModel.GridSnapStep);
            }
            
            
            // Constrain third ellipse point to orthogonal line (only in ellipse mode when center and major axis are set)
            if (mainViewModel.IsEllipseModeActive && 
                mainViewModel.EllipseCenter.HasValue && 
                mainViewModel.EllipseMajorAxisPoint.HasValue)
            {
                var center = mainViewModel.EllipseCenter.Value;
                var rotationAngle = mainViewModel.EllipseRotationAngle;
                
                // Calculate orthogonal vector to major axis (rotate 90 degrees)
                var majorAxisDirection = new Domain.Geometry.Vector2D(Math.Cos(rotationAngle), Math.Sin(rotationAngle));
                var orthogonalVector = new Domain.Geometry.Vector2D(-majorAxisDirection.Y, majorAxisDirection.X);
                
                // Project cursor position onto orthogonal line
                var cursorVector = new Domain.Geometry.Vector2D(
                    worldPos.X - center.X,
                    worldPos.Y - center.Y);
                var projectionLength = cursorVector.X * orthogonalVector.X + cursorVector.Y * orthogonalVector.Y;
                
                // Calculate constrained point on orthogonal line
                var constrainedPoint = center + orthogonalVector * projectionLength;
                worldPos = new Point(constrainedPoint.X, constrainedPoint.Y);
            }
            
            // Store snap info for drawing
            PreviewControl?.SetObjectSnapInfo(snappedXPoint, snappedYPoint, worldPos);
            PreviewControl?.SetObjectSnapInfoWithCoords(snappedXCoords, snappedYCoords, worldPos);
            PreviewControl?.InvalidateVisual(); // Ensure visual is invalidated after setting snap info
            
            _coordinatesText.Text = $"X: {worldPos.X:F3}; Y: {worldPos.Y:F3}";
            
            // Update point properties view model with cursor position (only in point mode)
            if (mainViewModel.IsPointModeActive && mainViewModel.PointPropertiesViewModel.SelectedPoint == null)
            {
                mainViewModel.PointPropertiesViewModel.CursorPosition = worldPos;
            }
            
            // Position tooltip near cursor
            var tooltipOffset = 15.0;
            Canvas.SetLeft(_tooltip, mousePos.X + tooltipOffset);
            Canvas.SetTop(_tooltip, mousePos.Y + tooltipOffset);
            
            if (!_tooltip.IsVisible)
            {
                _tooltip.IsVisible = true;
            }
        }
        else
        {
            // Only clear snap info if left button is not pressed (not moving a point)
            if (!e.GetCurrentPoint(PreviewControl).Properties.IsLeftButtonPressed)
            {
                PreviewControl?.SetObjectSnapInfo(null, null, default);
                PreviewControl?.SetObjectSnapInfoWithCoords(null, null, default);
                PreviewControl?.InvalidateVisual(); // Ensure visual is invalidated after clearing snap info
            }
            if (_tooltip.IsVisible)
            {
                _tooltip.IsVisible = false;
            }
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

    /// <summary>
    /// Gets all snap points from all primitives (point primitives, line endpoints, ellipse centers).
    /// </summary>
    private List<(Domain.Geometry.Point2D Point, Domain.Primitives.Primitive2D? SourcePrimitive)> GetAllSnapPoints(MainViewModel mainViewModel)
    {
        var snapPoints = new List<(Domain.Geometry.Point2D, Domain.Primitives.Primitive2D?)>();
        
        foreach (var primitiveViewModel in mainViewModel.Primitives2DViewModel.Primitives)
        {
            var primitive = primitiveViewModel.Primitive;
            
            if (primitive is Domain.Primitives.Point2DPrimitive point)
            {
                snapPoints.Add((new Domain.Geometry.Point2D(point.X, point.Y), primitive));
            }
            else if (primitive is Domain.Primitives.Line2DPrimitive line)
            {
                // Add start and end points of the line
                snapPoints.Add((line.Start, primitive));
                snapPoints.Add((line.End, primitive));
            }
            else if (primitive is Domain.Primitives.Ellipse2DPrimitive ellipse)
            {
                // Add center point of the ellipse
                snapPoints.Add((ellipse.Center, primitive));
            }
        }
        
        return snapPoints;
    }

    private (Point snappedPoint, Domain.Primitives.Point2DPrimitive? snappedXPoint, Domain.Primitives.Point2DPrimitive? snappedYPoint, Domain.Geometry.Point2D? snappedXPointCoords, Domain.Geometry.Point2D? snappedYPointCoords) SnapToObjects(Point worldPoint, MainViewModel mainViewModel, Preview2DViewModel vm)
    {
        const double snapTolerance = 5.0; // pixels tolerance for snapping
        var toleranceWorld = snapTolerance / vm.Zoom; // Convert to world coordinates
        
        Domain.Geometry.Point2D? nearestXPoint = null;
        Domain.Geometry.Point2D? nearestYPoint = null;
        double minXDistance = toleranceWorld;
        double minYDistance = toleranceWorld;
        
        var worldPosScreen = vm.WorldToScreen(worldPoint);
        
        // Get all snap points (from point primitives, line endpoints, ellipse centers)
        var snapPoints = GetAllSnapPoints(mainViewModel);
        
        // Find nearest points on X and Y axes
        foreach (var (snapPoint, _) in snapPoints)
        {
            // Convert snap point to screen coordinates
            var pointScreen = vm.WorldToScreen(new Point(snapPoint.X, snapPoint.Y));
            
            // Check distance in screen pixels
            var distanceXScreen = Math.Abs(pointScreen.X - worldPosScreen.X);
            var distanceYScreen = Math.Abs(pointScreen.Y - worldPosScreen.Y);
            
            // Check if point is close enough on X axis (within 5 pixels)
            if (distanceXScreen <= snapTolerance && distanceXScreen < minXDistance * vm.Zoom)
            {
                minXDistance = Math.Abs(snapPoint.X - worldPoint.X);
                nearestXPoint = snapPoint;
            }
            
            // Check if point is close enough on Y axis (within 5 pixels)
            if (distanceYScreen <= snapTolerance && distanceYScreen < minYDistance * vm.Zoom)
            {
                minYDistance = Math.Abs(snapPoint.Y - worldPoint.Y);
                nearestYPoint = snapPoint;
            }
        }
        
        // Apply snapping
        var snappedX = nearestXPoint.HasValue ? nearestXPoint.Value.X : worldPoint.X;
        var snappedY = nearestYPoint.HasValue ? nearestYPoint.Value.Y : worldPoint.Y;
        
        // Convert back to Point2DPrimitive for compatibility (find the source primitive if it exists)
        Domain.Primitives.Point2DPrimitive? snappedXPointPrimitive = null;
        Domain.Primitives.Point2DPrimitive? snappedYPointPrimitive = null;
        
        if (nearestXPoint.HasValue)
        {
            // Try to find the source point primitive
            foreach (var (snapPoint, sourcePrimitive) in snapPoints)
            {
                if (snapPoint.X == nearestXPoint.Value.X && snapPoint.Y == nearestXPoint.Value.Y && sourcePrimitive is Domain.Primitives.Point2DPrimitive pointPrim)
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
                if (snapPoint.X == nearestYPoint.Value.X && snapPoint.Y == nearestYPoint.Value.Y && sourcePrimitive is Domain.Primitives.Point2DPrimitive pointPrim)
                {
                    snappedYPointPrimitive = pointPrim;
                    break;
                }
            }
        }
        
        return (new Point(snappedX, snappedY), snappedXPointPrimitive, snappedYPointPrimitive, nearestXPoint, nearestYPoint);
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

    private Point SnapToGrid(Point worldPoint, double snapStep)
    {
        var snappedX = Math.Round(worldPoint.X / snapStep) * snapStep;
        var snappedY = Math.Round(worldPoint.Y / snapStep) * snapStep;
        return new Point(snappedX, snappedY);
    }

    private void Preview2DView_PointerExited(object? sender, PointerEventArgs e)
    {
        if (_tooltip != null)
        {
            _tooltip.IsVisible = false;
        }
    }

    private void Preview2DView_DataContextChanged(object? sender, EventArgs e)
    {
        UpdateMainViewModel();
    }

    private void UpdateMainViewModel()
    {
        // Unsubscribe from old view model
        if (_mainViewModel != null)
        {
            _mainViewModel.PropertyChanged -= MainViewModel_PropertyChanged;
        }
        
        var mainViewModel = GetMainViewModel();
        _mainViewModel = mainViewModel;
        
        if (PreviewControl != null)
        {
            PreviewControl.MainViewModel = mainViewModel;
        }
        
        // Subscribe to creation mode changes to update cursor
        if (mainViewModel != null)
        {
            mainViewModel.PropertyChanged += MainViewModel_PropertyChanged;
            var isInCreationMode = mainViewModel.IsPointModeActive || 
                                  mainViewModel.IsLineModeActive || 
                                  mainViewModel.IsEllipseModeActive;
            UpdateCursor(isInCreationMode);
        }
    }

    private void MainViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (sender is MainViewModel mainViewModel)
        {
            if (e.PropertyName == nameof(MainViewModel.IsPointModeActive) ||
                e.PropertyName == nameof(MainViewModel.IsLineModeActive) ||
                e.PropertyName == nameof(MainViewModel.IsEllipseModeActive))
            {
                var isInCreationMode = mainViewModel.IsPointModeActive || 
                                      mainViewModel.IsLineModeActive || 
                                      mainViewModel.IsEllipseModeActive;
                UpdateCursor(isInCreationMode);
            }
        }
    }

    private void UpdateCursor(bool isInCreationMode)
    {
        // Hide cursor in any creation mode (point, line, ellipse), show default cursor otherwise
        // Set cursor on both Preview2DView and PreviewControl for better compatibility
        var cursor = isInCreationMode ? new Cursor(StandardCursorType.None) : Cursor.Default;
        Cursor = cursor;
        if (PreviewControl != null)
        {
            PreviewControl.Cursor = cursor;
        }
    }

    protected override void OnAttachedToVisualTree(Avalonia.VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        UpdateMainViewModel();
    }

    protected override void OnDetachedFromVisualTree(Avalonia.VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        
        // Unsubscribe from MainViewModel events
        if (_mainViewModel != null)
        {
            _mainViewModel.PropertyChanged -= MainViewModel_PropertyChanged;
            _mainViewModel = null;
        }
        
        // Reset cursor to default
        Cursor = Cursor.Default;
    }


    private MainViewModel? GetMainViewModel()
    {
        // Find MainViewModel in the visual tree
        var parent = Parent;
        while (parent != null)
        {
            if (parent is Control control && control.DataContext is MainViewModel mainVm)
            {
                return mainVm;
            }
            parent = parent.Parent;
        }
        return null;
    }
}
