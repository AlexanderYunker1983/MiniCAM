using System;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MiniCAM.Core.Localization;
using MiniCAM.Core.Settings;

namespace MiniCAM.Core.ViewModels;

public partial class CodeGenerationSettingsViewModel : ViewModelBase, IDisposable
{
    // Original values for tracking changes
    private bool _originalAddCoolantCode;
    private bool _originalEnableCoolantAtStart;
    private bool _originalDisableCoolantAtEnd;
    private bool _originalUseLineNumbers;
    private string _originalStartLineNumber = "10";
    private string _originalLineNumberStep = "10";
    private bool _originalGenerateComments;
    private bool _originalAllowArcs;
    private bool _originalFormatCommands;
    private bool _originalSetWorkCoordinateSystem;
    private string _originalCoordinateSystem = "G54";
    private bool _originalSetAbsoluteCoordinates;
    private bool _originalAllowRelativeCoordinates;
    private bool _originalSetZerosAtStart;
    private string _originalX0 = "0";
    private string _originalY0 = "0";
    private string _originalZ0 = "0";
    private bool _originalMoveToPointAtEnd;
    private string _originalX = "0";
    private string _originalY = "0";
    private string _originalZ = "0";
    private bool _originalAddSpindleCode;
    private bool _originalSetSpindleSpeed;
    private string _originalSpindleSpeed = "1000";
    private bool _originalEnableSpindleBeforeOperations;
    private string _originalSpindleEnableCommand = "M3";
    private bool _originalAddSpindleDelayAfterEnable;
    private string _originalSpindleDelayParameter = "F";
    private string _originalSpindleDelayValue = "2";
    private bool _originalDisableSpindleAfterOperations;

    // Coolant properties
    [ObservableProperty]
    private bool _addCoolantCode;

    [ObservableProperty]
    private bool _enableCoolantAtStart;

    [ObservableProperty]
    private bool _disableCoolantAtEnd;

    [ObservableProperty]
    private bool _isCoolantSubOptionsEnabled;

    // Spindle properties
    [ObservableProperty]
    private bool _addSpindleCode;

    [ObservableProperty]
    private bool _setSpindleSpeed;

    [ObservableProperty]
    private string _spindleSpeed = "1000";

    [ObservableProperty]
    private bool _enableSpindleBeforeOperations;

    [ObservableProperty]
    private SpindleEnableCommandOption? _selectedSpindleEnableCommand;

    [ObservableProperty]
    private bool _addSpindleDelayAfterEnable;

    [ObservableProperty]
    private SpindleDelayParameterOption? _selectedSpindleDelayParameter;

    [ObservableProperty]
    private string _spindleDelayValue = "2";

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

    // Localized text properties
    [ObservableProperty]
    private string _coolantAddCodeText = Resources.CoolantAddCode;

    [ObservableProperty]
    private string _coolantEnableAtStartText = Resources.CoolantEnableAtStart;

    [ObservableProperty]
    private string _coolantDisableAtEndText = Resources.CoolantDisableAtEnd;

    [ObservableProperty]
    private string _spindleAddCodeText = Resources.SpindleAddCode;

    [ObservableProperty]
    private string _spindleSetSpeedText = Resources.SpindleSetSpeed;

    [ObservableProperty]
    private string _spindleSpeedLabelText = Resources.SpindleSpeedLabel;

    [ObservableProperty]
    private string _spindleEnableBeforeOperationsText = Resources.SpindleEnableBeforeOperations;

    [ObservableProperty]
    private string _spindleEnableCommandLabelText = Resources.SpindleEnableCommandLabel;

    [ObservableProperty]
    private string _spindleAddDelayAfterEnableText = Resources.SpindleAddDelayAfterEnable;

    [ObservableProperty]
    private string _spindleDelayParameterLabelText = Resources.SpindleDelayParameterLabel;

    [ObservableProperty]
    private string _spindleDelayValueLabelText = Resources.SpindleDelayValueLabel;

    [ObservableProperty]
    private string _spindleDisableAfterOperationsText = Resources.SpindleDisableAfterOperations;

    // Code generation properties
    [ObservableProperty]
    private bool _useLineNumbers = true;

    [ObservableProperty]
    private string _startLineNumber = "10";

    [ObservableProperty]
    private string _lineNumberStep = "10";

    [ObservableProperty]
    private bool _generateComments;

    [ObservableProperty]
    private bool _allowArcs;

    [ObservableProperty]
    private bool _formatCommands;

    [ObservableProperty]
    private bool _setWorkCoordinateSystem;

    [ObservableProperty]
    private CoordinateSystemOption? _selectedCoordinateSystem;

    [ObservableProperty]
    private bool _setAbsoluteCoordinates = true;

    [ObservableProperty]
    private bool _allowRelativeCoordinates;

    [ObservableProperty]
    private bool _setZerosAtStart = true;

    [ObservableProperty]
    private string _x0 = "0";

    [ObservableProperty]
    private string _y0 = "0";

    [ObservableProperty]
    private string _z0 = "0";

    [ObservableProperty]
    private bool _moveToPointAtEnd;

    [ObservableProperty]
    private string _x = "0";

    [ObservableProperty]
    private string _y = "0";

    [ObservableProperty]
    private string _z = "0";

    // Enabled states for dependent controls
    [ObservableProperty]
    private bool _isLineNumberOptionsEnabled;

    [ObservableProperty]
    private bool _isCoordinateSystemComboEnabled;

    [ObservableProperty]
    private bool _isSetZerosOptionsEnabled;

    [ObservableProperty]
    private bool _isMoveToPointOptionsEnabled;

