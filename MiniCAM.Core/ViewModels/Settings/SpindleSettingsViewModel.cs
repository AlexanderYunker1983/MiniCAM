using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using MiniCAM.Core.Localization;
using MiniCAM.Core.Settings;

namespace MiniCAM.Core.ViewModels;

/// <summary>
/// View model for the Spindle settings tab.
/// Manages spindle control options such as speed, enable commands, and delay parameters.
/// </summary>
public partial class SpindleSettingsViewModel : SettingsTabViewModelBase
{
    // Property name constants for tracking
    private const string PropertyAddSpindleCode = nameof(AddSpindleCode);
    private const string PropertySetSpindleSpeed = nameof(SetSpindleSpeed);
    private const string PropertySpindleSpeed = nameof(SpindleSpeed);
    private const string PropertyEnableSpindleBeforeOperations = nameof(EnableSpindleBeforeOperations);
    private const string PropertySpindleEnableCommand = nameof(SelectedSpindleEnableCommand);
    private const string PropertyAddSpindleDelayAfterEnable = nameof(AddSpindleDelayAfterEnable);
    private const string PropertySpindleDelayParameter = nameof(SelectedSpindleDelayParameter);
    private const string PropertySpindleDelayValue = nameof(SpindleDelayValue);
    private const string PropertyDisableSpindleAfterOperations = nameof(DisableSpindleAfterOperations);

    [ObservableProperty]
    private bool _addSpindleCode;

    [ObservableProperty]
    private bool _setSpindleSpeed;

    [ObservableProperty]
    private string _spindleSpeed = SpindleDefaults.Speed;

    [ObservableProperty]
    private bool _enableSpindleBeforeOperations;

    [ObservableProperty]
    private SpindleEnableCommandOption? _selectedSpindleEnableCommand;

    [ObservableProperty]
    private bool _addSpindleDelayAfterEnable;

    [ObservableProperty]
    private SpindleDelayParameterOption? _selectedSpindleDelayParameter;

    [ObservableProperty]
    private string _spindleDelayValue = SpindleDefaults.DelayValue;

    [ObservableProperty]
    private bool _disableSpindleAfterOperations;

    // Enabled states for dependent controls
    [ObservableProperty]
    private bool _isSpindleSubOptionsEnabled;

    [ObservableProperty]
    private bool _isSpindleSpeedCheckboxEnabled;

    [ObservableProperty]
    private bool _isSpindleSpeedInputEnabled;

    [ObservableProperty]
    private bool _isSpindleEnableSubOptionsEnabled;

    [ObservableProperty]
    private bool _isSpindleDelaySubOptionsEnabled;

    // Collections for ComboBoxes
    public ObservableCollection<SpindleEnableCommandOption> SpindleEnableCommands { get; } = new();
    public ObservableCollection<SpindleDelayParameterOption> SpindleDelayParameters { get; } = new();
    private readonly OptionCollectionHelper<SpindleEnableCommandOption> _spindleEnableCommandsHelper;
    private readonly OptionCollectionHelper<SpindleDelayParameterOption> _spindleDelayParametersHelper;

    // Property headers dictionary for change tracking
    private readonly Dictionary<string, PropertyHeaderViewModel> _headers = new();

    private readonly ISettingsService _settingsService;

    // Public properties for header access
    public PropertyHeaderViewModel SpindleAddCodeHeader => _headers[PropertyAddSpindleCode];
    public PropertyHeaderViewModel SpindleSetSpeedHeader => _headers[PropertySetSpindleSpeed];
    public PropertyHeaderViewModel SpindleSpeedLabelHeader => _headers[PropertySpindleSpeed];
    public PropertyHeaderViewModel SpindleEnableBeforeOperationsHeader => _headers[PropertyEnableSpindleBeforeOperations];
    public PropertyHeaderViewModel SpindleEnableCommandLabelHeader => _headers[PropertySpindleEnableCommand];
    public PropertyHeaderViewModel SpindleAddDelayAfterEnableHeader => _headers[PropertyAddSpindleDelayAfterEnable];
    public PropertyHeaderViewModel SpindleDelayParameterLabelHeader => _headers[PropertySpindleDelayParameter];
    public PropertyHeaderViewModel SpindleDelayValueLabelHeader => _headers[PropertySpindleDelayValue];
    public PropertyHeaderViewModel SpindleDisableAfterOperationsHeader => _headers[PropertyDisableSpindleAfterOperations];

