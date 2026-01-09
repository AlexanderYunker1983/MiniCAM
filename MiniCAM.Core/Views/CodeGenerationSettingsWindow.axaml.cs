using System;
using Avalonia.Controls;
using Localization = MiniCAM.Core.Localization;
using MiniCAM.Core.Localization;
using MiniCAM.Core.Settings;
using MiniCAM.Core.ViewModels;

namespace MiniCAM.Core.Views;

public partial class CodeGenerationSettingsWindow : Window
{
    public CodeGenerationSettingsWindow(ISettingsService settingsService)
    {
        if (settingsService == null)
            throw new ArgumentNullException(nameof(settingsService));
            
        InitializeComponent();
        Title = Localization.Resources.CodeGenerationSettingsTitle;
        DataContext = new CodeGenerationSettingsViewModel(settingsService);
        Closed += OnClosed;
        
        // Subscribe to culture changes to update tab headers
        Localization.Resources.CultureChanged += OnCultureChanged;
        
        // Set initial tab headers text
        UpdateTabHeaders();
    }

    private void OnCultureChanged(object? sender, CultureChangedEventArgs e)
    {
        Title = Localization.Resources.CodeGenerationSettingsTitle;
        UpdateTabHeaders();
    }

    private void UpdateTabHeaders()
    {
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
        Localization.Resources.CultureChanged -= OnCultureChanged;
        if (DataContext is CodeGenerationSettingsViewModel viewModel)
        {
            viewModel.Dispose();
        }
    }
}