    // Collections for ComboBoxes
    public ObservableCollection<CoordinateSystemOption> CoordinateSystems { get; } = new();

    // Localized text properties for code generation
    [ObservableProperty]
    private string _codeGenUseLineNumbersText = Resources.CodeGenUseLineNumbers;

    [ObservableProperty]
    private string _codeGenStartLineNumberText = Resources.CodeGenStartLineNumber;

    [ObservableProperty]
    private string _codeGenLineNumberStepText = Resources.CodeGenLineNumberStep;

    [ObservableProperty]
    private string _codeGenGenerateCommentsText = Resources.CodeGenGenerateComments;

    [ObservableProperty]
    private string _codeGenAllowArcsText = Resources.CodeGenAllowArcs;

    [ObservableProperty]
    private string _codeGenFormatCommandsText = Resources.CodeGenFormatCommands;

    [ObservableProperty]
    private string _codeGenSetWorkCoordinateSystemText = Resources.CodeGenSetWorkCoordinateSystem;

    [ObservableProperty]
    private string _codeGenCoordinateSystemLabelText = Resources.CodeGenCoordinateSystemLabel;

    [ObservableProperty]
    private string _codeGenSetAbsoluteCoordinatesText = Resources.CodeGenSetAbsoluteCoordinates;

    [ObservableProperty]
    private string _codeGenAllowRelativeCoordinatesText = Resources.CodeGenAllowRelativeCoordinates;

    [ObservableProperty]
    private string _codeGenSetZerosAtStartText = Resources.CodeGenSetZerosAtStart;

    [ObservableProperty]
    private string _codeGenX0Text = Resources.CodeGenX0;

    [ObservableProperty]
    private string _codeGenY0Text = Resources.CodeGenY0;

    [ObservableProperty]
    private string _codeGenZ0Text = Resources.CodeGenZ0;

    [ObservableProperty]
    private string _codeGenMoveToPointAtEndText = Resources.CodeGenMoveToPointAtEnd;

    [ObservableProperty]
    private string _codeGenXText = Resources.CodeGenX;

    [ObservableProperty]
    private string _codeGenYText = Resources.CodeGenY;

    [ObservableProperty]
    private string _codeGenZText = Resources.CodeGenZ;

    [ObservableProperty]
    private string _applyButtonText = Resources.ButtonApply;

    [ObservableProperty]
    private string _resetButtonText = Resources.ButtonReset;

    // Header text and font style properties for tracking changes
    [ObservableProperty]
    private string _codeGenUseLineNumbersHeaderText = Resources.CodeGenUseLineNumbers;

    [ObservableProperty]
    private FontStyle _codeGenUseLineNumbersFontStyle = FontStyle.Normal;

    [ObservableProperty]
    private string _codeGenStartLineNumberHeaderText = Resources.CodeGenStartLineNumber;

    [ObservableProperty]
    private FontStyle _codeGenStartLineNumberFontStyle = FontStyle.Normal;

    [ObservableProperty]
    private string _codeGenLineNumberStepHeaderText = Resources.CodeGenLineNumberStep;

    [ObservableProperty]
    private FontStyle _codeGenLineNumberStepFontStyle = FontStyle.Normal;

    [ObservableProperty]
    private string _codeGenGenerateCommentsHeaderText = Resources.CodeGenGenerateComments;

    [ObservableProperty]
    private FontStyle _codeGenGenerateCommentsFontStyle = FontStyle.Normal;

    [ObservableProperty]
    private string _codeGenAllowArcsHeaderText = Resources.CodeGenAllowArcs;

    [ObservableProperty]
    private FontStyle _codeGenAllowArcsFontStyle = FontStyle.Normal;

    [ObservableProperty]
    private string _codeGenFormatCommandsHeaderText = Resources.CodeGenFormatCommands;

    [ObservableProperty]
    private FontStyle _codeGenFormatCommandsFontStyle = FontStyle.Normal;

    [ObservableProperty]
    private string _codeGenSetWorkCoordinateSystemHeaderText = Resources.CodeGenSetWorkCoordinateSystem;

    [ObservableProperty]
    private FontStyle _codeGenSetWorkCoordinateSystemFontStyle = FontStyle.Normal;

    [ObservableProperty]
    private string _codeGenCoordinateSystemLabelHeaderText = Resources.CodeGenCoordinateSystemLabel;

    [ObservableProperty]
    private FontStyle _codeGenCoordinateSystemLabelFontStyle = FontStyle.Normal;

    [ObservableProperty]
    private string _codeGenSetAbsoluteCoordinatesHeaderText = Resources.CodeGenSetAbsoluteCoordinates;

    [ObservableProperty]
    private FontStyle _codeGenSetAbsoluteCoordinatesFontStyle = FontStyle.Normal;

    [ObservableProperty]
    private string _codeGenAllowRelativeCoordinatesHeaderText = Resources.CodeGenAllowRelativeCoordinates;

    [ObservableProperty]
    private FontStyle _codeGenAllowRelativeCoordinatesFontStyle = FontStyle.Normal;

    [ObservableProperty]
    private string _codeGenSetZerosAtStartHeaderText = Resources.CodeGenSetZerosAtStart;

    [ObservableProperty]
    private FontStyle _codeGenSetZerosAtStartFontStyle = FontStyle.Normal;

    [ObservableProperty]
    private string _codeGenX0HeaderText = Resources.CodeGenX0;

    [ObservableProperty]
    private FontStyle _codeGenX0FontStyle = FontStyle.Normal;

    [ObservableProperty]
    private string _codeGenY0HeaderText = Resources.CodeGenY0;

    [ObservableProperty]
    private FontStyle _codeGenY0FontStyle = FontStyle.Normal;