    public SpindleSettingsViewModel(ISettingsService settingsService)
    {
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
        _spindleEnableCommandsHelper = new OptionCollectionHelper<SpindleEnableCommandOption>(SpindleEnableCommands);
        _spindleDelayParametersHelper = new OptionCollectionHelper<SpindleDelayParameterOption>(SpindleDelayParameters);
        
        // Initialize headers
        _headers[PropertyAddSpindleCode] = new PropertyHeaderViewModel(Resources.SpindleAddCode);
        _headers[PropertySetSpindleSpeed] = new PropertyHeaderViewModel(Resources.SpindleSetSpeed);
        _headers[PropertySpindleSpeed] = new PropertyHeaderViewModel(Resources.SpindleSpeedLabel);
        _headers[PropertyEnableSpindleBeforeOperations] = new PropertyHeaderViewModel(Resources.SpindleEnableBeforeOperations);
        _headers[PropertySpindleEnableCommand] = new PropertyHeaderViewModel(Resources.SpindleEnableCommandLabel);
        _headers[PropertyAddSpindleDelayAfterEnable] = new PropertyHeaderViewModel(Resources.SpindleAddDelayAfterEnable);
        _headers[PropertySpindleDelayParameter] = new PropertyHeaderViewModel(Resources.SpindleDelayParameterLabel);
        _headers[PropertySpindleDelayValue] = new PropertyHeaderViewModel(Resources.SpindleDelayValueLabel);
        _headers[PropertyDisableSpindleAfterOperations] = new PropertyHeaderViewModel(Resources.SpindleDisableAfterOperations);
        
        InitializeSpindleEnableCommands();
        InitializeSpindleDelayParameters();
        LoadFromSettings(_settingsService.Current);
        RegisterTrackedProperties();
        UpdateSpindleDependentControls();
        HeaderTracker.UpdateAllHeadersImmediate();
    }

    /// <summary>
    /// Registers all properties for change tracking, organized by logical groups.
    /// </summary>
    private void RegisterTrackedProperties()
    {
        RegisterBasicSpindleProperties();
        RegisterSpindleEnableDisableProperties();
        RegisterSpindleDelayProperties();
    }

    /// <summary>
    /// Registers basic spindle configuration properties for tracking.
    /// </summary>
    private void RegisterBasicSpindleProperties()
    {
        RegisterProperty(PropertyAddSpindleCode, AddSpindleCode, () => Resources.SpindleAddCode);
        RegisterProperty(PropertySetSpindleSpeed, SetSpindleSpeed, () => Resources.SpindleSetSpeed);
        RegisterProperty(PropertySpindleSpeed, SpindleSpeed, () => Resources.SpindleSpeedLabel);
    }

    /// <summary>
    /// Registers spindle enable/disable properties for tracking.
    /// </summary>
    private void RegisterSpindleEnableDisableProperties()
    {
        RegisterProperty(PropertyEnableSpindleBeforeOperations, EnableSpindleBeforeOperations, () => Resources.SpindleEnableBeforeOperations);
        HeaderTracker.Register(
            PropertySpindleEnableCommand,
            SelectedSpindleEnableCommand?.Key ?? SpindleCommands.DefaultEnableCommand,
            () => Resources.SpindleEnableCommandLabel,
            _headers[PropertySpindleEnableCommand]);
        RegisterProperty(PropertyDisableSpindleAfterOperations, DisableSpindleAfterOperations, () => Resources.SpindleDisableAfterOperations);
    }

