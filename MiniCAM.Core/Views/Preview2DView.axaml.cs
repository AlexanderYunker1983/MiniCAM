using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using MiniCAM.Core.ViewModels;
using MiniCAM.Core.ViewModels.Main;

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
        
        if (mainViewModel?.IsPointModeActive == true)
        {
            var mousePos = e.GetPosition(this);
            var worldPos = vm.ScreenToWorld(e.GetPosition(PreviewControl));
            var originalWorldPos = worldPos;
            
            // Apply object snapping based on button state and Ctrl key
            var keyModifiers = e.KeyModifiers;
            var shouldObjectSnap = ShouldObjectSnap(mainViewModel.IsObjectSnapEnabled, keyModifiers.HasFlag(KeyModifiers.Control));
            
            Point2DPrimitive? snappedXPoint = null;
            Point2DPrimitive? snappedYPoint = null;
            
            if (shouldObjectSnap)
            {
                var snapResult = SnapToObjects(worldPos, mainViewModel, vm);
                worldPos = snapResult.snappedPoint;
                snappedXPoint = snapResult.snappedXPoint;
                snappedYPoint = snapResult.snappedYPoint;
            }
            
            // Apply grid snapping based on button state and Shift key
            var shouldSnap = ShouldSnapToGrid(mainViewModel.IsGridSnapEnabled, keyModifiers.HasFlag(KeyModifiers.Shift));
            
            if (shouldSnap)
            {
                worldPos = SnapToGrid(worldPos, mainViewModel.GridSnapStep);
            }
            
            // Apply coordinate locks from point properties
            var pointProps = mainViewModel.PointPropertiesViewModel;
            if (pointProps.IsXLocked)
            {
                worldPos = new Point(pointProps.X, worldPos.Y);
            }
            if (pointProps.IsYLocked)
            {
                worldPos = new Point(worldPos.X, pointProps.Y);
            }
            
            // Re-apply snapping after locks (if coordinates were changed by locks)
            if (pointProps.IsXLocked || pointProps.IsYLocked)
            {
                // Re-apply object snapping if enabled
                if (shouldObjectSnap)
                {
                    var snapResult = SnapToObjects(worldPos, mainViewModel, vm);
                    // Only apply snapping to unlocked coordinates
                    if (!pointProps.IsXLocked)
                    {
                        worldPos = new Point(snapResult.snappedPoint.X, worldPos.Y);
                        snappedXPoint = snapResult.snappedXPoint;
                    }
                    if (!pointProps.IsYLocked)
                    {
                        worldPos = new Point(worldPos.X, snapResult.snappedPoint.Y);
                        snappedYPoint = snapResult.snappedYPoint;
                    }
                }
                
                // Re-apply grid snapping if enabled
                if (shouldSnap)
                {
                    var snapped = SnapToGrid(worldPos, mainViewModel.GridSnapStep);
                    if (!pointProps.IsXLocked)
                    {
                        worldPos = new Point(snapped.X, worldPos.Y);
                    }
                    if (!pointProps.IsYLocked)
                    {
                        worldPos = new Point(worldPos.X, snapped.Y);
                    }
                }
            }
            
            // Store snap info for drawing
            PreviewControl?.SetObjectSnapInfo(snappedXPoint, snappedYPoint, worldPos);
            PreviewControl?.InvalidateVisual(); // Ensure visual is invalidated after setting snap info
            
            _coordinatesText.Text = $"X: {worldPos.X:F3}; Y: {worldPos.Y:F3}";
            
            // Update point properties view model with cursor position (only unlocked coordinates)
            if (mainViewModel.PointPropertiesViewModel.SelectedPoint == null)
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

    private (Point snappedPoint, Point2DPrimitive? snappedXPoint, Point2DPrimitive? snappedYPoint) SnapToObjects(Point worldPoint, MainViewModel mainViewModel, Preview2DViewModel vm)
    {
        const double snapTolerance = 5.0; // pixels tolerance for snapping
        var toleranceWorld = snapTolerance / vm.Zoom; // Convert to world coordinates
        
        Point2DPrimitive? nearestXPoint = null;
        Point2DPrimitive? nearestYPoint = null;
        double minXDistance = toleranceWorld;
        double minYDistance = toleranceWorld;
        
        // Find nearest points on X and Y axes
        foreach (var primitive in mainViewModel.Primitives2DViewModel.Primitives)
        {
            if (primitive is Point2DPrimitive point)
            {
                // Convert object point to screen coordinates
                var pointScreen = vm.WorldToScreen(new Point(point.X, point.Y));
                var worldPosScreen = vm.WorldToScreen(worldPoint);
                
                // Check distance in screen pixels
                var distanceXScreen = Math.Abs(pointScreen.X - worldPosScreen.X);
                var distanceYScreen = Math.Abs(pointScreen.Y - worldPosScreen.Y);
                
                // Check if point is close enough on X axis (within 5 pixels)
                if (distanceXScreen <= snapTolerance && distanceXScreen < minXDistance * vm.Zoom)
                {
                    minXDistance = Math.Abs(point.X - worldPoint.X);
                    nearestXPoint = point;
                }
                
                // Check if point is close enough on Y axis (within 5 pixels)
                if (distanceYScreen <= snapTolerance && distanceYScreen < minYDistance * vm.Zoom)
                {
                    minYDistance = Math.Abs(point.Y - worldPoint.Y);
                    nearestYPoint = point;
                }
            }
        }
        
        // Apply snapping
        var snappedX = nearestXPoint != null ? nearestXPoint.X : worldPoint.X;
        var snappedY = nearestYPoint != null ? nearestYPoint.Y : worldPoint.Y;
        
        return (new Point(snappedX, snappedY), nearestXPoint, nearestYPoint);
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
        
        // Subscribe to IsPointModeActive changes to update cursor
        if (mainViewModel != null)
        {
            mainViewModel.PropertyChanged += MainViewModel_PropertyChanged;
            UpdateCursor(mainViewModel.IsPointModeActive);
        }
    }

    private void MainViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainViewModel.IsPointModeActive) && sender is MainViewModel mainViewModel)
        {
            UpdateCursor(mainViewModel.IsPointModeActive);
        }
    }

    private void UpdateCursor(bool isPointModeActive)
    {
        // Hide cursor in point mode, show default cursor otherwise
        // Set cursor on both Preview2DView and PreviewControl for better compatibility
        var cursor = isPointModeActive ? new Cursor(StandardCursorType.None) : Cursor.Default;
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
        var parent = this.Parent;
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
