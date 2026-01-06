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
        // Subscribe to culture changes to update menu items
        Resources.CultureChanged += OnCultureChanged;
        UpdateMenuStrings();
    }

    private void OnCultureChanged(object? sender, CultureChangedEventArgs e)
    {
        UpdateMenuStrings();
    }

    private void UpdateMenuStrings()
    {
        MenuSettingsText = Resources.MenuSettings;
        MenuApplicationSettingsText = Resources.MenuApplicationSettings;
    }

    [ObservableProperty]
    private string _menuSettingsText = Resources.MenuSettings;

    [ObservableProperty]
    private string _menuApplicationSettingsText = Resources.MenuApplicationSettings;

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
            settingsWindow.Show(desktop.MainWindow);
        }
        else
        {
            settingsWindow.Show();
        }
    }
}