    /// <summary>
    /// Registers spindle delay properties for tracking.
    /// </summary>
    private void RegisterSpindleDelayProperties()
    {
        RegisterProperty(PropertyAddSpindleDelayAfterEnable, AddSpindleDelayAfterEnable, () => Resources.SpindleAddDelayAfterEnable);
        HeaderTracker.Register(
            PropertySpindleDelayParameter,
            SelectedSpindleDelayParameter?.Key ?? Settings.SpindleDelayParameters.Default,
            () => Resources.SpindleDelayParameterLabel,
            _headers[PropertySpindleDelayParameter]);
        RegisterProperty(PropertySpindleDelayValue, SpindleDelayValue, () => Resources.SpindleDelayValueLabel);
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
        UpdateSpindleEnableCommandDisplayNames();
        UpdateSpindleDelayParameterDisplayNames();
        HeaderTracker.UpdateAllHeaders();
    }

    /// <summary>
    /// Initializes the spindle enable commands collection with available options.
    /// </summary>
    private void InitializeSpindleEnableCommands()
    {
        _spindleEnableCommandsHelper.Clear();
        _spindleEnableCommandsHelper
            .Add(SpindleCommands.M3, () => Resources.SpindleEnableCommandM3, (k, d) => new SpindleEnableCommandOption(k, d))
            .Add(SpindleCommands.M4, () => Resources.SpindleEnableCommandM4, (k, d) => new SpindleEnableCommandOption(k, d));
    }

    /// <summary>
    /// Initializes the spindle delay parameters collection with available options.
    /// </summary>
    private void InitializeSpindleDelayParameters()
    {
        _spindleDelayParametersHelper.Clear();
        _spindleDelayParametersHelper
            .Add(Settings.SpindleDelayParameters.F, () => Resources.SpindleDelayParameterF, (k, d) => new SpindleDelayParameterOption(k, d))
            .Add(Settings.SpindleDelayParameters.P, () => Resources.SpindleDelayParameterP, (k, d) => new SpindleDelayParameterOption(k, d))
            .Add(Settings.SpindleDelayParameters.Pxx, () => Resources.SpindleDelayParameterPxx, (k, d) => new SpindleDelayParameterOption(k, d));
    }

    private void UpdateSpindleEnableCommandDisplayNames()
    {
        _spindleEnableCommandsHelper.UpdateDisplayNames();
    }

    private void UpdateSpindleDelayParameterDisplayNames()
    {
        _spindleDelayParametersHelper.UpdateDisplayNames();
    }

    partial void OnAddSpindleCodeChanged(bool value)
    {
        UpdateSpindleDependentControls();
        HeaderTracker.Update(PropertyAddSpindleCode, value);
    }

    partial void OnSetSpindleSpeedChanged(bool value)
    {
        UpdateSpindleSpeedInputEnabled();
        HeaderTracker.Update(PropertySetSpindleSpeed, value);
    }

    partial void OnEnableSpindleBeforeOperationsChanged(bool value)
    {
        IsSpindleEnableSubOptionsEnabled = EnableSpindleBeforeOperations && AddSpindleCode;
        if (!IsSpindleEnableSubOptionsEnabled)
        {
            IsSpindleDelaySubOptionsEnabled = false;
        }
        else
        {
            IsSpindleDelaySubOptionsEnabled = AddSpindleDelayAfterEnable && EnableSpindleBeforeOperations && AddSpindleCode;
        }
        // Update speed input enabled state when enable spindle changes
        UpdateSpindleSpeedInputEnabled();
        HeaderTracker.Update(PropertyEnableSpindleBeforeOperations, value);
    }

    partial void OnAddSpindleDelayAfterEnableChanged(bool value)
    {
        IsSpindleDelaySubOptionsEnabled = AddSpindleDelayAfterEnable && EnableSpindleBeforeOperations && AddSpindleCode;
        HeaderTracker.Update(PropertyAddSpindleDelayAfterEnable, value);
    }

