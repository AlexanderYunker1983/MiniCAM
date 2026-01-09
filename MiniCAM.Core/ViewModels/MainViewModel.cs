using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MiniCAM.Core.Localization;
using MiniCAM.Core.Views;

namespace MiniCAM.Core.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public MainViewModel()
    {
        // Subscribe to culture changes to update localized strings
        Resources.CultureChanged += OnCultureChanged;
        UpdateLocalizedStrings();
        
        // Subscribe to collection changes to update command availability
        Operations.CollectionChanged += (s, e) => UpdateCommandAvailability();
        
        // Initialize with sample operations
        Operations.Add(new OperationItem("Drilling Operation 1"));
        Operations.Add(new OperationItem("Drilling Operation 2"));
        Operations.Add(new OperationItem("Pocket Operation 1"));
    }

    private void OnCultureChanged(object? sender, CultureChangedEventArgs e)
    {
        UpdateLocalizedStrings();
    }

    private void UpdateLocalizedStrings()
    {
        MenuSettingsText = Resources.MenuSettings;
        MenuApplicationSettingsText = Resources.MenuApplicationSettings;
        MenuCodeGenerationSettingsText = Resources.MenuCodeGenerationSettings;
        RibbonTabDrillingText = Resources.RibbonTabDrilling;
        RibbonTabPocketText = Resources.RibbonTabPocket;
        RibbonTabProfileText = Resources.RibbonTabProfile;
        RibbonTabOtherText = Resources.RibbonTabOther;
        OperationPropertiesText = Resources.OperationProperties;
        Preview2DText = Resources.Preview2D;
        OperationsListText = Resources.OperationsList;
        GCodeText = Resources.GCode;
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

    // Operations list
    public ObservableCollection<OperationItem> Operations { get; } = new();

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
    private void OpenApplicationSettings()
    {
        var settingsWindow = new ApplicationSettingsWindow
        {
            DataContext = new ApplicationSettingsViewModel()
        };

        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            settingsWindow.Icon = desktop.MainWindow?.Icon;
            settingsWindow.ShowDialog(desktop.MainWindow);
        }
    }

    [RelayCommand]
    private void OpenCodeGenerationSettings()
    {
        var settingsWindow = new CodeGenerationSettingsWindow();

        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            settingsWindow.Icon = desktop.MainWindow?.Icon;
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
}

public partial class OperationItem : ObservableObject
{
    [ObservableProperty]
    private bool _isEnabled = true;

    [ObservableProperty]
    private string _name;

    public OperationItem(string name)
    {
        _name = name;
    }
}
