using System;
using CommunityToolkit.Mvvm.ComponentModel;
using MiniCAM.Core.Localization;
using MiniCAM.Core.ViewModels.Base;

namespace MiniCAM.Core.ViewModels.Main;

/// <summary>
/// ViewModel for ellipse properties panel.
/// </summary>
public partial class EllipsePropertiesViewModel : LocalizedViewModelBase
{
    private bool _isUpdatingFromEllipse = false;
    private Ellipse2DViewModel? _selectedEllipseViewModel;

    public EllipsePropertiesViewModel()
    {
    }

    /// <summary>
    /// Sets the selected ellipse ViewModel for tracking changes.
    /// </summary>
    internal void SetSelectedEllipseViewModel(Ellipse2DViewModel? viewModel)
    {
        // Unsubscribe from previous ViewModel
        if (_selectedEllipseViewModel != null)
        {
            _selectedEllipseViewModel.PropertyChanged -= SelectedEllipseViewModel_PropertyChanged;
        }

        _selectedEllipseViewModel = viewModel;

        // Subscribe to ViewModel changes
        if (_selectedEllipseViewModel != null)
        {
            _selectedEllipseViewModel.PropertyChanged += SelectedEllipseViewModel_PropertyChanged;
        }

        UpdateFromEllipse();
    }

    private void SelectedEllipseViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (_isUpdatingFromEllipse) return;

        // Update UI when ViewModel properties change
        if (e.PropertyName == nameof(Ellipse2DViewModel.CenterX))
        {
            _isUpdatingFromEllipse = true;
            CenterX = _selectedEllipseViewModel?.CenterX ?? 0;
            _isUpdatingFromEllipse = false;
        }
        else if (e.PropertyName == nameof(Ellipse2DViewModel.CenterY))
        {
            _isUpdatingFromEllipse = true;
            CenterY = _selectedEllipseViewModel?.CenterY ?? 0;
            _isUpdatingFromEllipse = false;
        }
        else if (e.PropertyName == nameof(Ellipse2DViewModel.RadiusX))
        {
            _isUpdatingFromEllipse = true;
            RadiusX = _selectedEllipseViewModel?.RadiusX ?? 0;
            _isUpdatingFromEllipse = false;
        }
        else if (e.PropertyName == nameof(Ellipse2DViewModel.RadiusY))
        {
            _isUpdatingFromEllipse = true;
            RadiusY = _selectedEllipseViewModel?.RadiusY ?? 0;
            _isUpdatingFromEllipse = false;
        }
        else if (e.PropertyName == nameof(Ellipse2DViewModel.RotationAngleDegrees))
        {
            _isUpdatingFromEllipse = true;
            RotationAngleDegrees = _selectedEllipseViewModel?.RotationAngleDegrees ?? 0;
            _isUpdatingFromEllipse = false;
        }
    }

    private void UpdateFromEllipse()
    {
        if (_selectedEllipseViewModel == null)
        {
            _isUpdatingFromEllipse = true;
            CenterX = 0;
            CenterY = 0;
            RadiusX = 0;
            RadiusY = 0;
            RotationAngleDegrees = 0;
            _isUpdatingFromEllipse = false;
            return;
        }

        _isUpdatingFromEllipse = true;
        CenterX = _selectedEllipseViewModel.CenterX;
        CenterY = _selectedEllipseViewModel.CenterY;
        RadiusX = _selectedEllipseViewModel.RadiusX;
        RadiusY = _selectedEllipseViewModel.RadiusY;
        RotationAngleDegrees = _selectedEllipseViewModel.RotationAngleDegrees;
        _isUpdatingFromEllipse = false;
    }

    [ObservableProperty]
    private double _centerX;

    [ObservableProperty]
    private double _centerY;

    [ObservableProperty]
    private double _radiusX;

    [ObservableProperty]
    private double _radiusY;

    [ObservableProperty]
    private double _rotationAngleDegrees;

    [ObservableProperty]
    private string _centerXLabelText = Resources.PrimitivePropertyCenter + " X";

    [ObservableProperty]
    private string _centerYLabelText = Resources.PrimitivePropertyCenter + " Y";

    [ObservableProperty]
    private string _radiusXLabelText = Resources.PrimitivePropertyRadiusX;

    [ObservableProperty]
    private string _radiusYLabelText = Resources.PrimitivePropertyRadiusY;

    [ObservableProperty]
    private string _rotationLabelText = Resources.PrimitivePropertyRotation + " (°)";

    partial void OnCenterXChanged(double value)
    {
        if (_isUpdatingFromEllipse) return;

        if (_selectedEllipseViewModel != null)
        {
            if (Math.Abs(_selectedEllipseViewModel.CenterX - value) > 0.0001)
            {
                _selectedEllipseViewModel.CenterX = value;
            }
        }
    }

    partial void OnCenterYChanged(double value)
    {
        if (_isUpdatingFromEllipse) return;

        if (_selectedEllipseViewModel != null)
        {
            if (Math.Abs(_selectedEllipseViewModel.CenterY - value) > 0.0001)
            {
                _selectedEllipseViewModel.CenterY = value;
            }
        }
    }

    partial void OnRadiusXChanged(double value)
    {
        if (_isUpdatingFromEllipse) return;

        if (_selectedEllipseViewModel != null)
        {
            if (Math.Abs(_selectedEllipseViewModel.RadiusX - value) > 0.0001)
            {
                _selectedEllipseViewModel.RadiusX = value;
            }
        }
    }

    partial void OnRadiusYChanged(double value)
    {
        if (_isUpdatingFromEllipse) return;

        if (_selectedEllipseViewModel != null)
        {
            if (Math.Abs(_selectedEllipseViewModel.RadiusY - value) > 0.0001)
            {
                _selectedEllipseViewModel.RadiusY = value;
            }
        }
    }

    partial void OnRotationAngleDegreesChanged(double value)
    {
        if (_isUpdatingFromEllipse) return;

        if (_selectedEllipseViewModel != null)
        {
            if (Math.Abs(_selectedEllipseViewModel.RotationAngleDegrees - value) > 0.1)
            {
                _selectedEllipseViewModel.RotationAngleDegrees = value;
            }
        }
    }

    protected override void UpdateLocalizedStrings()
    {
        CenterXLabelText = Resources.PrimitivePropertyCenter + " X";
        CenterYLabelText = Resources.PrimitivePropertyCenter + " Y";
        RadiusXLabelText = Resources.PrimitivePropertyRadiusX;
        RadiusYLabelText = Resources.PrimitivePropertyRadiusY;
        RotationLabelText = Resources.PrimitivePropertyRotation + " (°)";
    }
}