    partial void OnSpindleSpeedChanged(string value)
    {
        HeaderTracker.Update(PropertySpindleSpeed, value);
    }

    partial void OnDisableSpindleAfterOperationsChanged(bool value)
    {
        HeaderTracker.Update(PropertyDisableSpindleAfterOperations, value);
    }

    partial void OnSpindleDelayValueChanged(string value)
    {
        HeaderTracker.Update(PropertySpindleDelayValue, value);
    }

    partial void OnSelectedSpindleEnableCommandChanged(SpindleEnableCommandOption? value)
    {
        HeaderTracker.Update(PropertySpindleEnableCommand, value?.Key ?? SpindleCommands.DefaultEnableCommand);
    }

    partial void OnSelectedSpindleDelayParameterChanged(SpindleDelayParameterOption? value)
    {
        HeaderTracker.Update(PropertySpindleDelayParameter, value?.Key ?? Settings.SpindleDelayParameters.Default);
    }

    /// <summary>
    /// Updates the enabled state of dependent controls based on current property values.
    /// </summary>
    private void UpdateSpindleDependentControls()
    {
        IsSpindleSubOptionsEnabled = AddSpindleCode;
        UpdateSpindleSpeedControls();
        IsSpindleEnableSubOptionsEnabled = EnableSpindleBeforeOperations && AddSpindleCode;
        IsSpindleDelaySubOptionsEnabled = AddSpindleDelayAfterEnable && EnableSpindleBeforeOperations && AddSpindleCode;
    }

    /// <summary>
    /// Updates the enabled state of spindle speed controls.
    /// Speed checkbox is enabled when: AddSpindleCode and EnableSpindleBeforeOperations are true.
    /// Speed input is enabled only when: AddSpindleCode, EnableSpindleBeforeOperations, and SetSpindleSpeed are all true.
    /// </summary>
    private void UpdateSpindleSpeedControls()
    {
        IsSpindleSpeedCheckboxEnabled = EnableSpindleBeforeOperations && AddSpindleCode;
        IsSpindleSpeedInputEnabled = SetSpindleSpeed && EnableSpindleBeforeOperations && AddSpindleCode;
    }

    /// <summary>
    /// Updates the enabled state of spindle speed input.
    /// Speed input is enabled only when: AddSpindleCode, EnableSpindleBeforeOperations, and SetSpindleSpeed are all true.
    /// </summary>
    private void UpdateSpindleSpeedInputEnabled()
    {
        UpdateSpindleSpeedControls();
    }

    public override void LoadFromSettings(AppSettings settings)
    {
        LoadBoolProperty(settings, s => s.AddSpindleCode, SpindleDefaults.AddSpindleCode, PropertyAddSpindleCode, v => AddSpindleCode = v);
        LoadBoolProperty(settings, s => s.SetSpindleSpeed, SpindleDefaults.SetSpindleSpeed, PropertySetSpindleSpeed, v => SetSpindleSpeed = v);
        LoadStringProperty(settings, s => s.SpindleSpeed, SpindleDefaults.Speed, PropertySpindleSpeed, v => SpindleSpeed = v);
        LoadBoolProperty(settings, s => s.EnableSpindleBeforeOperations, SpindleDefaults.EnableSpindleBeforeOperations, PropertyEnableSpindleBeforeOperations, v => EnableSpindleBeforeOperations = v);
        
        var spindleEnableCommand = LoadStringProperty(settings, s => s.SpindleEnableCommand, SpindleCommands.DefaultEnableCommand, PropertySpindleEnableCommand, _ => { });
        SelectedSpindleEnableCommand = _spindleEnableCommandsHelper.FindByKey(spindleEnableCommand)
                                      ?? _spindleEnableCommandsHelper.FindByKey(SpindleCommands.DefaultEnableCommand);
        
        LoadBoolProperty(settings, s => s.AddSpindleDelayAfterEnable, SpindleDefaults.AddSpindleDelayAfterEnable, PropertyAddSpindleDelayAfterEnable, v => AddSpindleDelayAfterEnable = v);
        
        var spindleDelayParameter = LoadStringProperty(settings, s => s.SpindleDelayParameter, Settings.SpindleDelayParameters.Default, PropertySpindleDelayParameter, _ => { });
        SelectedSpindleDelayParameter = _spindleDelayParametersHelper.FindByKey(spindleDelayParameter)
                                       ?? _spindleDelayParametersHelper.FindByKey(Settings.SpindleDelayParameters.Default);
        
        LoadStringProperty(settings, s => s.SpindleDelayValue, SpindleDefaults.DelayValue, PropertySpindleDelayValue, v => SpindleDelayValue = v);
        LoadBoolProperty(settings, s => s.DisableSpindleAfterOperations, SpindleDefaults.DisableSpindleAfterOperations, PropertyDisableSpindleAfterOperations, v => DisableSpindleAfterOperations = v);
    }

