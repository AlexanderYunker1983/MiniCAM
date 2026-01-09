using System;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using MiniCAM.Core.Localization;
using MiniCAM.Core.Settings;

namespace MiniCAM.Core.ViewModels;

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
    private bool _isSpindleSpeedInputEnabled;

    [ObservableProperty]
    private bool _isSpindleEnableSubOptionsEnabled;

    [ObservableProperty]
    private bool _isSpindleDelaySubOptionsEnabled;

    // Collections for ComboBoxes
    public ObservableCollection<SpindleEnableCommandOption> SpindleEnableCommands { get; } = new();
    public ObservableCollection<SpindleDelayParameterOption> SpindleDelayParameters { get; } = new();

    // Header text and font style properties
    [ObservableProperty]
    private string _spindleAddCodeHeaderText = Resources.SpindleAddCode;

    [ObservableProperty]
    private FontStyle _spindleAddCodeFontStyle = FontStyle.Normal;

    [ObservableProperty]
    private string _spindleSetSpeedHeaderText = Resources.SpindleSetSpeed;

    [ObservableProperty]
    private FontStyle _spindleSetSpeedFontStyle = FontStyle.Normal;

    [ObservableProperty]
    private string _spindleSpeedLabelHeaderText = Resources.SpindleSpeedLabel;

    [ObservableProperty]
    private FontStyle _spindleSpeedLabelFontStyle = FontStyle.Normal;

    [ObservableProperty]
    private string _spindleEnableBeforeOperationsHeaderText = Resources.SpindleEnableBeforeOperations;

    [ObservableProperty]
    private FontStyle _spindleEnableBeforeOperationsFontStyle = FontStyle.Normal;

    [ObservableProperty]
    private string _spindleEnableCommandLabelHeaderText = Resources.SpindleEnableCommandLabel;

    [ObservableProperty]
    private FontStyle _spindleEnableCommandLabelFontStyle = FontStyle.Normal;

    [ObservableProperty]
    private string _spindleAddDelayAfterEnableHeaderText = Resources.SpindleAddDelayAfterEnable;

    [ObservableProperty]
    private FontStyle _spindleAddDelayAfterEnableFontStyle = FontStyle.Normal;

    [ObservableProperty]
    private string _spindleDelayParameterLabelHeaderText = Resources.SpindleDelayParameterLabel;

    [ObservableProperty]
    private FontStyle _spindleDelayParameterLabelFontStyle = FontStyle.Normal;

    [ObservableProperty]
    private string _spindleDelayValueLabelHeaderText = Resources.SpindleDelayValueLabel;

    [ObservableProperty]
    private FontStyle _spindleDelayValueLabelFontStyle = FontStyle.Normal;

    [ObservableProperty]
    private string _spindleDisableAfterOperationsHeaderText = Resources.SpindleDisableAfterOperations;

    [ObservableProperty]
    private FontStyle _spindleDisableAfterOperationsFontStyle = FontStyle.Normal;

    public SpindleSettingsViewModel()
    {
        BuildSpindleEnableCommands();
        BuildSpindleDelayParameters();
        LoadFromSettings(SettingsManager.Current);
        RegisterTrackedProperties();
        UpdateSpindleSubOptionsEnabled();
        HeaderTracker.UpdateAllHeaders();
    }

    private void RegisterTrackedProperties()
    {
        HeaderTracker.Register(
            PropertyAddSpindleCode,
            AddSpindleCode,
            () => Resources.SpindleAddCode,
            value => SpindleAddCodeHeaderText = value,
            value => SpindleAddCodeFontStyle = value);

        HeaderTracker.Register(
            PropertySetSpindleSpeed,
            SetSpindleSpeed,
            () => Resources.SpindleSetSpeed,
            value => SpindleSetSpeedHeaderText = value,
            value => SpindleSetSpeedFontStyle = value);

        HeaderTracker.Register(
            PropertySpindleSpeed,
            SpindleSpeed,
            () => Resources.SpindleSpeedLabel,
            value => SpindleSpeedLabelHeaderText = value,
            value => SpindleSpeedLabelFontStyle = value);

        HeaderTracker.Register(
            PropertyEnableSpindleBeforeOperations,
            EnableSpindleBeforeOperations,
            () => Resources.SpindleEnableBeforeOperations,
            value => SpindleEnableBeforeOperationsHeaderText = value,
            value => SpindleEnableBeforeOperationsFontStyle = value);

        HeaderTracker.Register(
            PropertySpindleEnableCommand,
            SelectedSpindleEnableCommand?.Key ?? SpindleCommands.DefaultEnableCommand,
            () => Resources.SpindleEnableCommandLabel,
            value => SpindleEnableCommandLabelHeaderText = value,
            value => SpindleEnableCommandLabelFontStyle = value);

        HeaderTracker.Register(
            PropertyAddSpindleDelayAfterEnable,
            AddSpindleDelayAfterEnable,
            () => Resources.SpindleAddDelayAfterEnable,
            value => SpindleAddDelayAfterEnableHeaderText = value,
            value => SpindleAddDelayAfterEnableFontStyle = value);

        HeaderTracker.Register(
            PropertySpindleDelayParameter,
            SelectedSpindleDelayParameter?.Key ?? Settings.SpindleDelayParameters.Default,
            () => Resources.SpindleDelayParameterLabel,
            value => SpindleDelayParameterLabelHeaderText = value,
            value => SpindleDelayParameterLabelFontStyle = value);

        HeaderTracker.Register(
            PropertySpindleDelayValue,
            SpindleDelayValue,
            () => Resources.SpindleDelayValueLabel,
            value => SpindleDelayValueLabelHeaderText = value,
            value => SpindleDelayValueLabelFontStyle = value);

        HeaderTracker.Register(
            PropertyDisableSpindleAfterOperations,
            DisableSpindleAfterOperations,
            () => Resources.SpindleDisableAfterOperations,
            value => SpindleDisableAfterOperationsHeaderText = value,
            value => SpindleDisableAfterOperationsFontStyle = value);
    }

    protected override void UpdateLocalizedStrings()
    {
        UpdateSpindleEnableCommandDisplayNames();
        UpdateSpindleDelayParameterDisplayNames();
        HeaderTracker.UpdateAllHeaders();
    }

    private void BuildSpindleEnableCommands()
    {
        SpindleEnableCommands.Clear();
        SpindleEnableCommands.Add(new SpindleEnableCommandOption(SpindleCommands.M3, Resources.SpindleEnableCommandM3));
        SpindleEnableCommands.Add(new SpindleEnableCommandOption(SpindleCommands.M4, Resources.SpindleEnableCommandM4));
    }

    private void BuildSpindleDelayParameters()
    {
        SpindleDelayParameters.Clear();
        SpindleDelayParameters.Add(new SpindleDelayParameterOption(Settings.SpindleDelayParameters.F, Resources.SpindleDelayParameterF));
        SpindleDelayParameters.Add(new SpindleDelayParameterOption(Settings.SpindleDelayParameters.P, Resources.SpindleDelayParameterP));
        SpindleDelayParameters.Add(new SpindleDelayParameterOption(Settings.SpindleDelayParameters.Pxx, Resources.SpindleDelayParameterPxx));
    }

    private void UpdateSpindleEnableCommandDisplayNames()
    {
        foreach (var command in SpindleEnableCommands)
        {
            command.DisplayName = command.Key switch
            {
                SpindleCommands.M3 => Resources.SpindleEnableCommandM3,
                SpindleCommands.M4 => Resources.SpindleEnableCommandM4,
                _ => command.Key
            };
        }
    }

    private void UpdateSpindleDelayParameterDisplayNames()
    {
        foreach (var parameter in SpindleDelayParameters)
        {
            parameter.DisplayName = parameter.Key switch
            {
                Settings.SpindleDelayParameters.F => Resources.SpindleDelayParameterF,
                Settings.SpindleDelayParameters.P => Resources.SpindleDelayParameterP,
                Settings.SpindleDelayParameters.Pxx => Resources.SpindleDelayParameterPxx,
                _ => parameter.Key
            };
        }
    }

    partial void OnAddSpindleCodeChanged(bool value)
    {
        UpdateSpindleSubOptionsEnabled();
        HeaderTracker.Update(PropertyAddSpindleCode, value);
    }

    partial void OnSetSpindleSpeedChanged(bool value)
    {
        IsSpindleSpeedInputEnabled = SetSpindleSpeed && AddSpindleCode;
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

    private void UpdateSpindleSubOptionsEnabled()
    {
        IsSpindleSubOptionsEnabled = AddSpindleCode;
        IsSpindleSpeedInputEnabled = SetSpindleSpeed && AddSpindleCode;
        IsSpindleEnableSubOptionsEnabled = EnableSpindleBeforeOperations && AddSpindleCode;
        IsSpindleDelaySubOptionsEnabled = AddSpindleDelayAfterEnable && EnableSpindleBeforeOperations && AddSpindleCode;
    }

    public override void LoadFromSettings(AppSettings settings)
    {
        var addSpindleCode = settings.AddSpindleCode ?? SpindleDefaults.AddSpindleCode;
        var setSpindleSpeed = settings.SetSpindleSpeed ?? SpindleDefaults.SetSpindleSpeed;
        var spindleSpeed = settings.SpindleSpeed ?? SpindleDefaults.Speed;
        var enableSpindleBeforeOperations = settings.EnableSpindleBeforeOperations ?? SpindleDefaults.EnableSpindleBeforeOperations;
        var spindleEnableCommand = settings.SpindleEnableCommand ?? SpindleCommands.DefaultEnableCommand;
        var addSpindleDelayAfterEnable = settings.AddSpindleDelayAfterEnable ?? SpindleDefaults.AddSpindleDelayAfterEnable;
        var spindleDelayParameter = settings.SpindleDelayParameter ?? Settings.SpindleDelayParameters.Default;
        var spindleDelayValue = settings.SpindleDelayValue ?? SpindleDefaults.DelayValue;
        var disableSpindleAfterOperations = settings.DisableSpindleAfterOperations ?? SpindleDefaults.DisableSpindleAfterOperations;

        AddSpindleCode = addSpindleCode;
        SetSpindleSpeed = setSpindleSpeed;
        SpindleSpeed = spindleSpeed;
        EnableSpindleBeforeOperations = enableSpindleBeforeOperations;
        SelectedSpindleEnableCommand = SpindleEnableCommands.FirstOrDefault(x => x.Key == spindleEnableCommand)
                                      ?? SpindleEnableCommands.FirstOrDefault(x => x.Key == SpindleCommands.DefaultEnableCommand);
        AddSpindleDelayAfterEnable = addSpindleDelayAfterEnable;
        SelectedSpindleDelayParameter = SpindleDelayParameters.FirstOrDefault(x => x.Key == spindleDelayParameter)
                                       ?? SpindleDelayParameters.FirstOrDefault(x => x.Key == Settings.SpindleDelayParameters.Default);
        SpindleDelayValue = spindleDelayValue;
        DisableSpindleAfterOperations = disableSpindleAfterOperations;

        // Update original values in tracker
        HeaderTracker.UpdateOriginal(PropertyAddSpindleCode, addSpindleCode);
        HeaderTracker.UpdateOriginal(PropertySetSpindleSpeed, setSpindleSpeed);
        HeaderTracker.UpdateOriginal(PropertySpindleSpeed, spindleSpeed);
        HeaderTracker.UpdateOriginal(PropertyEnableSpindleBeforeOperations, enableSpindleBeforeOperations);
        HeaderTracker.UpdateOriginal(PropertySpindleEnableCommand, spindleEnableCommand);
        HeaderTracker.UpdateOriginal(PropertyAddSpindleDelayAfterEnable, addSpindleDelayAfterEnable);
        HeaderTracker.UpdateOriginal(PropertySpindleDelayParameter, spindleDelayParameter);
        HeaderTracker.UpdateOriginal(PropertySpindleDelayValue, spindleDelayValue);
        HeaderTracker.UpdateOriginal(PropertyDisableSpindleAfterOperations, disableSpindleAfterOperations);
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
        AddSpindleCode = HeaderTracker.GetOriginalValue<bool>(PropertyAddSpindleCode);
        SetSpindleSpeed = HeaderTracker.GetOriginalValue<bool>(PropertySetSpindleSpeed);
        SpindleSpeed = HeaderTracker.GetOriginalValue<string>(PropertySpindleSpeed) ?? SpindleDefaults.Speed;
        EnableSpindleBeforeOperations = HeaderTracker.GetOriginalValue<bool>(PropertyEnableSpindleBeforeOperations);
        var originalCommand = HeaderTracker.GetOriginalValue<string>(PropertySpindleEnableCommand) ?? SpindleCommands.DefaultEnableCommand;
        SelectedSpindleEnableCommand = SpindleEnableCommands.FirstOrDefault(x => x.Key == originalCommand)
                                      ?? SpindleEnableCommands.FirstOrDefault(x => x.Key == SpindleCommands.DefaultEnableCommand);
        AddSpindleDelayAfterEnable = HeaderTracker.GetOriginalValue<bool>(PropertyAddSpindleDelayAfterEnable);
        var originalParameter = HeaderTracker.GetOriginalValue<string>(PropertySpindleDelayParameter) ?? Settings.SpindleDelayParameters.Default;
        SelectedSpindleDelayParameter = SpindleDelayParameters.FirstOrDefault(x => x.Key == originalParameter)
                                       ?? SpindleDelayParameters.FirstOrDefault(x => x.Key == Settings.SpindleDelayParameters.Default);
        SpindleDelayValue = HeaderTracker.GetOriginalValue<string>(PropertySpindleDelayValue) ?? SpindleDefaults.DelayValue;
        DisableSpindleAfterOperations = HeaderTracker.GetOriginalValue<bool>(PropertyDisableSpindleAfterOperations);
        UpdateSpindleSubOptionsEnabled();
        HeaderTracker.UpdateAllHeaders();
    }
}
