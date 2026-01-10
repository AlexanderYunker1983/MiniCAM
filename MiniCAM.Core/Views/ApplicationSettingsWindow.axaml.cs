using Avalonia.Controls;
using ApplicationSettingsViewModel = MiniCAM.Core.ViewModels.Settings.ApplicationSettingsViewModel;

namespace MiniCAM.Core.Views;

public partial class ApplicationSettingsWindow : Window
{
    public ApplicationSettingsWindow()
    {
        InitializeComponent();
        Closed += OnClosed;
    }

    private void OnClosed(object? sender, System.EventArgs e)
    {
        if (DataContext is ApplicationSettingsViewModel viewModel)
        {
            viewModel.Dispose();
        }
    }
}

