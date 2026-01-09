using System;
using System.Collections.Generic;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using MiniCAM.Core.Localization;
using MiniCAM.Core.Settings;

namespace MiniCAM.Core.ViewModels;

/// <summary>
/// View model for the Coolant settings tab.
/// Manages coolant control options such as enable/disable at start/end.
/// </summary>
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

    // Property headers dictionary for change tracking
    private readonly Dictionary<string, PropertyHeaderViewModel> _headers = new();

    private const string PropertyAddCoolantCode = nameof(AddCoolantCode);
    private const string PropertyEnableCoolantAtStart = nameof(EnableCoolantAtStart);
    private const string PropertyDisableCoolantAtEnd = nameof(DisableCoolantAtEnd);

    private readonly ISettingsService _settingsService;

    /// <summary>
    /// Gets the header view model for AddCoolantCode property.
    /// </summary>
    public PropertyHeaderViewModel CoolantAddCodeHeader => _headers[PropertyAddCoolantCode];

    /// <summary>
    /// Gets the header view model for EnableCoolantAtStart property.
    /// </summary>
    public PropertyHeaderViewModel CoolantEnableAtStartHeader => _headers[PropertyEnableCoolantAtStart];

    /// <summary>
    /// Gets the header view model for DisableCoolantAtEnd property.
    /// </summary>
    public PropertyHeaderViewModel CoolantDisableAtEndHeader => _headers[PropertyDisableCoolantAtEnd];

    public CoolantSettingsViewModel(ISettingsService settingsService)
    {
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
        
        // Initialize headers
        _headers[PropertyAddCoolantCode] = new PropertyHeaderViewModel(Resources.CoolantAddCode);
        _headers[PropertyEnableCoolantAtStart] = new PropertyHeaderViewModel(Resources.CoolantEnableAtStart);
        _headers[PropertyDisableCoolantAtEnd] = new PropertyHeaderViewModel(Resources.CoolantDisableAtEnd);
        
        LoadFromSettings(_settingsService.Current);
        RegisterTrackedProperties();
        UpdateCoolantDependentControls();
        HeaderTracker.UpdateAllHeadersImmediate();
    }

    /// <summary>
    /// Registers all properties for change tracking.
    /// </summary>
    private void RegisterTrackedProperties()
    {
        RegisterProperty(PropertyAddCoolantCode, AddCoolantCode, () => Resources.CoolantAddCode);
        RegisterProperty(PropertyEnableCoolantAtStart, EnableCoolantAtStart, () => Resources.CoolantEnableAtStart);
        RegisterProperty(PropertyDisableCoolantAtEnd, DisableCoolantAtEnd, () => Resources.CoolantDisableAtEnd);
    }

    /// <summary>
    /// Helper method to register a simple property for tracking with its header.
    /// </summary>
    private void RegisterProperty<T>(string propertyName, T value, Func<string> getResourceString)
    {
        HeaderTracker.Register(propertyName, value, getResourceString, _headers[propertyName]);
    }

    protected override void UpdateLocalizedStrings()
    {
        HeaderTracker.UpdateAllHeaders();
    }

    partial void OnAddCoolantCodeChanged(bool value)
    {
        UpdateCoolantDependentControls();
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

    /// <summary>
    /// Updates the enabled state of dependent controls based on current property values.
    /// </summary>
    private void UpdateCoolantDependentControls()
    {
        IsCoolantSubOptionsEnabled = AddCoolantCode;
    }

    public override void LoadFromSettings(AppSettings settings)
    {
        LoadBoolProperty(settings, s => s.AddCoolantCode, CoolantDefaults.AddCoolantCode, PropertyAddCoolantCode, v => AddCoolantCode = v);
        LoadBoolProperty(settings, s => s.EnableCoolantAtStart, CoolantDefaults.EnableCoolantAtStart, PropertyEnableCoolantAtStart, v => EnableCoolantAtStart = v);
        LoadBoolProperty(settings, s => s.DisableCoolantAtEnd, CoolantDefaults.DisableCoolantAtEnd, PropertyDisableCoolantAtEnd, v => DisableCoolantAtEnd = v);
    }

    public override void SaveToSettings(AppSettings settings)
    {
        settings.AddCoolantCode = AddCoolantCode;
        settings.EnableCoolantAtStart = EnableCoolantAtStart;
        settings.DisableCoolantAtEnd = DisableCoolantAtEnd;
    }

    public override void ResetToOriginal()
    {
        ResetBoolProperty(PropertyAddCoolantCode, CoolantDefaults.AddCoolantCode, v => AddCoolantCode = v);
        ResetBoolProperty(PropertyEnableCoolantAtStart, CoolantDefaults.EnableCoolantAtStart, v => EnableCoolantAtStart = v);
        ResetBoolProperty(PropertyDisableCoolantAtEnd, CoolantDefaults.DisableCoolantAtEnd, v => DisableCoolantAtEnd = v);
        UpdateCoolantDependentControls();
        HeaderTracker.UpdateAllHeadersImmediate();
    }
}
