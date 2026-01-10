using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MiniCAM.Core.CodeGeneration;
using MiniCAM.Core.Localization;
using MiniCAM.Core.Settings;
using MiniCAM.Core.ViewModels.Base;
using MiniCAM.Core.Views;

namespace MiniCAM.Core.ViewModels.Main;

/// <summary>
/// Main view model for the application window.
/// Manages operations list, ribbon tabs, and application settings dialogs.
/// </summary>
public partial class MainViewModel : LocalizedViewModelBase
{
    private readonly ISettingsService _settingsService;
    
    // Line creation state
    private Domain.Geometry.Point2D? _lineStartPoint;
    
    // Ellipse creation state
    private Domain.Geometry.Point2D? _ellipseCenter;
    private double _ellipseRotationAngle = 0; // Angle determined after first click
    private Domain.Geometry.Point2D? _ellipseMajorAxisPoint;

    public MainViewModel(ISettingsService settingsService)
    {
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
        
        // Initialize 2D Preview ViewModel
        Preview2DViewModel = new Preview2DViewModel();
        
        // Initialize 2D Primitives ViewModel
        Primitives2DViewModel = new Primitives2DViewModel();
        
        // Initialize Properties ViewModels
        PointPropertiesViewModel = new PointPropertiesViewModel();
        LinePropertiesViewModel = new LinePropertiesViewModel();
        EllipsePropertiesViewModel = new EllipsePropertiesViewModel();
        
        // Subscribe to selected primitive changes
        Primitives2DViewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(Primitives2DViewModel.SelectedPrimitive))
            {
                UpdatePrimitiveProperties();
            }
        };
        
        // Subscribe to primitive creation mode changes
        PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(IsPointModeActive))
            {
                UpdatePrimitiveProperties();
                // Update selection enabled state based on any active creation mode
                UpdateSelectionEnabled();
                // Disable other modes when point mode is activated
                if (IsPointModeActive)
                {
                    IsLineModeActive = false;
                    IsEllipseModeActive = false;
                }
            }
            else if (e.PropertyName == nameof(IsLineModeActive))
            {
                // Update selection enabled state based on any active creation mode
                UpdateSelectionEnabled();
                // Disable other modes when line mode is activated
                if (IsLineModeActive)
                {
                    IsPointModeActive = false;
                    IsEllipseModeActive = false;
                    // Reset line creation state
                    _lineStartPoint = null;
                }
            }
            else if (e.PropertyName == nameof(IsEllipseModeActive))
            {
                // Update selection enabled state based on any active creation mode
                UpdateSelectionEnabled();
                // Disable other modes when ellipse mode is activated
                if (IsEllipseModeActive)
                {
                    IsPointModeActive = false;
                    IsLineModeActive = false;
                }
                else
                {
                    // Reset ellipse creation state when exiting ellipse mode
                    _ellipseCenter = null;
                    _ellipseMajorAxisPoint = null;
                    _ellipseRotationAngle = 0;
                    OnPropertyChanged(nameof(EllipseCenter));
                    OnPropertyChanged(nameof(EllipseMajorAxisPoint));
                    OnPropertyChanged(nameof(EllipseRotationAngle));
                }
            }
        };
        
        // Sync operations between MainViewModel and Preview2DViewModel
        Operations.CollectionChanged += (s, e) =>
        {
            UpdateCommandAvailability();
            SyncOperationsToPreview();
        };
        
        GCodeLines.CollectionChanged += (s, e) => UpdateGCodeCommandAvailability();
    }

    private void SyncOperationsToPreview()
    {
        Preview2DViewModel.Operations.Clear();
        foreach (var operationViewModel in Operations)
        {
            // Preview2DViewModel still uses old OperationItem for display
            // Create a temporary OperationItem for compatibility
            var operationItem = new OperationItem(operationViewModel.Name)
            {
                IsEnabled = operationViewModel.IsEnabled
            };
            Preview2DViewModel.Operations.Add(operationItem);
        }
    }

    protected override void UpdateLocalizedStrings()
    {
        MenuSettingsText = Resources.MenuSettings;
        MenuApplicationSettingsText = Resources.MenuApplicationSettings;
        MenuCodeGenerationSettingsText = Resources.MenuCodeGenerationSettings;
        RibbonTabDrillingText = Resources.RibbonTab2DPrimitives;
        RibbonTabPocketText = Resources.RibbonTabPocket;
        RibbonTabProfileText = Resources.RibbonTabProfile;
        RibbonTabOtherText = Resources.RibbonTabOther;
        OperationPropertiesText = Resources.OperationProperties;
        Preview2DText = Resources.Preview2D;
        OperationsListText = Resources.OperationsList;
        GCodeText = Resources.GCode;
        Primitives2DText = Resources.Primitives2D;
        PointButtonText = Resources.ButtonPoint;
        LineButtonText = Resources.ButtonLine;
        EllipseButtonText = Resources.ButtonEllipse;
        GridSnapButtonText = Resources.ButtonGridSnap;
        ObjectSnapButtonText = Resources.ButtonObjectSnap;
        GenerateButtonText = Resources.ButtonGenerate;
        SaveButtonText = Resources.ButtonSave;
    }

    [ObservableProperty]
    private string _menuSettingsText = Resources.MenuSettings;

    [ObservableProperty]
    private string _menuApplicationSettingsText = Resources.MenuApplicationSettings;

    [ObservableProperty]
    private string _menuCodeGenerationSettingsText = Resources.MenuCodeGenerationSettings;

    // Ribbon tab headers

    [ObservableProperty]
    private string _ribbonTabDrillingText = Resources.RibbonTabDrilling;

    [ObservableProperty]
    private string _ribbonTabPocketText = Resources.RibbonTabPocket;

    [ObservableProperty]
    private string _ribbonTabProfileText = Resources.RibbonTabProfile;

    [ObservableProperty]
    private string _ribbonTabOtherText = Resources.RibbonTabOther;

    // Main view panel labels
    [ObservableProperty]
    private string _operationPropertiesText = Resources.OperationProperties;

    [ObservableProperty]
    private string _preview2DText = Resources.Preview2D;

    [ObservableProperty]
    private string _operationsListText = Resources.OperationsList;

    [ObservableProperty]
    private string _gCodeText = Resources.GCode;

    [ObservableProperty]
    private string _primitives2DText = Resources.Primitives2D;

    [ObservableProperty]
    private string _pointButtonText = Resources.ButtonPoint;

    [ObservableProperty]
    private bool _isPointModeActive = false;

    [ObservableProperty]
    private string _lineButtonText = "Line";

    [ObservableProperty]
    private bool _isLineModeActive = false;

    [ObservableProperty]
    private string _ellipseButtonText = "Ellipse";

    [ObservableProperty]
    private bool _isEllipseModeActive = false;

    [ObservableProperty]
    private bool _isGridSnapEnabled = false;

    [ObservableProperty]
    private double _gridSnapStep = 1.0; // Default snap step: 1mm

    [ObservableProperty]
    private string _gridSnapButtonText = Resources.ButtonGridSnap;

    [ObservableProperty]
    private bool _isObjectSnapEnabled = false;

    [ObservableProperty]
    private string _objectSnapButtonText = Resources.ButtonObjectSnap;

    public double[] GridSnapSteps { get; } = new[] { 10.0, 5.0, 2.5, 1.0, 0.5, 0.1, 0.05, 0.01, 0.005, 0.001 };

    [ObservableProperty]
    private string _generateButtonText = Resources.ButtonGenerate;

    [ObservableProperty]
    private string _saveButtonText = Resources.ButtonSave;

    // 2D Primitives ViewModel
    public Primitives2DViewModel Primitives2DViewModel { get; }

    // Point Properties ViewModel
    public PointPropertiesViewModel PointPropertiesViewModel { get; }
    public LinePropertiesViewModel LinePropertiesViewModel { get; }
    public EllipsePropertiesViewModel EllipsePropertiesViewModel { get; }

    // Panel visibility controls
    [ObservableProperty]
    private bool _isLeftPanelVisible = true;

    [ObservableProperty]
    private bool _isRightPanelVisible = true;

    [ObservableProperty]
    private string _leftButtonContent = "<<";

    [ObservableProperty]
    private string _rightButtonContent = ">>";

    partial void OnIsLeftPanelVisibleChanged(bool oldValue, bool newValue)
    {
        OnPropertyChanged(nameof(LeftPanelWidth));
        OnPropertyChanged(nameof(LeftSplitterWidth));
    }

    partial void OnIsRightPanelVisibleChanged(bool oldValue, bool newValue)
    {
        OnPropertyChanged(nameof(RightPanelWidth));
        OnPropertyChanged(nameof(RightSplitterWidth));
    }

    // Column widths - collapse to 0 when panel is hidden
    public GridLength LeftPanelWidth => IsLeftPanelVisible ? new GridLength(1, GridUnitType.Star) : new GridLength(0);
    
    public GridLength RightPanelWidth => IsRightPanelVisible ? new GridLength(1, GridUnitType.Star) : new GridLength(0);
    
    public GridLength LeftSplitterWidth => IsLeftPanelVisible ? GridLength.Auto : new GridLength(0);
    
    public GridLength RightSplitterWidth => IsRightPanelVisible ? GridLength.Auto : new GridLength(0);

    // Operations list (using ViewModel wrapper)
    public ObservableCollection<OperationItemViewModel> Operations { get; } = new();

    // G-code lines list
    public ObservableCollection<string> GCodeLines { get; } = new();

    // 2D Preview ViewModel
    public Preview2DViewModel Preview2DViewModel { get; }

    [ObservableProperty]
    private OperationItemViewModel? _selectedOperation;

    partial void OnSelectedOperationChanged(OperationItemViewModel? oldValue, OperationItemViewModel? newValue)
    {
        UpdateCommandAvailability();
    }

    // Controls visibility of the ribbon tab content block.
    [ObservableProperty]
    private bool _isRibbonContentVisible = true;

    /// <summary>
    /// Updates the selection enabled state based on whether any creation mode is active.
    /// </summary>
    private void UpdateSelectionEnabled()
    {
        // Disable selection when any creation mode is active
        var anyModeActive = IsPointModeActive || IsLineModeActive || IsEllipseModeActive;
        Primitives2DViewModel.IsSelectionEnabled = !anyModeActive;
    }

    [RelayCommand]
    private void TogglePointMode()
    {
        IsPointModeActive = !IsPointModeActive;
    }

    [RelayCommand]
    private void ToggleLineMode()
    {
        IsLineModeActive = !IsLineModeActive;
        if (!IsLineModeActive)
        {
            _lineStartPoint = null; // Reset line creation state
        }
    }

    [RelayCommand]
    private void ToggleEllipseMode()
    {
        IsEllipseModeActive = !IsEllipseModeActive;
    }

    /// <summary>
    /// Adds a point primitive at the specified world coordinates.
    /// </summary>
    public void AddPointPrimitive(double x, double y)
    {
        var pointName = Resources.PrimitivePoint;
        var domainPoint = new Domain.Primitives.Point2DPrimitive(x, y, pointName);
        var pointViewModel = new Point2DViewModel(domainPoint);
        Primitives2DViewModel.Primitives.Add(pointViewModel);
        
        // Don't select the newly added point - keep current selection or no selection
        // This allows continuous point addition without changing selection
        
        // Keep point mode active for adding more points
        IsPointModeActive = true;
    }

    /// <summary>
    /// Gets the current line start point when in line creation mode, or null if not set.
    /// </summary>
    public Domain.Geometry.Point2D? LineStartPoint => _lineStartPoint;

    /// <summary>
    /// Handles click in line creation mode. First click sets start point, second click creates line.
    /// </summary>
    public void HandleLineModeClick(double x, double y)
    {
        if (!IsLineModeActive) return;

        var point = new Domain.Geometry.Point2D(x, y);

        if (_lineStartPoint == null)
        {
            // First click - set start point
            _lineStartPoint = point;
            OnPropertyChanged(nameof(LineStartPoint));
        }
        else
        {
            // Second click - create line
            AddLinePrimitive(_lineStartPoint.Value, point);
            _lineStartPoint = null; // Reset for next line
            OnPropertyChanged(nameof(LineStartPoint));
        }
    }

    /// <summary>
    /// Adds a line primitive between two points.
    /// </summary>
    public void AddLinePrimitive(Domain.Geometry.Point2D start, Domain.Geometry.Point2D end)
    {
        var lineName = Resources.PrimitiveLine;
        var domainLine = new Domain.Primitives.Line2DPrimitive(start, end, lineName);
        var lineViewModel = new Line2DViewModel(domainLine);
        Primitives2DViewModel.Primitives.Add(lineViewModel);
        
        // Keep line mode active for adding more lines
        IsLineModeActive = true;
    }

    /// <summary>
    /// Gets the current ellipse center when in ellipse creation mode, or null if not set.
    /// </summary>
    public Domain.Geometry.Point2D? EllipseCenter => _ellipseCenter;

    /// <summary>
    /// Gets the current ellipse rotation angle when in ellipse creation mode (determined after first click).
    /// </summary>
    public double EllipseRotationAngle => _ellipseRotationAngle;

    /// <summary>
    /// Gets the current ellipse major axis point when in ellipse creation mode, or null if not set.
    /// </summary>
    public Domain.Geometry.Point2D? EllipseMajorAxisPoint => _ellipseMajorAxisPoint;

    /// <summary>
    /// Updates the ellipse rotation angle based on current mouse position (called after first click).
    /// </summary>
    public void UpdateEllipseRotationAngle(double mouseX, double mouseY)
    {
        if (!IsEllipseModeActive || _ellipseCenter == null) return;

        var center = _ellipseCenter.Value;
        var mousePoint = new Domain.Geometry.Point2D(mouseX, mouseY);
        var vector = mousePoint - center;
        
        // Calculate rotation angle from center to mouse position
        _ellipseRotationAngle = Math.Atan2(vector.Y, vector.X);
        OnPropertyChanged(nameof(EllipseRotationAngle));
    }

    /// <summary>
    /// Handles click in ellipse creation mode. 
    /// First click sets center, second click sets major axis point, third click creates ellipse.
    /// </summary>
    public void HandleEllipseModeClick(double x, double y)
    {
        if (!IsEllipseModeActive) return;

        var point = new Domain.Geometry.Point2D(x, y);

        if (_ellipseCenter == null)
        {
            // First click - set center
            _ellipseCenter = point;
            OnPropertyChanged(nameof(EllipseCenter));
        }
        else if (_ellipseMajorAxisPoint == null)
        {
            // Second click - set major axis point using current rotation angle
            // The point is on the line defined by the rotation angle
            var center = _ellipseCenter.Value;
            var vector = point - center;
            
            // Project point onto the line defined by rotation angle
            var directionVector = new Domain.Geometry.Vector2D(Math.Cos(_ellipseRotationAngle), Math.Sin(_ellipseRotationAngle));
            var projectionLength = vector.X * directionVector.X + vector.Y * directionVector.Y;
            
            // Calculate major axis point
            var majorAxisPoint = center + directionVector * projectionLength;
            _ellipseMajorAxisPoint = majorAxisPoint;
            OnPropertyChanged(nameof(EllipseMajorAxisPoint));
        }
        else
        {
            // Third click - create ellipse
            var center = _ellipseCenter.Value;
            var majorAxisPoint = _ellipseMajorAxisPoint.Value;
            var minorAxisPoint = point;
            
            // Calculate radius X (distance from center to major axis point)
            var radiusX = center.DistanceTo(majorAxisPoint);
            
            // Calculate radius Y (distance from center to minor axis point, projected onto orthogonal line)
            // The minor axis point should be on a line orthogonal to the major axis
            var minorAxisVector = minorAxisPoint - center;
            
            // Orthogonal vector to major axis: rotate 90 degrees from rotation angle
            var orthogonalVector = new Domain.Geometry.Vector2D(
                -Math.Sin(_ellipseRotationAngle), 
                Math.Cos(_ellipseRotationAngle));
            
            // Project minor axis point onto orthogonal line
            var projectionLength = minorAxisVector.X * orthogonalVector.X + minorAxisVector.Y * orthogonalVector.Y;
            var radiusY = Math.Abs(projectionLength);
            
            // Use the rotation angle determined after first click
            AddEllipsePrimitive(center, radiusX, radiusY, _ellipseRotationAngle);
            
            // Reset for next ellipse
            _ellipseCenter = null;
            _ellipseMajorAxisPoint = null;
            _ellipseRotationAngle = 0;
            OnPropertyChanged(nameof(EllipseCenter));
            OnPropertyChanged(nameof(EllipseMajorAxisPoint));
            OnPropertyChanged(nameof(EllipseRotationAngle));
        }
    }

    /// <summary>
    /// Adds an ellipse primitive at the specified center with given radii and rotation.
    /// </summary>
    public void AddEllipsePrimitive(Domain.Geometry.Point2D center, double radiusX, double radiusY, double rotationAngle = 0)
    {
        var ellipseName = Resources.PrimitiveEllipse;
        var domainEllipse = new Domain.Primitives.Ellipse2DPrimitive(center, radiusX, radiusY, rotationAngle, ellipseName);
        var ellipseViewModel = new Ellipse2DViewModel(domainEllipse);
        Primitives2DViewModel.Primitives.Add(ellipseViewModel);
        
        // Keep ellipse mode active for adding more ellipses
        IsEllipseModeActive = true;
    }

    [ObservableProperty]
    private bool _isPointPropertiesVisible = false;

    [ObservableProperty]
    private bool _isLinePropertiesVisible = false;

    [ObservableProperty]
    private bool _isEllipsePropertiesVisible = false;

    /// <summary>
    /// Gets whether any primitive properties panel is visible.
    /// </summary>
    public bool IsAnyPrimitivePropertiesVisible => IsPointPropertiesVisible || IsLinePropertiesVisible || IsEllipsePropertiesVisible;

    partial void OnIsPointPropertiesVisibleChanged(bool value)
    {
        OnPropertyChanged(nameof(IsAnyPrimitivePropertiesVisible));
    }

    partial void OnIsLinePropertiesVisibleChanged(bool value)
    {
        OnPropertyChanged(nameof(IsAnyPrimitivePropertiesVisible));
    }

    partial void OnIsEllipsePropertiesVisibleChanged(bool value)
    {
        OnPropertyChanged(nameof(IsAnyPrimitivePropertiesVisible));
    }

    private void UpdatePrimitiveProperties()
    {
        var selectedPrimitive = Primitives2DViewModel.SelectedPrimitive;
        
        // Update point properties visibility - only show when point is selected, not in creation mode
        if (selectedPrimitive is Point2DViewModel selectedPointViewModel)
        {
            PointPropertiesViewModel.SetSelectedPointViewModel(selectedPointViewModel);
            IsPointPropertiesVisible = true;
        }
        else
        {
            PointPropertiesViewModel.SetSelectedPointViewModel(null);
            IsPointPropertiesVisible = false;
        }
        
        // Update line properties visibility
        if (selectedPrimitive is Line2DViewModel selectedLineViewModel)
        {
            LinePropertiesViewModel.SetSelectedLineViewModel(selectedLineViewModel);
            IsLinePropertiesVisible = true;
        }
        else
        {
            LinePropertiesViewModel.SetSelectedLineViewModel(null);
            IsLinePropertiesVisible = false;
        }
        
        // Update ellipse properties visibility
        if (selectedPrimitive is Ellipse2DViewModel selectedEllipseViewModel)
        {
            EllipsePropertiesViewModel.SetSelectedEllipseViewModel(selectedEllipseViewModel);
            IsEllipsePropertiesVisible = true;
        }
        else
        {
            EllipsePropertiesViewModel.SetSelectedEllipseViewModel(null);
            IsEllipsePropertiesVisible = false;
        }
    }

    [RelayCommand]
    private void OpenApplicationSettings()
    {
        var settingsWindow = new ApplicationSettingsWindow
        {
            DataContext = new Settings.ApplicationSettingsViewModel(_settingsService)
        };

        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop && desktop.MainWindow != null)
        {
            settingsWindow.Icon = desktop.MainWindow.Icon;
            settingsWindow.ShowDialog(desktop.MainWindow);
        }
    }

    [RelayCommand]
    private void OpenCodeGenerationSettings()
    {
        var settingsWindow = new CodeGenerationSettingsWindow(_settingsService);

        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop && desktop.MainWindow != null)
        {
            settingsWindow.Icon = desktop.MainWindow.Icon;
            settingsWindow.ShowDialog(desktop.MainWindow);
        }
    }

    private bool CanMoveOperationUp()
    {
        if (SelectedOperation == null) return false;
        var index = Operations.IndexOf(SelectedOperation);
        return index > 0;
    }

    [RelayCommand(CanExecute = nameof(CanMoveOperationUp))]
    private void MoveOperationUp()
    {
        if (SelectedOperation == null) return;
        
        var index = Operations.IndexOf(SelectedOperation);
        if (index > 0)
        {
            var operationToMove = SelectedOperation;
            Operations.Move(index, index - 1);
            // Keep selection on the moved operation
            SelectedOperation = operationToMove;
            UpdateCommandAvailability();
        }
    }

    private bool CanMoveOperationDown()
    {
        if (SelectedOperation == null) return false;
        var index = Operations.IndexOf(SelectedOperation);
        return index < Operations.Count - 1;
    }

    [RelayCommand(CanExecute = nameof(CanMoveOperationDown))]
    private void MoveOperationDown()
    {
        if (SelectedOperation == null) return;
        
        var index = Operations.IndexOf(SelectedOperation);
        if (index < Operations.Count - 1)
        {
            var operationToMove = SelectedOperation;
            Operations.Move(index, index + 1);
            // Keep selection on the moved operation
            SelectedOperation = operationToMove;
            UpdateCommandAvailability();
        }
    }

    private bool CanDeleteOperation()
    {
        return SelectedOperation != null;
    }

    [RelayCommand(CanExecute = nameof(CanDeleteOperation))]
    private void DeleteOperation()
    {
        if (SelectedOperation == null) return;
        
        Operations.Remove(SelectedOperation);
        SelectedOperation = null;
    }

    private void UpdateCommandAvailability()
    {
        MoveOperationUpCommand.NotifyCanExecuteChanged();
        MoveOperationDownCommand.NotifyCanExecuteChanged();
        DeleteOperationCommand.NotifyCanExecuteChanged();
    }

    private bool CanSaveGCode()
    {
        return GCodeLines.Count > 0;
    }

    private void UpdateGCodeCommandAvailability()
    {
        SaveGCodeCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    private void GenerateGCode()
    {
        GCodeLines.Clear();

        // Get all settings needed for code generation
        var appSettings = _settingsService.Current;
        
        // Create generator and generate G-code
        // Generation will work even if there are no operations (will generate header and end)
        var generator = new GCodeGenerator(appSettings.CodeGeneration, appSettings.Spindle, appSettings.Coolant);
        var generatedLines = generator.Generate(Operations);
        
        // Add generated lines to collection
        foreach (var line in generatedLines)
        {
            GCodeLines.Add(line);
        }
    }

    [RelayCommand]
    private void ToggleLeftPanel()
    {
        IsLeftPanelVisible = !IsLeftPanelVisible;
        LeftButtonContent = IsLeftPanelVisible ? "<<" : ">>";
    }

    [RelayCommand]
    private void ToggleRightPanel()
    {
        IsRightPanelVisible = !IsRightPanelVisible;
        RightButtonContent = IsRightPanelVisible ? ">>" : "<<";
    }

    [RelayCommand(CanExecute = nameof(CanSaveGCode))]
    private async void SaveGCode()
    {
        if (GCodeLines.Count == 0) return;

        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop && desktop.MainWindow != null)
        {
            var topLevel = TopLevel.GetTopLevel(desktop.MainWindow);
            if (topLevel?.StorageProvider is { } storageProvider)
            {
                var file = await storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
                {
                    Title = Resources.ButtonSave,
                    SuggestedFileName = "program.nc",
                    FileTypeChoices = new[]
                    {
                        new FilePickerFileType("G-code files")
                        {
                            Patterns = new[] { "*.nc", "*.ncc", "*.ngc", "*.tap", "*.gcode", "*.txt" }
                        }
                    }
                });

                if (file != null)
                {
                    try
                    {
                        await using var stream = await file.OpenWriteAsync();
                        // Use UTF-8 encoding without BOM for G-code files (cross-platform compatible)
                        using var writer = new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
                        foreach (var line in GCodeLines)
                        {
                            await writer.WriteLineAsync(line);
                        }
                        await writer.FlushAsync();
                    }
                    catch (Exception ex)
                    {
                        // In a real application, you might want to show an error dialog
                        System.Diagnostics.Debug.WriteLine($"Error saving file: {ex.Message}");
                    }
                }
            }
        }
    }
}

/// <summary>
/// Represents a single operation item in the operations list.
/// </summary>
public partial class OperationItem : ObservableObject
{
    /// <summary>
    /// Gets or sets a value indicating whether the operation is enabled.
    /// </summary>
    [ObservableProperty]
    private bool _isEnabled = true;

    /// <summary>
    /// Gets or sets the operation name.
    /// </summary>
    [ObservableProperty]
    private string _name;

    /// <summary>
    /// Initializes a new instance of the OperationItem class.
    /// </summary>
    /// <param name="name">The operation name.</param>
    public OperationItem(string name)
    {
        _name = name;
    }
}
