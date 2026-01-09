using System;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using MiniCAM.Core.Localization;
using MiniCAM.Core.Settings;

namespace MiniCAM.Core.ViewModels;

/// <summary>
/// View model for the Code Generation settings tab.
/// Manages code generation options such as line numbers, coordinate systems, and movement settings.
/// </summary>
public partial class CodeGenerationTabViewModel : SettingsTabViewModelBase
{
    // Property name constants for tracking
    private const string PropertyUseLineNumbers = nameof(UseLineNumbers);
    private const string PropertyStartLineNumber = nameof(StartLineNumber);
    private const string PropertyLineNumberStep = nameof(LineNumberStep);
    private const string PropertyGenerateComments = nameof(GenerateComments);
    private const string PropertyAllowArcs = nameof(AllowArcs);
    private const string PropertyFormatCommands = nameof(FormatCommands);
    private const string PropertySetWorkCoordinateSystem = nameof(SetWorkCoordinateSystem);
    private const string PropertyCoordinateSystem = nameof(SelectedCoordinateSystem);
    private const string PropertySetAbsoluteCoordinates = nameof(SetAbsoluteCoordinates);
    private const string PropertyAllowRelativeCoordinates = nameof(AllowRelativeCoordinates);
    private const string PropertySetZerosAtStart = nameof(SetZerosAtStart);
    private const string PropertyX0 = nameof(X0);
    private const string PropertyY0 = nameof(Y0);
    private const string PropertyZ0 = nameof(Z0);
    private const string PropertyMoveToPointAtEnd = nameof(MoveToPointAtEnd);
    private const string PropertyX = nameof(X);
    private const string PropertyY = nameof(Y);
    private const string PropertyZ = nameof(Z);

    // Code generation properties
    [ObservableProperty]
    private bool _useLineNumbers = CodeGenerationDefaults.UseLineNumbers;

    [ObservableProperty]
    private string _startLineNumber = CodeGenerationDefaults.StartLineNumber;

    [ObservableProperty]
    private string _lineNumberStep = CodeGenerationDefaults.LineNumberStep;

    [ObservableProperty]
    private bool _generateComments = CodeGenerationDefaults.GenerateComments;

    [ObservableProperty]
    private bool _allowArcs = CodeGenerationDefaults.AllowArcs;

    [ObservableProperty]
    private bool _formatCommands = CodeGenerationDefaults.FormatCommands;

    [ObservableProperty]
    private bool _setWorkCoordinateSystem = CodeGenerationDefaults.SetWorkCoordinateSystem;

    [ObservableProperty]
    private CoordinateSystemOption? _selectedCoordinateSystem;

    [ObservableProperty]
    private bool _setAbsoluteCoordinates = CodeGenerationDefaults.SetAbsoluteCoordinates;

    [ObservableProperty]
    private bool _allowRelativeCoordinates = CodeGenerationDefaults.AllowRelativeCoordinates;

    [ObservableProperty]
    private bool _setZerosAtStart = CodeGenerationDefaults.SetZerosAtStart;

    [ObservableProperty]
    private string _x0 = CodeGenerationDefaults.Coordinate;

    [ObservableProperty]
    private string _y0 = CodeGenerationDefaults.Coordinate;

    [ObservableProperty]
    private string _z0 = CodeGenerationDefaults.Coordinate;

    [ObservableProperty]
    private bool _moveToPointAtEnd = CodeGenerationDefaults.MoveToPointAtEnd;

    [ObservableProperty]
    private string _x = CodeGenerationDefaults.Coordinate;

    [ObservableProperty]
    private string _y = CodeGenerationDefaults.Coordinate;

    [ObservableProperty]
    private string _z = CodeGenerationDefaults.Coordinate;

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
    private readonly OptionCollectionHelper<CoordinateSystemOption> _coordinateSystemsHelper;

    // Header text and font style properties
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

    public CodeGenerationTabViewModel()
    {
        _coordinateSystemsHelper = new OptionCollectionHelper<CoordinateSystemOption>(CoordinateSystems);
        BuildCoordinateSystems();
        LoadFromSettings(SettingsManager.Current);
        RegisterTrackedProperties();
        UpdateCodeGenSubOptionsEnabled();
        HeaderTracker.UpdateAllHeadersImmediate();
    }