    public override void SaveToSettings(AppSettings settings)
    {
        settings.AddSpindleCode = AddSpindleCode;
        settings.SetSpindleSpeed = SetSpindleSpeed;
        settings.SpindleSpeed = SpindleSpeed;
        settings.EnableSpindleBeforeOperations = EnableSpindleBeforeOperations;
        settings.SpindleEnableCommand = SelectedSpindleEnableCommand?.Key ?? SpindleCommands.DefaultEnableCommand;
        settings.AddSpindleDelayAfterEnable = AddSpindleDelayAfterEnable;
        settings.SpindleDelayParameter = SelectedSpindleDelayParameter?.Key ?? Settings.SpindleDelayParameters.Default;
        settings.SpindleDelayValue = SpindleDelayValue;
        settings.DisableSpindleAfterOperations = DisableSpindleAfterOperations;
    }

    public override void ResetToOriginal()
    {
        ResetBoolProperty(PropertyAddSpindleCode, SpindleDefaults.AddSpindleCode, v => AddSpindleCode = v);
        ResetBoolProperty(PropertySetSpindleSpeed, SpindleDefaults.SetSpindleSpeed, v => SetSpindleSpeed = v);
        ResetStringProperty(PropertySpindleSpeed, SpindleDefaults.Speed, v => SpindleSpeed = v);
        ResetBoolProperty(PropertyEnableSpindleBeforeOperations, SpindleDefaults.EnableSpindleBeforeOperations, v => EnableSpindleBeforeOperations = v);
        
        var originalCommand = ResetStringProperty(PropertySpindleEnableCommand, SpindleCommands.DefaultEnableCommand, _ => { });
        SelectedSpindleEnableCommand = _spindleEnableCommandsHelper.FindByKey(originalCommand)
                                      ?? _spindleEnableCommandsHelper.FindByKey(SpindleCommands.DefaultEnableCommand);
        
        ResetBoolProperty(PropertyAddSpindleDelayAfterEnable, SpindleDefaults.AddSpindleDelayAfterEnable, v => AddSpindleDelayAfterEnable = v);
        
        var originalParameter = ResetStringProperty(PropertySpindleDelayParameter, Settings.SpindleDelayParameters.Default, _ => { });
        SelectedSpindleDelayParameter = _spindleDelayParametersHelper.FindByKey(originalParameter)
                                       ?? _spindleDelayParametersHelper.FindByKey(Settings.SpindleDelayParameters.Default);
        
        ResetStringProperty(PropertySpindleDelayValue, SpindleDefaults.DelayValue, v => SpindleDelayValue = v);
        ResetBoolProperty(PropertyDisableSpindleAfterOperations, SpindleDefaults.DisableSpindleAfterOperations, v => DisableSpindleAfterOperations = v);
        
        UpdateSpindleDependentControls();
        HeaderTracker.UpdateAllHeadersImmediate();
    }
}
