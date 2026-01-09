using System;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using MiniCAM.Core.Localization;
using MiniCAM.Core.Settings;

namespace MiniCAM.Core.ViewModels;

public partial class CoolantSettingsViewModel : SettingsTabViewModelBase
{
    [ObservableProperty]
    private bool _addCoolantCode;

    [ObservableProperty]
    private bool _enableCoolantAtStart;

    [ObservableProperty]
    private bool _disableCoolantAtEnd;

    [ObservableProperty]
    private bool _isCoolantSubOptionsEnabled;

    // Header text and font style properties
    [ObservableProperty]
    private string _coolantAddCodeHeaderText = Resources.CoolantAddCode;

    [ObservableProperty]
    private FontStyle _coolantAddCodeFontStyle = FontStyle.Normal;

    [ObservableProperty]
    private string _coolantEnableAtStartHeaderText = Resources.CoolantEnableAtStart;

    [ObservableProperty]
    private FontStyle _coolantEnableAtStartFontStyle = FontStyle.Normal;

    [ObservableProperty]
    private string _coolantDisableAtEndHeaderText = Resources.CoolantDisableAtEnd;

    [ObservableProperty]
    private FontStyle _coolantDisableAtEndFontStyle = FontStyle.Normal;

    private const string PropertyAddCoolantCode = nameof(AddCoolantCode);
    private const string PropertyEnableCoolantAtStart = nameof(EnableCoolantAtStart);
    private const string PropertyDisableCoolantAtEnd = nameof(DisableCoolantAtEnd);

    public CoolantSettingsViewModel()
    {
        LoadFromSettings(SettingsManager.Current);
        RegisterTrackedProperties();
        UpdateCoolantSubOptionsEnabled();
        HeaderTracker.UpdateAllHeaders();
    }

    private void RegisterTrackedProperties()
    {
        HeaderTracker.Register(
            PropertyAddCoolantCode,
            AddCoolantCode,
            () => Resources.CoolantAddCode,
            value => CoolantAddCodeHeaderText = value,
            value => CoolantAddCodeFontStyle = value);

        HeaderTracker.Register(
            PropertyEnableCoolantAtStart,
            EnableCoolantAtStart,
            () => Resources.CoolantEnableAtStart,
            value => CoolantEnableAtStartHeaderText = value,
            value => CoolantEnableAtStartFontStyle = value);

        HeaderTracker.Register(
            PropertyDisableCoolantAtEnd,
            DisableCoolantAtEnd,
            () => Resources.CoolantDisableAtEnd,
            value => CoolantDisableAtEndHeaderText = value,
            value => CoolantDisableAtEndFontStyle = value);
    }

    protected override void UpdateLocalizedStrings()
    {
        HeaderTracker.UpdateAllHeaders();
    }

    partial void OnAddCoolantCodeChanged(bool value)
    {
        UpdateCoolantSubOptionsEnabled();
        HeaderTracker.Update(PropertyAddCoolantCode, value);
    }

    partial void OnEnableCoolantAtStartChanged(bool value)
    {
        HeaderTracker.Update(PropertyEnableCoolantAtStart, value);
    }

    partial void OnDisableCoolantAtEndChanged(bool value)
    {
        HeaderTracker.Update(PropertyDisableCoolantAtEnd, value);
    }

    private void UpdateCoolantSubOptionsEnabled()
    {
        IsCoolantSubOptionsEnabled = AddCoolantCode;
    }

    public override void LoadFromSettings(AppSettings settings)
    {
        var addCoolantCode = settings.AddCoolantCode ?? CoolantDefaults.AddCoolantCode;
        var enableCoolantAtStart = settings.EnableCoolantAtStart ?? CoolantDefaults.EnableCoolantAtStart;
        var disableCoolantAtEnd = settings.DisableCoolantAtEnd ?? CoolantDefaults.DisableCoolantAtEnd;

        AddCoolantCode = addCoolantCode;
        EnableCoolantAtStart = enableCoolantAtStart;
        DisableCoolantAtEnd = disableCoolantAtEnd;

        // Update original values in tracker
        HeaderTracker.UpdateOriginal(PropertyAddCoolantCode, addCoolantCode);
        HeaderTracker.UpdateOriginal(PropertyEnableCoolantAtStart, enableCoolantAtStart);
        HeaderTracker.UpdateOriginal(PropertyDisableCoolantAtEnd, disableCoolantAtEnd);
    }

    public override void SaveToSettings(AppSettings settings)
    {
        settings.AddCoolantCode = AddCoolantCode;
        settings.EnableCoolantAtStart = EnableCoolantAtStart;
        settings.DisableCoolantAtEnd = DisableCoolantAtEnd;
    }

    public override void ResetToOriginal()
    {
        AddCoolantCode = HeaderTracker.GetOriginalValue<bool>(PropertyAddCoolantCode);
        EnableCoolantAtStart = HeaderTracker.GetOriginalValue<bool>(PropertyEnableCoolantAtStart);
        DisableCoolantAtEnd = HeaderTracker.GetOriginalValue<bool>(PropertyDisableCoolantAtEnd);
        UpdateCoolantSubOptionsEnabled();
        HeaderTracker.UpdateAllHeaders();
    }
}