    private void RegisterTrackedProperties()
    {
        HeaderTracker.Register(
            PropertyUseLineNumbers,
            UseLineNumbers,
            () => Resources.CodeGenUseLineNumbers,
            value => CodeGenUseLineNumbersHeaderText = value,
            value => CodeGenUseLineNumbersFontStyle = value);

        HeaderTracker.Register(
            PropertyStartLineNumber,
            StartLineNumber,
            () => Resources.CodeGenStartLineNumber,
            value => CodeGenStartLineNumberHeaderText = value,
            value => CodeGenStartLineNumberFontStyle = value);

        HeaderTracker.Register(
            PropertyLineNumberStep,
            LineNumberStep,
            () => Resources.CodeGenLineNumberStep,
            value => CodeGenLineNumberStepHeaderText = value,
            value => CodeGenLineNumberStepFontStyle = value);

        HeaderTracker.Register(
            PropertyGenerateComments,
            GenerateComments,
            () => Resources.CodeGenGenerateComments,
            value => CodeGenGenerateCommentsHeaderText = value,
            value => CodeGenGenerateCommentsFontStyle = value);

        HeaderTracker.Register(
            PropertyAllowArcs,
            AllowArcs,
            () => Resources.CodeGenAllowArcs,
            value => CodeGenAllowArcsHeaderText = value,
            value => CodeGenAllowArcsFontStyle = value);

        HeaderTracker.Register(
            PropertyFormatCommands,
            FormatCommands,
            () => Resources.CodeGenFormatCommands,
            value => CodeGenFormatCommandsHeaderText = value,
            value => CodeGenFormatCommandsFontStyle = value);

        HeaderTracker.Register(
            PropertySetWorkCoordinateSystem,
            SetWorkCoordinateSystem,
            () => Resources.CodeGenSetWorkCoordinateSystem,
            value => CodeGenSetWorkCoordinateSystemHeaderText = value,
            value => CodeGenSetWorkCoordinateSystemFontStyle = value);

        HeaderTracker.Register(
            PropertyCoordinateSystem,
            SelectedCoordinateSystem?.Key ?? Settings.CoordinateSystems.Default,
            () => Resources.CodeGenCoordinateSystemLabel,
            value => CodeGenCoordinateSystemLabelHeaderText = value,
            value => CodeGenCoordinateSystemLabelFontStyle = value);

        HeaderTracker.Register(
            PropertySetAbsoluteCoordinates,
            SetAbsoluteCoordinates,
            () => Resources.CodeGenSetAbsoluteCoordinates,
            value => CodeGenSetAbsoluteCoordinatesHeaderText = value,
            value => CodeGenSetAbsoluteCoordinatesFontStyle = value);

        HeaderTracker.Register(
            PropertyAllowRelativeCoordinates,
            AllowRelativeCoordinates,
            () => Resources.CodeGenAllowRelativeCoordinates,
            value => CodeGenAllowRelativeCoordinatesHeaderText = value,
            value => CodeGenAllowRelativeCoordinatesFontStyle = value);

        HeaderTracker.Register(
            PropertySetZerosAtStart,
            SetZerosAtStart,
            () => Resources.CodeGenSetZerosAtStart,
            value => CodeGenSetZerosAtStartHeaderText = value,
            value => CodeGenSetZerosAtStartFontStyle = value);

        HeaderTracker.Register(
            PropertyX0,
            X0,
            () => Resources.CodeGenX0,
            value => CodeGenX0HeaderText = value,
            value => CodeGenX0FontStyle = value);

        HeaderTracker.Register(
            PropertyY0,
            Y0,
            () => Resources.CodeGenY0,
            value => CodeGenY0HeaderText = value,
            value => CodeGenY0FontStyle = value);

        HeaderTracker.Register(
            PropertyZ0,
            Z0,
            () => Resources.CodeGenZ0,
            value => CodeGenZ0HeaderText = value,
            value => CodeGenZ0FontStyle = value);

        HeaderTracker.Register(
            PropertyMoveToPointAtEnd,
            MoveToPointAtEnd,
            () => Resources.CodeGenMoveToPointAtEnd,
            value => CodeGenMoveToPointAtEndHeaderText = value,
            value => CodeGenMoveToPointAtEndFontStyle = value);

        HeaderTracker.Register(
            PropertyX,
            X,
            () => Resources.CodeGenX,
            value => CodeGenXHeaderText = value,
            value => CodeGenXFontStyle = value);

        HeaderTracker.Register(
            PropertyY,
            Y,
            () => Resources.CodeGenY,
            value => CodeGenYHeaderText = value,
            value => CodeGenYFontStyle = value);

        HeaderTracker.Register(
            PropertyZ,
            Z,
            () => Resources.CodeGenZ,
            value => CodeGenZHeaderText = value,
            value => CodeGenZFontStyle = value);
    }