    [ObservableProperty]
    private string _codeGenZ0HeaderText = Resources.CodeGenZ0;

    [ObservableProperty]
    private FontStyle _codeGenZ0FontStyle = FontStyle.Normal;

    [ObservableProperty]
    private string _codeGenMoveToPointAtEndHeaderText = Resources.CodeGenMoveToPointAtEnd;

    [ObservableProperty]
    private FontStyle _codeGenMoveToPointAtEndFontStyle = FontStyle.Normal;

    [ObservableProperty]
    private string _codeGenXHeaderText = Resources.CodeGenX;

    [ObservableProperty]
    private FontStyle _codeGenXFontStyle = FontStyle.Normal;

    [ObservableProperty]
    private string _codeGenYHeaderText = Resources.CodeGenY;

    [ObservableProperty]
    private FontStyle _codeGenYFontStyle = FontStyle.Normal;

    [ObservableProperty]
    private string _codeGenZHeaderText = Resources.CodeGenZ;

    [ObservableProperty]
    private FontStyle _codeGenZFontStyle = FontStyle.Normal;

    // Spindle headers
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

    // Coolant headers
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

    public CodeGenerationSettingsViewModel()
    {
        Resources.CultureChanged += OnCultureChanged;
        BuildSpindleEnableCommands();
        BuildSpindleDelayParameters();
        BuildCoordinateSystems();
        LoadFromSettings();
        UpdateCoolantSubOptionsEnabled();
        UpdateSpindleSubOptionsEnabled();
        UpdateCodeGenSubOptionsEnabled();
        UpdateHeaders();
    }

    private void OnCultureChanged(object? sender, CultureChangedEventArgs e)
    {
        UpdateLocalizedTexts();
    }

    private void BuildSpindleEnableCommands()
    {
        SpindleEnableCommands.Clear();
        SpindleEnableCommands.Add(new SpindleEnableCommandOption("M3", Resources.SpindleEnableCommandM3));
        SpindleEnableCommands.Add(new SpindleEnableCommandOption("M4", Resources.SpindleEnableCommandM4));
    }

    private void BuildSpindleDelayParameters()
    {
        SpindleDelayParameters.Clear();
        SpindleDelayParameters.Add(new SpindleDelayParameterOption("F", Resources.SpindleDelayParameterF));
        SpindleDelayParameters.Add(new SpindleDelayParameterOption("P", Resources.SpindleDelayParameterP));
        SpindleDelayParameters.Add(new SpindleDelayParameterOption("Pxx.", Resources.SpindleDelayParameterPxx));
    }

    private void BuildCoordinateSystems()
    {
        CoordinateSystems.Clear();
        CoordinateSystems.Add(new CoordinateSystemOption("G54", Resources.CodeGenCoordinateSystemG54));
        CoordinateSystems.Add(new CoordinateSystemOption("G55", Resources.CodeGenCoordinateSystemG55));
        CoordinateSystems.Add(new CoordinateSystemOption("G56", Resources.CodeGenCoordinateSystemG56));
        CoordinateSystems.Add(new CoordinateSystemOption("G57", Resources.CodeGenCoordinateSystemG57));
        CoordinateSystems.Add(new CoordinateSystemOption("G58", Resources.CodeGenCoordinateSystemG58));
        CoordinateSystems.Add(new CoordinateSystemOption("G59", Resources.CodeGenCoordinateSystemG59));
    }

    private void UpdateLocalizedTexts()
    {
        CoolantAddCodeText = Resources.CoolantAddCode;
        CoolantEnableAtStartText = Resources.CoolantEnableAtStart;
        CoolantDisableAtEndText = Resources.CoolantDisableAtEnd;
        SpindleAddCodeText = Resources.SpindleAddCode;
        SpindleSetSpeedText = Resources.SpindleSetSpeed;
        SpindleSpeedLabelText = Resources.SpindleSpeedLabel;
        SpindleEnableBeforeOperationsText = Resources.SpindleEnableBeforeOperations;
        SpindleEnableCommandLabelText = Resources.SpindleEnableCommandLabel;
        SpindleAddDelayAfterEnableText = Resources.SpindleAddDelayAfterEnable;
        SpindleDelayParameterLabelText = Resources.SpindleDelayParameterLabel;
        SpindleDelayValueLabelText = Resources.SpindleDelayValueLabel;
        SpindleDisableAfterOperationsText = Resources.SpindleDisableAfterOperations;
        CodeGenUseLineNumbersText = Resources.CodeGenUseLineNumbers;
        CodeGenStartLineNumberText = Resources.CodeGenStartLineNumber;
        CodeGenLineNumberStepText = Resources.CodeGenLineNumberStep;
        CodeGenGenerateCommentsText = Resources.CodeGenGenerateComments;
        CodeGenAllowArcsText = Resources.CodeGenAllowArcs;
        CodeGenFormatCommandsText = Resources.CodeGenFormatCommands;
        CodeGenSetWorkCoordinateSystemText = Resources.CodeGenSetWorkCoordinateSystem;
        CodeGenCoordinateSystemLabelText = Resources.CodeGenCoordinateSystemLabel;
        CodeGenSetAbsoluteCoordinatesText = Resources.CodeGenSetAbsoluteCoordinates;
        CodeGenAllowRelativeCoordinatesText = Resources.CodeGenAllowRelativeCoordinates;
        CodeGenSetZerosAtStartText = Resources.CodeGenSetZerosAtStart;
        CodeGenX0Text = Resources.CodeGenX0;
        CodeGenY0Text = Resources.CodeGenY0;
        CodeGenZ0Text = Resources.CodeGenZ0;
        CodeGenMoveToPointAtEndText = Resources.CodeGenMoveToPointAtEnd;
        CodeGenXText = Resources.CodeGenX;
        CodeGenYText = Resources.CodeGenY;
        CodeGenZText = Resources.CodeGenZ;
        ApplyButtonText = Resources.ButtonApply;
        ResetButtonText = Resources.ButtonReset;
        
        // Update ComboBox items
        UpdateSpindleEnableCommandDisplayNames();
        UpdateSpindleDelayParameterDisplayNames();
        UpdateCoordinateSystemDisplayNames();
    }

