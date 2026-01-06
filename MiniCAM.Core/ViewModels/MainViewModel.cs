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
    }

    private void OnCultureChanged(object? sender, CultureChangedEventArgs e)
    {
        UpdateLocalizedStrings();
    }

    private void UpdateLocalizedStrings()
    {
        MenuSettingsText = Resources.MenuSettings;
        MenuApplicationSettingsText = Resources.MenuApplicationSettings;
        RibbonTabDrillingText = Resources.RibbonTabDrilling;
        RibbonTabPocketText = Resources.RibbonTabPocket;
        RibbonTabProfileText = Resources.RibbonTabProfile;
        RibbonTabOtherText = Resources.RibbonTabOther;
    }

    [ObservableProperty]
    private string _menuSettingsText = Resources.MenuSettings;

    [ObservableProperty]
    private string _menuApplicationSettingsText = Resources.MenuApplicationSettings;

    // Ribbon tab headers

    [ObservableProperty]
    private string _ribbonTabDrillingText = Resources.RibbonTabDrilling;

    [ObservableProperty]
    private string _ribbonTabPocketText = Resources.RibbonTabPocket;

    [ObservableProperty]
    private string _ribbonTabProfileText = Resources.RibbonTabProfile;

    [ObservableProperty]
    private string _ribbonTabOtherText = Resources.RibbonTabOther;

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
}