    protected override void UpdateLocalizedStrings()
    {
        UpdateCoordinateSystemDisplayNames();
        HeaderTracker.UpdateAllHeaders();
    }

    private void BuildCoordinateSystems()
    {
        _coordinateSystemsHelper.Clear();
        _coordinateSystemsHelper
            .Add(Settings.CoordinateSystems.G54, () => Resources.CodeGenCoordinateSystemG54, (k, d) => new CoordinateSystemOption(k, d))
            .Add(Settings.CoordinateSystems.G55, () => Resources.CodeGenCoordinateSystemG55, (k, d) => new CoordinateSystemOption(k, d))
            .Add(Settings.CoordinateSystems.G56, () => Resources.CodeGenCoordinateSystemG56, (k, d) => new CoordinateSystemOption(k, d))
            .Add(Settings.CoordinateSystems.G57, () => Resources.CodeGenCoordinateSystemG57, (k, d) => new CoordinateSystemOption(k, d))
            .Add(Settings.CoordinateSystems.G58, () => Resources.CodeGenCoordinateSystemG58, (k, d) => new CoordinateSystemOption(k, d))
            .Add(Settings.CoordinateSystems.G59, () => Resources.CodeGenCoordinateSystemG59, (k, d) => new CoordinateSystemOption(k, d));
    }

    private void UpdateCoordinateSystemDisplayNames()
    {
        _coordinateSystemsHelper.UpdateDisplayNames();
    }

    partial void OnUseLineNumbersChanged(bool value)
    {
        IsLineNumberOptionsEnabled = UseLineNumbers;
        HeaderTracker.Update(PropertyUseLineNumbers, value);
    }

    partial void OnSetWorkCoordinateSystemChanged(bool value)
    {
        IsCoordinateSystemComboEnabled = SetWorkCoordinateSystem;
        HeaderTracker.Update(PropertySetWorkCoordinateSystem, value);
    }

    partial void OnSetZerosAtStartChanged(bool value)
    {
        IsSetZerosOptionsEnabled = SetZerosAtStart;
        HeaderTracker.Update(PropertySetZerosAtStart, value);
    }

    partial void OnMoveToPointAtEndChanged(bool value)
    {
        IsMoveToPointOptionsEnabled = MoveToPointAtEnd;
        HeaderTracker.Update(PropertyMoveToPointAtEnd, value);
    }

    // Additional partial methods for tracking changes
    partial void OnStartLineNumberChanged(string value)
    {
        HeaderTracker.Update(PropertyStartLineNumber, value);
    }

    partial void OnLineNumberStepChanged(string value)
    {
        HeaderTracker.Update(PropertyLineNumberStep, value);
    }

    partial void OnGenerateCommentsChanged(bool value)
    {
        HeaderTracker.Update(PropertyGenerateComments, value);
    }

    partial void OnAllowArcsChanged(bool value)
    {
        HeaderTracker.Update(PropertyAllowArcs, value);
    }

    partial void OnFormatCommandsChanged(bool value)
    {
        HeaderTracker.Update(PropertyFormatCommands, value);
    }

    partial void OnSetAbsoluteCoordinatesChanged(bool value)
    {
        HeaderTracker.Update(PropertySetAbsoluteCoordinates, value);
    }

    partial void OnAllowRelativeCoordinatesChanged(bool value)
    {
        HeaderTracker.Update(PropertyAllowRelativeCoordinates, value);
    }

    partial void OnX0Changed(string value)
    {
        HeaderTracker.Update(PropertyX0, value);
    }

    partial void OnY0Changed(string value)
    {
        HeaderTracker.Update(PropertyY0, value);
    }

    partial void OnZ0Changed(string value)
    {
        HeaderTracker.Update(PropertyZ0, value);
    }

    partial void OnXChanged(string value)
    {
        HeaderTracker.Update(PropertyX, value);
    }

    partial void OnYChanged(string value)
    {
        HeaderTracker.Update(PropertyY, value);
    }

    partial void OnZChanged(string value)
    {
        HeaderTracker.Update(PropertyZ, value);
    }

    partial void OnSelectedCoordinateSystemChanged(CoordinateSystemOption? value)
    {
        HeaderTracker.Update(PropertyCoordinateSystem, value?.Key ?? Settings.CoordinateSystems.Default);
    }

