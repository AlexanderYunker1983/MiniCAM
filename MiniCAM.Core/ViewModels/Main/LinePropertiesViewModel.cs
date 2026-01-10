using System;
using CommunityToolkit.Mvvm.ComponentModel;
using MiniCAM.Core.Localization;
using MiniCAM.Core.ViewModels.Base;

namespace MiniCAM.Core.ViewModels.Main;

/// <summary>
/// ViewModel for line properties panel.
/// </summary>
public partial class LinePropertiesViewModel : LocalizedViewModelBase
{
    private bool _isUpdatingFromLine = false;
    private Line2DViewModel? _selectedLineViewModel;

    public LinePropertiesViewModel()
    {
    }

    /// <summary>
    /// Sets the selected line ViewModel for tracking changes.
    /// </summary>
    internal void SetSelectedLineViewModel(Line2DViewModel? viewModel)
    {
        // Unsubscribe from previous ViewModel
        if (_selectedLineViewModel != null)
        {
            _selectedLineViewModel.PropertyChanged -= SelectedLineViewModel_PropertyChanged;
        }

        _selectedLineViewModel = viewModel;

        // Subscribe to ViewModel changes
        if (_selectedLineViewModel != null)
        {
            _selectedLineViewModel.PropertyChanged += SelectedLineViewModel_PropertyChanged;
        }

        UpdateFromLine();
    }

    private void SelectedLineViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (_isUpdatingFromLine) return;

        // Update UI when ViewModel properties change
        if (e.PropertyName == nameof(Line2DViewModel.StartX))
        {
            _isUpdatingFromLine = true;
            StartX = _selectedLineViewModel?.StartX ?? 0;
            _isUpdatingFromLine = false;
        }
        else if (e.PropertyName == nameof(Line2DViewModel.StartY))
        {
            _isUpdatingFromLine = true;
            StartY = _selectedLineViewModel?.StartY ?? 0;
            _isUpdatingFromLine = false;
        }
        else if (e.PropertyName == nameof(Line2DViewModel.EndX))
        {
            _isUpdatingFromLine = true;
            EndX = _selectedLineViewModel?.EndX ?? 0;
            _isUpdatingFromLine = false;
        }
        else if (e.PropertyName == nameof(Line2DViewModel.EndY))
        {
            _isUpdatingFromLine = true;
            EndY = _selectedLineViewModel?.EndY ?? 0;
            _isUpdatingFromLine = false;
        }
        else if (e.PropertyName == nameof(Line2DViewModel.Length))
        {
            _isUpdatingFromLine = true;
            Length = _selectedLineViewModel?.Length ?? 0;
            _isUpdatingFromLine = false;
        }
    }

    private void UpdateFromLine()
    {
        if (_selectedLineViewModel == null)
        {
            _isUpdatingFromLine = true;
            StartX = 0;
            StartY = 0;
            EndX = 0;
            EndY = 0;
            Length = 0;
            _isUpdatingFromLine = false;
            return;
        }

        _isUpdatingFromLine = true;
        StartX = _selectedLineViewModel.StartX;
        StartY = _selectedLineViewModel.StartY;
        EndX = _selectedLineViewModel.EndX;
        EndY = _selectedLineViewModel.EndY;
        Length = _selectedLineViewModel.Length;
        _isUpdatingFromLine = false;
    }

    [ObservableProperty]
    private double _startX;

    [ObservableProperty]
    private double _startY;

    [ObservableProperty]
    private double _endX;

    [ObservableProperty]
    private double _endY;

    [ObservableProperty]
    private double _length;

    [ObservableProperty]
    private string _startXLabelText = Resources.PrimitivePropertyStart + " X";

    [ObservableProperty]
    private string _startYLabelText = Resources.PrimitivePropertyStart + " Y";

    [ObservableProperty]
    private string _endXLabelText = Resources.PrimitivePropertyEnd + " X";

    [ObservableProperty]
    private string _endYLabelText = Resources.PrimitivePropertyEnd + " Y";

    [ObservableProperty]
    private string _lengthLabelText = Resources.PrimitivePropertyLength;

    partial void OnStartXChanged(double value)
    {
        if (_isUpdatingFromLine) return;

        if (_selectedLineViewModel != null)
        {
            if (Math.Abs(_selectedLineViewModel.StartX - value) > 0.0001)
            {
                _selectedLineViewModel.StartX = value;
            }
        }
    }

    partial void OnStartYChanged(double value)
    {
        if (_isUpdatingFromLine) return;

        if (_selectedLineViewModel != null)
        {
            if (Math.Abs(_selectedLineViewModel.StartY - value) > 0.0001)
            {
                _selectedLineViewModel.StartY = value;
            }
        }
    }

    partial void OnEndXChanged(double value)
    {
        if (_isUpdatingFromLine) return;

        if (_selectedLineViewModel != null)
        {
            if (Math.Abs(_selectedLineViewModel.EndX - value) > 0.0001)
            {
                _selectedLineViewModel.EndX = value;
            }
        }
    }

    partial void OnEndYChanged(double value)
    {
        if (_isUpdatingFromLine) return;

        if (_selectedLineViewModel != null)
        {
            if (Math.Abs(_selectedLineViewModel.EndY - value) > 0.0001)
            {
                _selectedLineViewModel.EndY = value;
            }
        }
    }

    protected override void UpdateLocalizedStrings()
    {
        StartXLabelText = Resources.PrimitivePropertyStart + " X";
        StartYLabelText = Resources.PrimitivePropertyStart + " Y";
        EndXLabelText = Resources.PrimitivePropertyEnd + " X";
        EndYLabelText = Resources.PrimitivePropertyEnd + " Y";
        LengthLabelText = Resources.PrimitivePropertyLength;
    }
}
