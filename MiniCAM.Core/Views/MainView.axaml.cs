using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using MiniCAM.Core.ViewModels;

namespace MiniCAM.Core.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
    }

    private void RibbonTabs_OnDoubleTapped(object? sender, TappedEventArgs e)
    {
        // Ignore double-clicks coming from buttons inside tab content.
        if (e.Source is Control control &&
            (control is Button || control.FindAncestorOfType<Button>() is not null))
        {
            return;
        }

        if (DataContext is MainViewModel vm)
        {
            vm.IsRibbonContentVisible = !vm.IsRibbonContentVisible;
        }
    }
}