    private void UpdateCodeGenSubOptionsEnabled()
    {
        IsLineNumberOptionsEnabled = UseLineNumbers;
        IsCoordinateSystemComboEnabled = SetWorkCoordinateSystem;
        IsSetZerosOptionsEnabled = SetZerosAtStart;
        IsMoveToPointOptionsEnabled = MoveToPointAtEnd;
    }

    public override void LoadFromSettings(AppSettings settings)
    {
        var useLineNumbers = settings.GetValueOrDefault(s => s.UseLineNumbers, CodeGenerationDefaults.UseLineNumbers);
        var startLineNumber = settings.GetStringOrDefault(s => s.StartLineNumber, CodeGenerationDefaults.StartLineNumber);
        var lineNumberStep = settings.GetStringOrDefault(s => s.LineNumberStep, CodeGenerationDefaults.LineNumberStep);
        var generateComments = settings.GetValueOrDefault(s => s.GenerateComments, CodeGenerationDefaults.GenerateComments);
        var allowArcs = settings.GetValueOrDefault(s => s.AllowArcs, CodeGenerationDefaults.AllowArcs);
        var formatCommands = settings.GetValueOrDefault(s => s.FormatCommands, CodeGenerationDefaults.FormatCommands);
        var setWorkCoordinateSystem = settings.GetValueOrDefault(s => s.SetWorkCoordinateSystem, CodeGenerationDefaults.SetWorkCoordinateSystem);
        var coordinateSystem = settings.GetStringOrDefault(s => s.CoordinateSystem, Settings.CoordinateSystems.Default);
        var setAbsoluteCoordinates = settings.GetValueOrDefault(s => s.SetAbsoluteCoordinates, CodeGenerationDefaults.SetAbsoluteCoordinates);
        var allowRelativeCoordinates = settings.GetValueOrDefault(s => s.AllowRelativeCoordinates, CodeGenerationDefaults.AllowRelativeCoordinates);
        var setZerosAtStart = settings.GetValueOrDefault(s => s.SetZerosAtStart, CodeGenerationDefaults.SetZerosAtStart);
        var x0 = settings.GetStringOrDefault(s => s.X0, CodeGenerationDefaults.Coordinate);
        var y0 = settings.GetStringOrDefault(s => s.Y0, CodeGenerationDefaults.Coordinate);
        var z0 = settings.GetStringOrDefault(s => s.Z0, CodeGenerationDefaults.Coordinate);
        var moveToPointAtEnd = settings.GetValueOrDefault(s => s.MoveToPointAtEnd, CodeGenerationDefaults.MoveToPointAtEnd);
        var x = settings.GetStringOrDefault(s => s.X, CodeGenerationDefaults.Coordinate);
        var y = settings.GetStringOrDefault(s => s.Y, CodeGenerationDefaults.Coordinate);
        var z = settings.GetStringOrDefault(s => s.Z, CodeGenerationDefaults.Coordinate);

        UseLineNumbers = useLineNumbers;
        StartLineNumber = startLineNumber;
        LineNumberStep = lineNumberStep;
        GenerateComments = generateComments;
        AllowArcs = allowArcs;
        FormatCommands = formatCommands;
        SetWorkCoordinateSystem = setWorkCoordinateSystem;
        SelectedCoordinateSystem = _coordinateSystemsHelper.FindByKey(coordinateSystem)
                                   ?? _coordinateSystemsHelper.FindByKey(Settings.CoordinateSystems.Default);
        SetAbsoluteCoordinates = setAbsoluteCoordinates;
        AllowRelativeCoordinates = allowRelativeCoordinates;
        SetZerosAtStart = setZerosAtStart;
        X0 = x0;
        Y0 = y0;
        Z0 = z0;
        MoveToPointAtEnd = moveToPointAtEnd;
        X = x;
        Y = y;
        Z = z;

        // Update original values in tracker
        HeaderTracker.UpdateOriginal(PropertyUseLineNumbers, useLineNumbers);
        HeaderTracker.UpdateOriginal(PropertyStartLineNumber, startLineNumber);
        HeaderTracker.UpdateOriginal(PropertyLineNumberStep, lineNumberStep);
        HeaderTracker.UpdateOriginal(PropertyGenerateComments, generateComments);
        HeaderTracker.UpdateOriginal(PropertyAllowArcs, allowArcs);
        HeaderTracker.UpdateOriginal(PropertyFormatCommands, formatCommands);
        HeaderTracker.UpdateOriginal(PropertySetWorkCoordinateSystem, setWorkCoordinateSystem);
        HeaderTracker.UpdateOriginal(PropertyCoordinateSystem, coordinateSystem);
        HeaderTracker.UpdateOriginal(PropertySetAbsoluteCoordinates, setAbsoluteCoordinates);
        HeaderTracker.UpdateOriginal(PropertyAllowRelativeCoordinates, allowRelativeCoordinates);
        HeaderTracker.UpdateOriginal(PropertySetZerosAtStart, setZerosAtStart);
        HeaderTracker.UpdateOriginal(PropertyX0, x0);
        HeaderTracker.UpdateOriginal(PropertyY0, y0);
        HeaderTracker.UpdateOriginal(PropertyZ0, z0);
        HeaderTracker.UpdateOriginal(PropertyMoveToPointAtEnd, moveToPointAtEnd);
        HeaderTracker.UpdateOriginal(PropertyX, x);
        HeaderTracker.UpdateOriginal(PropertyY, y);
        HeaderTracker.UpdateOriginal(PropertyZ, z);
    }

