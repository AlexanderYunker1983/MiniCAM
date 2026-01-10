using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using MiniCAM.Core.Localization;

namespace MiniCAM.Core.ViewModels.Main;

/// <summary>
/// ViewModel for point properties panel.
/// </summary>
public partial class PointPropertiesViewModel : LocalizedViewModelBase
{
    private Point2DPrimitive? _selectedPoint;
    private bool _isUpdatingFromPoint = false;
    private bool _isCreatingPoint = false; // Flag to prevent re-entry during point creation

    public PointPropertiesViewModel()
    {
    }

    /// <summary>
    /// Selected point to display/edit properties for.
    /// </summary>
    public Point2DPrimitive? SelectedPoint
    {
        get => _selectedPoint;
        set
        {
            if (_selectedPoint != null)
            {
                _selectedPoint.PropertyChanged -= SelectedPoint_PropertyChanged;
            }

            _selectedPoint = value;

            if (_selectedPoint != null)
            {
                _selectedPoint.PropertyChanged += SelectedPoint_PropertyChanged;
                UpdateFromPoint();
            }
            else
            {
                // No point selected - use cursor position
                UpdateFromPoint();
            }

            OnPropertyChanged();
        }
    }

    private void SelectedPoint_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (_isUpdatingFromPoint) return;

        // Update UI only if coordinate is not locked
        if (e.PropertyName == nameof(Point2DPrimitive.X) && !IsXLocked)
        {
            _isUpdatingFromPoint = true;
            X = _selectedPoint?.X ?? 0;
            _isUpdatingFromPoint = false;
        }
        else if (e.PropertyName == nameof(Point2DPrimitive.Y) && !IsYLocked)
        {
            _isUpdatingFromPoint = true;
            Y = _selectedPoint?.Y ?? 0;
            _isUpdatingFromPoint = false;
        }
    }

    private void UpdateFromPoint()
    {
        if (_selectedPoint == null)
        {
            // No point selected - use cursor position
            _isUpdatingFromPoint = true;
            if (!IsXLocked)
            {
                X = CursorPosition.X;
            }
            if (!IsYLocked)
            {
                Y = CursorPosition.Y;
            }
            _isUpdatingFromPoint = false;
            return;
        }

        _isUpdatingFromPoint = true;
        if (!IsXLocked)
        {
            X = _selectedPoint.X;
        }
        if (!IsYLocked)
        {
            Y = _selectedPoint.Y;
        }
        _isUpdatingFromPoint = false;
    }

    [ObservableProperty]
    private Point _cursorPosition;

    partial void OnCursorPositionChanged(Point value)
    {
        // Only update coordinates from cursor when no point is selected and in point mode
        if (_selectedPoint == null && !_isUpdatingFromPoint && !_isCreatingPoint)
        {
            _isUpdatingFromPoint = true;
            // Update only unlocked coordinates - locked coordinates stay fixed
            if (!IsXLocked)
            {
                X = value.X;
            }
            if (!IsYLocked)
            {
                Y = value.Y;
            }
            _isUpdatingFromPoint = false;
        }
    }

    [ObservableProperty]
    private double _x;

    [ObservableProperty]
    private double _y;

    private bool _isXLocked;
    
    public bool IsXLocked
    {
        get => _isXLocked;
        set
        {
            if (SetProperty(ref _isXLocked, value))
            {
                // Only call partial method if not creating point
                if (!_isCreatingPoint)
                {
                    OnIsXLockedChanged(value);
                }
            }
        }
    }

    private bool _isYLocked;
    
    public bool IsYLocked
    {
        get => _isYLocked;
        set
        {
            if (SetProperty(ref _isYLocked, value))
            {
                // Only call partial method if not creating point
                if (!_isCreatingPoint)
                {
                    OnIsYLockedChanged(value);
                }
            }
        }
    }

    [ObservableProperty]
    private string _xLabelText = Resources.CoordinateX;

    [ObservableProperty]
    private string _yLabelText = Resources.CoordinateY;

    partial void OnXChanged(double value)
    {
        if (_isUpdatingFromPoint || _isCreatingPoint) return;

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
        if (_isUpdatingFromPoint || _isCreatingPoint) return;

        // If point is selected, update it when user manually changes value
        if (_selectedPoint != null)
        {
            if (Math.Abs(_selectedPoint.Y - value) > 0.0001)
            {
                _selectedPoint.Y = value;
            }
        }
    }

    private void OnIsXLockedChanged(bool value)
    {
        if (value)
        {
            // X is now locked
            // Check if Y is also locked - if so, create point
            if (IsYLocked && _selectedPoint == null)
            {
                CreatePointAndResetLocks();
            }
        }
    }

    private void OnIsYLockedChanged(bool value)
    {
        if (value)
        {
            // Y is now locked
            // Check if X is also locked - if so, create point
            if (IsXLocked && _selectedPoint == null)
            {
                CreatePointAndResetLocks();
            }
        }
    }

    private void CreatePointAndResetLocks()
    {
        if (_isCreatingPoint) return; // Prevent re-entry
        
        // Set flag FIRST to prevent any handlers from processing
        _isCreatingPoint = true;
        
        // Save coordinates before resetting locks
        var x = X;
        var y = Y;
        
        // Raise event to create point
        PointCreatedFromLock?.Invoke(x, y);
        
        // Reset locks - the flag prevents OnIsXLockedChanged and OnIsYLockedChanged from being called
        // SetProperty will update the UI but won't call our custom handlers
        IsXLocked = false;
        IsYLocked = false;
        
        // Reset flag after properties are updated
        _isCreatingPoint = false;
    }
    
    /// <summary>
    /// Event raised when both coordinates are locked and point should be created.
    /// </summary>
    public event Action<double, double>? PointCreatedFromLock;

    protected override void UpdateLocalizedStrings()
    {
        XLabelText = Resources.CoordinateX;
        YLabelText = Resources.CoordinateY;
    }
}
