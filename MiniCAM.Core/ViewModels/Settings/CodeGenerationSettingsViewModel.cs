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
public partial class CodeGenerationSettingsViewModel : LocalizedViewModelBase
{
    private readonly ISettingsService _settingsService;
    public CodeGenerationTabViewModel CodeGenerationTab { get; }
    public SpindleSettingsViewModel SpindleSettings { get; }
    public CoolantSettingsViewModel CoolantSettings { get; }

    [ObservableProperty]
    private string _applyButtonText = Resources.ButtonApply;

    [ObservableProperty]
    private string _resetButtonText = Resources.ButtonReset;

    public CodeGenerationSettingsViewModel(ISettingsService settingsService)
    {
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
        CodeGenerationTab = new CodeGenerationTabViewModel(_settingsService);
        SpindleSettings = new SpindleSettingsViewModel(_settingsService);
        CoolantSettings = new CoolantSettingsViewModel(_settingsService);
    }

    protected override void UpdateLocalizedStrings()
    {
        ApplyButtonText = Resources.ButtonApply;
        ResetButtonText = Resources.ButtonReset;
    }

    [RelayCommand]
    private void Apply()
    {
        var settings = _settingsService.Current;

        CodeGenerationTab.SaveToSettings(settings);
        SpindleSettings.SaveToSettings(settings);
        CoolantSettings.SaveToSettings(settings);

        _settingsService.SaveCurrent();

        // Reload from settings to update original values (settings now contain the saved values)
        CodeGenerationTab.LoadFromSettings(settings);
        SpindleSettings.LoadFromSettings(settings);
        CoolantSettings.LoadFromSettings(settings);
    }

    [RelayCommand]
    private void Reset()
    {
        _settingsService.Reload();
        var settings = _settingsService.Current;

        CodeGenerationTab.LoadFromSettings(settings);
        SpindleSettings.LoadFromSettings(settings);
        CoolantSettings.LoadFromSettings(settings);

        CodeGenerationTab.ResetToOriginal();
        SpindleSettings.ResetToOriginal();
        CoolantSettings.ResetToOriginal();
    }

    public override void Dispose()
    {
        base.Dispose();
        CodeGenerationTab?.Dispose();
        SpindleSettings?.Dispose();
        CoolantSettings?.Dispose();
    }
}