    private void UpdateCoordinateSystemDisplayNames()
    {
        foreach (var system in CoordinateSystems)
        {
            system.DisplayName = system.Key switch
            {
                "G54" => Resources.CodeGenCoordinateSystemG54,
                "G55" => Resources.CodeGenCoordinateSystemG55,
                "G56" => Resources.CodeGenCoordinateSystemG56,
                "G57" => Resources.CodeGenCoordinateSystemG57,
                "G58" => Resources.CodeGenCoordinateSystemG58,
                "G59" => Resources.CodeGenCoordinateSystemG59,
                _ => system.Key
            };
        }
    }

    private void UpdateSpindleEnableCommandDisplayNames()
    {
        foreach (var command in SpindleEnableCommands)
        {
            command.DisplayName = command.Key switch
            {
                "M3" => Resources.SpindleEnableCommandM3,
                "M4" => Resources.SpindleEnableCommandM4,
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
                "F" => Resources.SpindleDelayParameterF,
                "P" => Resources.SpindleDelayParameterP,
                "Pxx." => Resources.SpindleDelayParameterPxx,
                _ => parameter.Key
            };
        }
    }

    private void LoadFromSettings()
    {
        var settings = SettingsManager.Current;

        // Load Coolant settings
        _originalAddCoolantCode = settings.AddCoolantCode ?? false;
        _originalEnableCoolantAtStart = settings.EnableCoolantAtStart ?? false;
        _originalDisableCoolantAtEnd = settings.DisableCoolantAtEnd ?? false;

        AddCoolantCode = _originalAddCoolantCode;
        EnableCoolantAtStart = _originalEnableCoolantAtStart;
        DisableCoolantAtEnd = _originalDisableCoolantAtEnd;

        // Load Spindle settings
        _originalAddSpindleCode = settings.AddSpindleCode ?? false;
        _originalSetSpindleSpeed = settings.SetSpindleSpeed ?? false;
        _originalSpindleSpeed = settings.SpindleSpeed ?? "1000";
        _originalEnableSpindleBeforeOperations = settings.EnableSpindleBeforeOperations ?? false;
        _originalSpindleEnableCommand = settings.SpindleEnableCommand ?? "M3";
        _originalAddSpindleDelayAfterEnable = settings.AddSpindleDelayAfterEnable ?? false;
        _originalSpindleDelayParameter = settings.SpindleDelayParameter ?? "F";
        _originalSpindleDelayValue = settings.SpindleDelayValue ?? "2";
        _originalDisableSpindleAfterOperations = settings.DisableSpindleAfterOperations ?? false;

        AddSpindleCode = _originalAddSpindleCode;
        SetSpindleSpeed = _originalSetSpindleSpeed;
        SpindleSpeed = _originalSpindleSpeed;
        EnableSpindleBeforeOperations = _originalEnableSpindleBeforeOperations;
        SelectedSpindleEnableCommand = SpindleEnableCommands.FirstOrDefault(x => x.Key == _originalSpindleEnableCommand)
                                      ?? SpindleEnableCommands.FirstOrDefault(x => x.Key == "M3");
        AddSpindleDelayAfterEnable = _originalAddSpindleDelayAfterEnable;
        SelectedSpindleDelayParameter = SpindleDelayParameters.FirstOrDefault(x => x.Key == _originalSpindleDelayParameter)
                                       ?? SpindleDelayParameters.FirstOrDefault(x => x.Key == "F");
        SpindleDelayValue = _originalSpindleDelayValue;
        DisableSpindleAfterOperations = _originalDisableSpindleAfterOperations;

        // Load Code Generation settings
        _originalUseLineNumbers = settings.UseLineNumbers ?? true;
        _originalStartLineNumber = settings.StartLineNumber ?? "10";
        _originalLineNumberStep = settings.LineNumberStep ?? "10";
        _originalGenerateComments = settings.GenerateComments ?? false;
        _originalAllowArcs = settings.AllowArcs ?? false;
        _originalFormatCommands = settings.FormatCommands ?? false;
        _originalSetWorkCoordinateSystem = settings.SetWorkCoordinateSystem ?? false;
        _originalCoordinateSystem = settings.CoordinateSystem ?? "G54";
        _originalSetAbsoluteCoordinates = settings.SetAbsoluteCoordinates ?? true;
        _originalAllowRelativeCoordinates = settings.AllowRelativeCoordinates ?? false;
        _originalSetZerosAtStart = settings.SetZerosAtStart ?? true;
        _originalX0 = settings.X0 ?? "0";
        _originalY0 = settings.Y0 ?? "0";
        _originalZ0 = settings.Z0 ?? "0";
        _originalMoveToPointAtEnd = settings.MoveToPointAtEnd ?? false;
        _originalX = settings.X ?? "0";
        _originalY = settings.Y ?? "0";
        _originalZ = settings.Z ?? "0";

        UseLineNumbers = _originalUseLineNumbers;
        StartLineNumber = _originalStartLineNumber;
        LineNumberStep = _originalLineNumberStep;
        GenerateComments = _originalGenerateComments;
        AllowArcs = _originalAllowArcs;
        FormatCommands = _originalFormatCommands;
        SetWorkCoordinateSystem = _originalSetWorkCoordinateSystem;
        SelectedCoordinateSystem = CoordinateSystems.FirstOrDefault(x => x.Key == _originalCoordinateSystem)
                                   ?? CoordinateSystems.FirstOrDefault(x => x.Key == "G54");
        SetAbsoluteCoordinates = _originalSetAbsoluteCoordinates;
        AllowRelativeCoordinates = _originalAllowRelativeCoordinates;
        SetZerosAtStart = _originalSetZerosAtStart;
        X0 = _originalX0;
        Y0 = _originalY0;
        Z0 = _originalZ0;
        MoveToPointAtEnd = _originalMoveToPointAtEnd;
        X = _originalX;
        Y = _originalY;
        Z = _originalZ;
    }

