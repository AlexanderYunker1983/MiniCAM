using System;
using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using MiniCAM.Core.Domain.Primitives;
using MiniCAM.Core.Localization;
using MiniCAM.Core.ViewModels.Base;

namespace MiniCAM.Core.ViewModels.Main;

/// <summary>
/// ViewModel for point properties panel.
/// </summary>
public partial class PointPropertiesViewModel : LocalizedViewModelBase
{
    private Point2DPrimitive? _selectedPoint;
    private bool _isUpdatingFromPoint = false;

    public PointPropertiesViewModel()
    {
    }

    private Point2DViewModel? _selectedPointViewModel;

    /// <summary>
    /// Selected point to display/edit properties for.
    /// </summary>
    public Point2DPrimitive? SelectedPoint
    {
        get => _selectedPoint;
        set
        {
            // Unsubscribe from previous ViewModel
            if (_selectedPointViewModel != null)
            {
                _selectedPointViewModel.PropertyChanged -= SelectedPointViewModel_PropertyChanged;
            }

            _selectedPoint = value;
            
            // Find corresponding ViewModel if point is selected
            _selectedPointViewModel = null;
            if (value != null)
            {
                // Try to find ViewModel - this will be set from MainViewModel
                // For now, we'll update from domain model directly
            }
            
            UpdateFromPoint();
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Sets the selected point ViewModel for tracking changes.
    /// </summary>
    internal void SetSelectedPointViewModel(Point2DViewModel? viewModel)
    {
        // Unsubscribe from previous ViewModel
        if (_selectedPointViewModel != null)
        {
            _selectedPointViewModel.PropertyChanged -= SelectedPointViewModel_PropertyChanged;
        }

        _selectedPointViewModel = viewModel;
        _selectedPoint = viewModel?.Primitive as Point2DPrimitive;

        // Subscribe to ViewModel changes
        if (_selectedPointViewModel != null)
        {
            _selectedPointViewModel.PropertyChanged += SelectedPointViewModel_PropertyChanged;
        }

        UpdateFromPoint();
    }

    private void SelectedPointViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (_isUpdatingFromPoint) return;

        // Update UI when ViewModel coordinates change
        if (e.PropertyName == nameof(Point2DViewModel.X))
        {
            _isUpdatingFromPoint = true;
            X = _selectedPointViewModel?.X ?? 0;
            _isUpdatingFromPoint = false;
        }
        else if (e.PropertyName == nameof(Point2DViewModel.Y))
        {
            _isUpdatingFromPoint = true;
            Y = _selectedPointViewModel?.Y ?? 0;
            _isUpdatingFromPoint = false;
        }
    }

    private void UpdateFromPoint()
    {
        if (_selectedPoint == null)
        {
            // No point selected - use cursor position
            _isUpdatingFromPoint = true;
            X = CursorPosition.X;
            Y = CursorPosition.Y;
            _isUpdatingFromPoint = false;
            return;
        }

        _isUpdatingFromPoint = true;
        X = _selectedPoint.X;
        Y = _selectedPoint.Y;
        _isUpdatingFromPoint = false;
    }

    [ObservableProperty]
    private Point _cursorPosition;

    partial void OnCursorPositionChanged(Point value)
    {
        // Only update coordinates from cursor when no point is selected and in point mode
        if (_selectedPoint == null && !_isUpdatingFromPoint)
        {
            _isUpdatingFromPoint = true;
            X = value.X;
            Y = value.Y;
            _isUpdatingFromPoint = false;
        }
    }

    [ObservableProperty]
    private double _x;

    [ObservableProperty]
    private double _y;

    [ObservableProperty]
    private string _xLabelText = Resources.CoordinateX;

    [ObservableProperty]
    private string _yLabelText = Resources.CoordinateY;

    partial void OnXChanged(double value)
    {
        if (_isUpdatingFromPoint) return;

        // If point is selected, update it when user manually changes value
        if (_selectedPoint != null)
        {
            if (Math.Abs(_selectedPoint.X - value) > 0.0001)
            {
                _selectedPoint.X = value;
            }
        }
    }

    partial void OnYChanged(double value)
    {
        if (_isUpdatingFromPoint) return;

        // If point is selected, update it when user manually changes value
        if (_selectedPoint != null)
        {
            if (Math.Abs(_selectedPoint.Y - value) > 0.0001)
            {
                _selectedPoint.Y = value;
            }
        }
    }


    protected override void UpdateLocalizedStrings()
    {
        XLabelText = Resources.CoordinateX;
        YLabelText = Resources.CoordinateY;
    }
}
