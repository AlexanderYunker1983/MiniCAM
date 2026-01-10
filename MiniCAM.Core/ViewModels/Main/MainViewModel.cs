using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
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
using MiniCAM.Core.ViewModels.Main;
using MiniCAM.Core.Views;

namespace MiniCAM.Core.ViewModels;

/// <summary>
/// Main view model for the application window.
/// Manages operations list, ribbon tabs, and application settings dialogs.
/// </summary>
public partial class MainViewModel : LocalizedViewModelBase
{
    private readonly ISettingsService _settingsService;

    public MainViewModel(ISettingsService settingsService)
    {
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
        
        // Initialize 2D Preview ViewModel
        Preview2DViewModel = new Preview2DViewModel();
        
        // Initialize 2D Primitives ViewModel
        Primitives2DViewModel = new Primitives2DViewModel();
        
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
        foreach (var operation in Operations)
        {
            Preview2DViewModel.Operations.Add(operation);
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
    public GridLength LeftPanelWidth => IsLeftPanelVisible ? new GridLength(2, GridUnitType.Star) : new GridLength(0);
    
    public GridLength RightPanelWidth => IsRightPanelVisible ? new GridLength(2, GridUnitType.Star) : new GridLength(0);
    
    public GridLength LeftSplitterWidth => IsLeftPanelVisible ? GridLength.Auto : new GridLength(0);
    
    public GridLength RightSplitterWidth => IsRightPanelVisible ? GridLength.Auto : new GridLength(0);

    // Operations list
    public ObservableCollection<OperationItem> Operations { get; } = new();

    // G-code lines list
    public ObservableCollection<string> GCodeLines { get; } = new();

    // 2D Preview ViewModel
    public Preview2DViewModel Preview2DViewModel { get; }

    [ObservableProperty]
    private OperationItem? _selectedOperation;

    partial void OnSelectedOperationChanged(OperationItem? oldValue, OperationItem? newValue)
    {
        UpdateCommandAvailability();
    }

    // Controls visibility of the ribbon tab content block.
    [ObservableProperty]
    private bool _isRibbonContentVisible = true;

    [RelayCommand]
    private void TogglePointMode()
    {
        IsPointModeActive = !IsPointModeActive;
    }

    /// <summary>
    /// Adds a point primitive at the specified world coordinates.
    /// </summary>
    public void AddPointPrimitive(double x, double y)
    {
        var pointName = Resources.PrimitivePoint;
        var point = new Point2DPrimitive(x, y, pointName);
        Primitives2DViewModel.Primitives.Add(point);
    }

    [RelayCommand]
    private void OpenApplicationSettings()
    {
        var settingsWindow = new ApplicationSettingsWindow
        {
            DataContext = new ApplicationSettingsViewModel(_settingsService)
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