    partial void OnAddCoolantCodeChanged(bool value)
    {
        UpdateCoolantSubOptionsEnabled();
        UpdateHeaders();
    }

    private void UpdateCoolantSubOptionsEnabled()
    {
        IsCoolantSubOptionsEnabled = AddCoolantCode;
    }

    partial void OnAddSpindleCodeChanged(bool value)
    {
        UpdateSpindleSubOptionsEnabled();
        UpdateHeaders();
    }

    partial void OnSetSpindleSpeedChanged(bool value)
    {
        IsSpindleSpeedInputEnabled = SetSpindleSpeed && AddSpindleCode;
        UpdateHeaders();
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
        UpdateHeaders();
    }

    partial void OnAddSpindleDelayAfterEnableChanged(bool value)
    {
        IsSpindleDelaySubOptionsEnabled = AddSpindleDelayAfterEnable && EnableSpindleBeforeOperations && AddSpindleCode;
        UpdateHeaders();
    }

    private void UpdateSpindleSubOptionsEnabled()
    {
        IsSpindleSubOptionsEnabled = AddSpindleCode;
        IsSpindleSpeedInputEnabled = SetSpindleSpeed && AddSpindleCode;
        IsSpindleEnableSubOptionsEnabled = EnableSpindleBeforeOperations && AddSpindleCode;
        IsSpindleDelaySubOptionsEnabled = AddSpindleDelayAfterEnable && EnableSpindleBeforeOperations && AddSpindleCode;
    }

    partial void OnUseLineNumbersChanged(bool value)
    {
        IsLineNumberOptionsEnabled = UseLineNumbers;
        UpdateHeaders();
    }

    partial void OnSetWorkCoordinateSystemChanged(bool value)
    {
        IsCoordinateSystemComboEnabled = SetWorkCoordinateSystem;
        UpdateHeaders();
    }

    partial void OnSetZerosAtStartChanged(bool value)
    {
        IsSetZerosOptionsEnabled = SetZerosAtStart;
        UpdateHeaders();
    }

    partial void OnMoveToPointAtEndChanged(bool value)
    {
        IsMoveToPointOptionsEnabled = MoveToPointAtEnd;
        UpdateHeaders();
    }

    // Additional partial methods for tracking changes
    partial void OnEnableCoolantAtStartChanged(bool value) => UpdateHeaders();
    partial void OnDisableCoolantAtEndChanged(bool value) => UpdateHeaders();
    partial void OnSpindleSpeedChanged(string value) => UpdateHeaders();
    partial void OnDisableSpindleAfterOperationsChanged(bool value) => UpdateHeaders();
    partial void OnSpindleDelayValueChanged(string value) => UpdateHeaders();
    partial void OnStartLineNumberChanged(string value) => UpdateHeaders();
    partial void OnLineNumberStepChanged(string value) => UpdateHeaders();
    partial void OnGenerateCommentsChanged(bool value) => UpdateHeaders();
    partial void OnAllowArcsChanged(bool value) => UpdateHeaders();
    partial void OnFormatCommandsChanged(bool value) => UpdateHeaders();
    partial void OnSetAbsoluteCoordinatesChanged(bool value) => UpdateHeaders();
    partial void OnAllowRelativeCoordinatesChanged(bool value) => UpdateHeaders();
    partial void OnX0Changed(string value) => UpdateHeaders();
    partial void OnY0Changed(string value) => UpdateHeaders();
    partial void OnZ0Changed(string value) => UpdateHeaders();
    partial void OnXChanged(string value) => UpdateHeaders();
    partial void OnYChanged(string value) => UpdateHeaders();
    partial void OnZChanged(string value) => UpdateHeaders();
    partial void OnSelectedCoordinateSystemChanged(CoordinateSystemOption? value) => UpdateHeaders();
    partial void OnSelectedSpindleEnableCommandChanged(SpindleEnableCommandOption? value) => UpdateHeaders();
    partial void OnSelectedSpindleDelayParameterChanged(SpindleDelayParameterOption? value) => UpdateHeaders();

    private void UpdateCodeGenSubOptionsEnabled()
    {
        IsLineNumberOptionsEnabled = UseLineNumbers;
        IsCoordinateSystemComboEnabled = SetWorkCoordinateSystem;
        IsSetZerosOptionsEnabled = SetZerosAtStart;
        IsMoveToPointOptionsEnabled = MoveToPointAtEnd;
    }

