using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MiniCAM.Core.Localization;
using MiniCAM.Core.Settings;

namespace MiniCAM.Core.ViewModels;

/// <summary>
/// Main coordinator ViewModel for code generation settings window.
/// Manages three tab ViewModels: CodeGenerationTabViewModel, SpindleSettingsViewModel, and CoolantSettingsViewModel.
/// </summary>
public partial class CodeGenerationSettingsViewModel : ViewModelBase, IDisposable
{
    public CodeGenerationTabViewModel CodeGenerationTab { get; }
    public SpindleSettingsViewModel SpindleSettings { get; }
    public CoolantSettingsViewModel CoolantSettings { get; }

    [ObservableProperty]
    private string _applyButtonText = Resources.ButtonApply;

    [ObservableProperty]
    private string _resetButtonText = Resources.ButtonReset;

    public CodeGenerationSettingsViewModel()
    {
        CodeGenerationTab = new CodeGenerationTabViewModel();
        SpindleSettings = new SpindleSettingsViewModel();
        CoolantSettings = new CoolantSettingsViewModel();
        
        Resources.CultureChanged += OnCultureChanged;
        UpdateLocalizedStrings();
    }

    private void OnCultureChanged(object? sender, CultureChangedEventArgs e)
    {
        UpdateLocalizedStrings();
    }

    private void UpdateLocalizedStrings()
    {
        ApplyButtonText = Resources.ButtonApply;
        ResetButtonText = Resources.ButtonReset;
    }

    [RelayCommand]
    private void Apply()
    {
        var settings = SettingsManager.Current;

        CodeGenerationTab.SaveToSettings(settings);
        SpindleSettings.SaveToSettings(settings);
        CoolantSettings.SaveToSettings(settings);

        SettingsManager.SaveCurrent();

        // Reload from settings to update original values (settings now contain the saved values)
        CodeGenerationTab.LoadFromSettings(settings);
        SpindleSettings.LoadFromSettings(settings);
        CoolantSettings.LoadFromSettings(settings);
    }

    [RelayCommand]
    private void Reset()
    {
        SettingsManager.Reload();
        var settings = SettingsManager.Current;

        CodeGenerationTab.LoadFromSettings(settings);
        SpindleSettings.LoadFromSettings(settings);
        CoolantSettings.LoadFromSettings(settings);

        CodeGenerationTab.ResetToOriginal();
        SpindleSettings.ResetToOriginal();
        CoolantSettings.ResetToOriginal();
    }

    public void Dispose()
    {
        Resources.CultureChanged -= OnCultureChanged;
        CodeGenerationTab?.Dispose();
        SpindleSettings?.Dispose();
        CoolantSettings?.Dispose();
    }
}

// Option classes used by tab ViewModels
public partial class SpindleEnableCommandOption : OptionBase
{
    public SpindleEnableCommandOption(string key, string displayName)
        : base(key, displayName)
    {
    }
}

public partial class SpindleDelayParameterOption : OptionBase
{
    public SpindleDelayParameterOption(string key, string displayName)
        : base(key, displayName)
    {
    }
}

public partial class CoordinateSystemOption : OptionBase
{
    public CoordinateSystemOption(string key, string displayName)
        : base(key, displayName)
    {
    }
}
