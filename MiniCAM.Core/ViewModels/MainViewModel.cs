using CommunityToolkit.Mvvm.ComponentModel;

namespace MiniCAM.Core.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _greeting = "Welcome to Avalonia!";
}
