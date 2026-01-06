using Avalonia.Controls;
using Localization = MiniCAM.Core.Localization;
using MiniCAM.Core.ViewModels;

namespace MiniCAM.Core.Views;

public partial class CodeGenerationSettingsWindow : Window
{
    public CodeGenerationSettingsWindow()
    {
        InitializeComponent();
        Title = Localization.Resources.CodeGenerationSettingsTitle;
        DataContext = new CodeGenerationSettingsViewModel();
        Closed += OnClosed;
        
        // Set tab headers text
        if (this.FindControl<TextBlock>("TabCodeGenerationText") is { } codeGenTab)
        {
            codeGenTab.Text = Localization.Resources.TabCodeGeneration;
        }
        
        if (this.FindControl<TextBlock>("TabSpindleText") is { } spindleTab)
        {
            spindleTab.Text = Localization.Resources.TabSpindle;
        }
        
        if (this.FindControl<TextBlock>("TabCoolantText") is { } coolantTab)
        {
            coolantTab.Text = Localization.Resources.TabCoolant;
        }
    }

    private void OnClosed(object? sender, System.EventArgs e)
    {
        if (DataContext is CodeGenerationSettingsViewModel viewModel)
        {
            viewModel.Dispose();
        }
    }
}

