using Avalonia.Controls;

namespace MiniCAM.Core.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        // Set window title from resources
        UpdateWindowTitle();
        
        // Subscribe to culture change events to update UI dynamically
        Localization.Resources.CultureChanged += OnCultureChanged;
    }

    private void OnCultureChanged(object? sender, Localization.CultureChangedEventArgs e)
    {
        // Update window title when culture changes
        UpdateWindowTitle();
    }

    private void UpdateWindowTitle()
    {
        Title = Localization.Resources.WindowTitle;
    }
}