    private void UpdateHeaders()
    {
        // Code Generation headers
        var useLineNumbersModified = UseLineNumbers != _originalUseLineNumbers;
        CodeGenUseLineNumbersHeaderText = useLineNumbersModified ? $"{Resources.CodeGenUseLineNumbers} *" : Resources.CodeGenUseLineNumbers;
        CodeGenUseLineNumbersFontStyle = useLineNumbersModified ? FontStyle.Italic : FontStyle.Normal;

        var startLineNumberModified = StartLineNumber != _originalStartLineNumber;
        CodeGenStartLineNumberHeaderText = startLineNumberModified ? $"{Resources.CodeGenStartLineNumber} *" : Resources.CodeGenStartLineNumber;
        CodeGenStartLineNumberFontStyle = startLineNumberModified ? FontStyle.Italic : FontStyle.Normal;

        var lineNumberStepModified = LineNumberStep != _originalLineNumberStep;
        CodeGenLineNumberStepHeaderText = lineNumberStepModified ? $"{Resources.CodeGenLineNumberStep} *" : Resources.CodeGenLineNumberStep;
        CodeGenLineNumberStepFontStyle = lineNumberStepModified ? FontStyle.Italic : FontStyle.Normal;

        var generateCommentsModified = GenerateComments != _originalGenerateComments;
        CodeGenGenerateCommentsHeaderText = generateCommentsModified ? $"{Resources.CodeGenGenerateComments} *" : Resources.CodeGenGenerateComments;
        CodeGenGenerateCommentsFontStyle = generateCommentsModified ? FontStyle.Italic : FontStyle.Normal;

        var allowArcsModified = AllowArcs != _originalAllowArcs;
        CodeGenAllowArcsHeaderText = allowArcsModified ? $"{Resources.CodeGenAllowArcs} *" : Resources.CodeGenAllowArcs;
        CodeGenAllowArcsFontStyle = allowArcsModified ? FontStyle.Italic : FontStyle.Normal;

        var formatCommandsModified = FormatCommands != _originalFormatCommands;
        CodeGenFormatCommandsHeaderText = formatCommandsModified ? $"{Resources.CodeGenFormatCommands} *" : Resources.CodeGenFormatCommands;
        CodeGenFormatCommandsFontStyle = formatCommandsModified ? FontStyle.Italic : FontStyle.Normal;

        var setWorkCoordinateSystemModified = SetWorkCoordinateSystem != _originalSetWorkCoordinateSystem;
        CodeGenSetWorkCoordinateSystemHeaderText = setWorkCoordinateSystemModified ? $"{Resources.CodeGenSetWorkCoordinateSystem} *" : Resources.CodeGenSetWorkCoordinateSystem;
        CodeGenSetWorkCoordinateSystemFontStyle = setWorkCoordinateSystemModified ? FontStyle.Italic : FontStyle.Normal;

        var coordinateSystemModified = (SelectedCoordinateSystem?.Key ?? "G54") != _originalCoordinateSystem;
        CodeGenCoordinateSystemLabelHeaderText = coordinateSystemModified ? $"{Resources.CodeGenCoordinateSystemLabel} *" : Resources.CodeGenCoordinateSystemLabel;
        CodeGenCoordinateSystemLabelFontStyle = coordinateSystemModified ? FontStyle.Italic : FontStyle.Normal;

        var setAbsoluteCoordinatesModified = SetAbsoluteCoordinates != _originalSetAbsoluteCoordinates;
        CodeGenSetAbsoluteCoordinatesHeaderText = setAbsoluteCoordinatesModified ? $"{Resources.CodeGenSetAbsoluteCoordinates} *" : Resources.CodeGenSetAbsoluteCoordinates;
        CodeGenSetAbsoluteCoordinatesFontStyle = setAbsoluteCoordinatesModified ? FontStyle.Italic : FontStyle.Normal;

        var allowRelativeCoordinatesModified = AllowRelativeCoordinates != _originalAllowRelativeCoordinates;
        CodeGenAllowRelativeCoordinatesHeaderText = allowRelativeCoordinatesModified ? $"{Resources.CodeGenAllowRelativeCoordinates} *" : Resources.CodeGenAllowRelativeCoordinates;
        CodeGenAllowRelativeCoordinatesFontStyle = allowRelativeCoordinatesModified ? FontStyle.Italic : FontStyle.Normal;

        var setZerosAtStartModified = SetZerosAtStart != _originalSetZerosAtStart;
        CodeGenSetZerosAtStartHeaderText = setZerosAtStartModified ? $"{Resources.CodeGenSetZerosAtStart} *" : Resources.CodeGenSetZerosAtStart;
        CodeGenSetZerosAtStartFontStyle = setZerosAtStartModified ? FontStyle.Italic : FontStyle.Normal;

        var x0Modified = X0 != _originalX0;
        CodeGenX0HeaderText = x0Modified ? $"{Resources.CodeGenX0} *" : Resources.CodeGenX0;
        CodeGenX0FontStyle = x0Modified ? FontStyle.Italic : FontStyle.Normal;

        var y0Modified = Y0 != _originalY0;
        CodeGenY0HeaderText = y0Modified ? $"{Resources.CodeGenY0} *" : Resources.CodeGenY0;
        CodeGenY0FontStyle = y0Modified ? FontStyle.Italic : FontStyle.Normal;

        var z0Modified = Z0 != _originalZ0;
        CodeGenZ0HeaderText = z0Modified ? $"{Resources.CodeGenZ0} *" : Resources.CodeGenZ0;
        CodeGenZ0FontStyle = z0Modified ? FontStyle.Italic : FontStyle.Normal;

        var moveToPointAtEndModified = MoveToPointAtEnd != _originalMoveToPointAtEnd;
        CodeGenMoveToPointAtEndHeaderText = moveToPointAtEndModified ? $"{Resources.CodeGenMoveToPointAtEnd} *" : Resources.CodeGenMoveToPointAtEnd;
        CodeGenMoveToPointAtEndFontStyle = moveToPointAtEndModified ? FontStyle.Italic : FontStyle.Normal;

        var xModified = X != _originalX;
        CodeGenXHeaderText = xModified ? $"{Resources.CodeGenX} *" : Resources.CodeGenX;
        CodeGenXFontStyle = xModified ? FontStyle.Italic : FontStyle.Normal;

        var yModified = Y != _originalY;
        CodeGenYHeaderText = yModified ? $"{Resources.CodeGenY} *" : Resources.CodeGenY;
        CodeGenYFontStyle = yModified ? FontStyle.Italic : FontStyle.Normal;

        var zModified = Z != _originalZ;
        CodeGenZHeaderText = zModified ? $"{Resources.CodeGenZ} *" : Resources.CodeGenZ;
        CodeGenZFontStyle = zModified ? FontStyle.Italic : FontStyle.Normal;

        // Spindle headers
        var addSpindleCodeModified = AddSpindleCode != _originalAddSpindleCode;
        SpindleAddCodeHeaderText = addSpindleCodeModified ? $"{Resources.SpindleAddCode} *" : Resources.SpindleAddCode;
        SpindleAddCodeFontStyle = addSpindleCodeModified ? FontStyle.Italic : FontStyle.Normal;

        var setSpindleSpeedModified = SetSpindleSpeed != _originalSetSpindleSpeed;
        SpindleSetSpeedHeaderText = setSpindleSpeedModified ? $"{Resources.SpindleSetSpeed} *" : Resources.SpindleSetSpeed;
        SpindleSetSpeedFontStyle = setSpindleSpeedModified ? FontStyle.Italic : FontStyle.Normal;

        var spindleSpeedModified = SpindleSpeed != _originalSpindleSpeed;
        SpindleSpeedLabelHeaderText = spindleSpeedModified ? $"{Resources.SpindleSpeedLabel} *" : Resources.SpindleSpeedLabel;
        SpindleSpeedLabelFontStyle = spindleSpeedModified ? FontStyle.Italic : FontStyle.Normal;

        var enableSpindleBeforeOperationsModified = EnableSpindleBeforeOperations != _originalEnableSpindleBeforeOperations;
        SpindleEnableBeforeOperationsHeaderText = enableSpindleBeforeOperationsModified ? $"{Resources.SpindleEnableBeforeOperations} *" : Resources.SpindleEnableBeforeOperations;
        SpindleEnableBeforeOperationsFontStyle = enableSpindleBeforeOperationsModified ? FontStyle.Italic : FontStyle.Normal;

        var spindleEnableCommandModified = (SelectedSpindleEnableCommand?.Key ?? "M3") != _originalSpindleEnableCommand;
        SpindleEnableCommandLabelHeaderText = spindleEnableCommandModified ? $"{Resources.SpindleEnableCommandLabel} *" : Resources.SpindleEnableCommandLabel;
        SpindleEnableCommandLabelFontStyle = spindleEnableCommandModified ? FontStyle.Italic : FontStyle.Normal;

        var addSpindleDelayAfterEnableModified = AddSpindleDelayAfterEnable != _originalAddSpindleDelayAfterEnable;
        SpindleAddDelayAfterEnableHeaderText = addSpindleDelayAfterEnableModified ? $"{Resources.SpindleAddDelayAfterEnable} *" : Resources.SpindleAddDelayAfterEnable;
        SpindleAddDelayAfterEnableFontStyle = addSpindleDelayAfterEnableModified ? FontStyle.Italic : FontStyle.Normal;

        var spindleDelayParameterModified = (SelectedSpindleDelayParameter?.Key ?? "F") != _originalSpindleDelayParameter;
        SpindleDelayParameterLabelHeaderText = spindleDelayParameterModified ? $"{Resources.SpindleDelayParameterLabel} *" : Resources.SpindleDelayParameterLabel;
        SpindleDelayParameterLabelFontStyle = spindleDelayParameterModified ? FontStyle.Italic : FontStyle.Normal;

        var spindleDelayValueModified = SpindleDelayValue != _originalSpindleDelayValue;
        SpindleDelayValueLabelHeaderText = spindleDelayValueModified ? $"{Resources.SpindleDelayValueLabel} *" : Resources.SpindleDelayValueLabel;
        SpindleDelayValueLabelFontStyle = spindleDelayValueModified ? FontStyle.Italic : FontStyle.Normal;

        var disableSpindleAfterOperationsModified = DisableSpindleAfterOperations != _originalDisableSpindleAfterOperations;
        SpindleDisableAfterOperationsHeaderText = disableSpindleAfterOperationsModified ? $"{Resources.SpindleDisableAfterOperations} *" : Resources.SpindleDisableAfterOperations;
        SpindleDisableAfterOperationsFontStyle = disableSpindleAfterOperationsModified ? FontStyle.Italic : FontStyle.Normal;

        // Coolant headers
        var addCoolantCodeModified = AddCoolantCode != _originalAddCoolantCode;
        CoolantAddCodeHeaderText = addCoolantCodeModified ? $"{Resources.CoolantAddCode} *" : Resources.CoolantAddCode;
        CoolantAddCodeFontStyle = addCoolantCodeModified ? FontStyle.Italic : FontStyle.Normal;

        var enableCoolantAtStartModified = EnableCoolantAtStart != _originalEnableCoolantAtStart;
        CoolantEnableAtStartHeaderText = enableCoolantAtStartModified ? $"{Resources.CoolantEnableAtStart} *" : Resources.CoolantEnableAtStart;
        CoolantEnableAtStartFontStyle = enableCoolantAtStartModified ? FontStyle.Italic : FontStyle.Normal;

        var disableCoolantAtEndModified = DisableCoolantAtEnd != _originalDisableCoolantAtEnd;
        CoolantDisableAtEndHeaderText = disableCoolantAtEndModified ? $"{Resources.CoolantDisableAtEnd} *" : Resources.CoolantDisableAtEnd;
        CoolantDisableAtEndFontStyle = disableCoolantAtEndModified ? FontStyle.Italic : FontStyle.Normal;
    }

