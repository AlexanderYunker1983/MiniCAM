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
    }

    protected override void UpdateLocalizedStrings()
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

    public override void Dispose()
    {
        base.Dispose();
        CodeGenerationTab?.Dispose();
        SpindleSettings?.Dispose();
        CoolantSettings?.Dispose();
    }
}

// Option classes used by tab ViewModels

/// <summary>
/// Represents a spindle enable command option (M3, M4).
/// </summary>
public partial class SpindleEnableCommandOption : OptionBase
{
    /// <summary>
    /// Initializes a new instance of the SpindleEnableCommandOption class.
    /// </summary>
    /// <param name="key">The command key (e.g., "M3", "M4").</param>
    /// <param name="displayName">The localized display name.</param>
    public SpindleEnableCommandOption(string key, string displayName)
        : base(key, displayName)
    {
    }
}

/// <summary>
/// Represents a spindle delay parameter option (F, P, Pxx.).
/// </summary>
public partial class SpindleDelayParameterOption : OptionBase
{
    /// <summary>
    /// Initializes a new instance of the SpindleDelayParameterOption class.
    /// </summary>
    /// <param name="key">The parameter key (e.g., "F", "P", "Pxx.").</param>
    /// <param name="displayName">The localized display name.</param>
    public SpindleDelayParameterOption(string key, string displayName)
        : base(key, displayName)
    {
    }
}

/// <summary>
/// Represents a work coordinate system option (G54-G59).
/// </summary>
public partial class CoordinateSystemOption : OptionBase
{
    /// <summary>
    /// Initializes a new instance of the CoordinateSystemOption class.
    /// </summary>
    /// <param name="key">The coordinate system key (e.g., "G54", "G55").</param>
    /// <param name="displayName">The localized display name.</param>
    public CoordinateSystemOption(string key, string displayName)
        : base(key, displayName)
    {
    }
}