    public override void SaveToSettings(AppSettings settings)
    {
        settings.UseLineNumbers = UseLineNumbers;
        settings.StartLineNumber = StartLineNumber;
        settings.LineNumberStep = LineNumberStep;
        settings.GenerateComments = GenerateComments;
        settings.AllowArcs = AllowArcs;
        settings.FormatCommands = FormatCommands;
        settings.SetWorkCoordinateSystem = SetWorkCoordinateSystem;
        settings.CoordinateSystem = SelectedCoordinateSystem?.Key ?? Settings.CoordinateSystems.Default;
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
    }

    public override void ResetToOriginal()
    {
        UseLineNumbers = HeaderTracker.GetOriginalValue<bool>(PropertyUseLineNumbers);
        StartLineNumber = HeaderTracker.GetOriginalValue<string>(PropertyStartLineNumber) ?? CodeGenerationDefaults.StartLineNumber;
        LineNumberStep = HeaderTracker.GetOriginalValue<string>(PropertyLineNumberStep) ?? CodeGenerationDefaults.LineNumberStep;
        GenerateComments = HeaderTracker.GetOriginalValue<bool>(PropertyGenerateComments);
        AllowArcs = HeaderTracker.GetOriginalValue<bool>(PropertyAllowArcs);
        FormatCommands = HeaderTracker.GetOriginalValue<bool>(PropertyFormatCommands);
        SetWorkCoordinateSystem = HeaderTracker.GetOriginalValue<bool>(PropertySetWorkCoordinateSystem);
        var originalCoordinateSystem = HeaderTracker.GetOriginalValue<string>(PropertyCoordinateSystem) ?? Settings.CoordinateSystems.Default;
        SelectedCoordinateSystem = _coordinateSystemsHelper.FindByKey(originalCoordinateSystem)
                                   ?? _coordinateSystemsHelper.FindByKey(Settings.CoordinateSystems.Default);
        SetAbsoluteCoordinates = HeaderTracker.GetOriginalValue<bool>(PropertySetAbsoluteCoordinates);
        AllowRelativeCoordinates = HeaderTracker.GetOriginalValue<bool>(PropertyAllowRelativeCoordinates);
        SetZerosAtStart = HeaderTracker.GetOriginalValue<bool>(PropertySetZerosAtStart);
        X0 = HeaderTracker.GetOriginalValue<string>(PropertyX0) ?? CodeGenerationDefaults.Coordinate;
        Y0 = HeaderTracker.GetOriginalValue<string>(PropertyY0) ?? CodeGenerationDefaults.Coordinate;
        Z0 = HeaderTracker.GetOriginalValue<string>(PropertyZ0) ?? CodeGenerationDefaults.Coordinate;
        MoveToPointAtEnd = HeaderTracker.GetOriginalValue<bool>(PropertyMoveToPointAtEnd);
        X = HeaderTracker.GetOriginalValue<string>(PropertyX) ?? CodeGenerationDefaults.Coordinate;
        Y = HeaderTracker.GetOriginalValue<string>(PropertyY) ?? CodeGenerationDefaults.Coordinate;
        Z = HeaderTracker.GetOriginalValue<string>(PropertyZ) ?? CodeGenerationDefaults.Coordinate;
        UpdateCodeGenSubOptionsEnabled();
        HeaderTracker.UpdateAllHeadersImmediate();
    }
}