    [RelayCommand]
    private void Apply()
    {
        var settings = SettingsManager.Current;

        // Save Coolant settings
        settings.AddCoolantCode = AddCoolantCode;
        settings.EnableCoolantAtStart = EnableCoolantAtStart;
        settings.DisableCoolantAtEnd = DisableCoolantAtEnd;

        // Save Spindle settings
        settings.AddSpindleCode = AddSpindleCode;
        settings.SetSpindleSpeed = SetSpindleSpeed;
        settings.SpindleSpeed = SpindleSpeed;
        settings.EnableSpindleBeforeOperations = EnableSpindleBeforeOperations;
        settings.SpindleEnableCommand = SelectedSpindleEnableCommand?.Key ?? "M3";
        settings.AddSpindleDelayAfterEnable = AddSpindleDelayAfterEnable;
        settings.SpindleDelayParameter = SelectedSpindleDelayParameter?.Key ?? "F";
        settings.SpindleDelayValue = SpindleDelayValue;
        settings.DisableSpindleAfterOperations = DisableSpindleAfterOperations;

        // Save Code Generation settings
        settings.UseLineNumbers = UseLineNumbers;
        settings.StartLineNumber = StartLineNumber;
        settings.LineNumberStep = LineNumberStep;
        settings.GenerateComments = GenerateComments;
        settings.AllowArcs = AllowArcs;
        settings.FormatCommands = FormatCommands;
        settings.SetWorkCoordinateSystem = SetWorkCoordinateSystem;
        settings.CoordinateSystem = SelectedCoordinateSystem?.Key ?? "G54";
        settings.SetAbsoluteCoordinates = SetAbsoluteCoordinates;
        settings.AllowRelativeCoordinates = AllowRelativeCoordinates;
        settings.SetZerosAtStart = SetZerosAtStart;
        settings.X0 = X0;
        settings.Y0 = Y0;
        settings.Z0 = Z0;
        settings.MoveToPointAtEnd = MoveToPointAtEnd;
        settings.X = X;
        settings.Y = Y;
        settings.Z = Z;

        // Save to disk
        SettingsManager.SaveCurrent();

        // Update original values
        _originalAddCoolantCode = AddCoolantCode;
        _originalEnableCoolantAtStart = EnableCoolantAtStart;
        _originalDisableCoolantAtEnd = DisableCoolantAtEnd;
        _originalAddSpindleCode = AddSpindleCode;
        _originalSetSpindleSpeed = SetSpindleSpeed;
        _originalSpindleSpeed = SpindleSpeed;
        _originalEnableSpindleBeforeOperations = EnableSpindleBeforeOperations;
        _originalSpindleEnableCommand = SelectedSpindleEnableCommand?.Key ?? "M3";
        _originalAddSpindleDelayAfterEnable = AddSpindleDelayAfterEnable;
        _originalSpindleDelayParameter = SelectedSpindleDelayParameter?.Key ?? "F";
        _originalSpindleDelayValue = SpindleDelayValue;
        _originalDisableSpindleAfterOperations = DisableSpindleAfterOperations;
        _originalUseLineNumbers = UseLineNumbers;
        _originalStartLineNumber = StartLineNumber;
        _originalLineNumberStep = LineNumberStep;
        _originalGenerateComments = GenerateComments;
        _originalAllowArcs = AllowArcs;
        _originalFormatCommands = FormatCommands;
        _originalSetWorkCoordinateSystem = SetWorkCoordinateSystem;
        _originalCoordinateSystem = SelectedCoordinateSystem?.Key ?? "G54";
        _originalSetAbsoluteCoordinates = SetAbsoluteCoordinates;
        _originalAllowRelativeCoordinates = AllowRelativeCoordinates;
        _originalSetZerosAtStart = SetZerosAtStart;
        _originalX0 = X0;
        _originalY0 = Y0;
        _originalZ0 = Z0;
        _originalMoveToPointAtEnd = MoveToPointAtEnd;
        _originalX = X;
        _originalY = Y;
        _originalZ = Z;
        UpdateHeaders();
    }

    [RelayCommand]
    private void Reset()
    {
        // Reload from settings
        SettingsManager.Reload();
        LoadFromSettings();
        
        // Update enabled states
        UpdateCoolantSubOptionsEnabled();
        UpdateSpindleSubOptionsEnabled();
        UpdateCodeGenSubOptionsEnabled();
        UpdateHeaders();
    }

    public void Dispose()
    {
        Resources.CultureChanged -= OnCultureChanged;
    }
}

public partial class SpindleEnableCommandOption : ObservableObject
{
    public string Key { get; }

    [ObservableProperty]
    private string _displayName;

    public SpindleEnableCommandOption(string key, string displayName)
    {
        Key = key;
        _displayName = displayName;
    }
}

public partial class SpindleDelayParameterOption : ObservableObject
{
    public string Key { get; }

    [ObservableProperty]
    private string _displayName;

    public SpindleDelayParameterOption(string key, string displayName)
    {
        Key = key;
        _displayName = displayName;
    }
}

public partial class CoordinateSystemOption : ObservableObject
{
    public string Key { get; }

    [ObservableProperty]
    private string _displayName;

    public CoordinateSystemOption(string key, string displayName)
    {
        Key = key;
        _displayName = displayName;
    }
}

