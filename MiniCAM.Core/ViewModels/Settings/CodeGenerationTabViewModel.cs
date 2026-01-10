using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using MiniCAM.Core.Localization;
using MiniCAM.Core.Settings;
using MiniCAM.Core.ViewModels.Base;
using MiniCAM.Core.ViewModels.Common;
using MiniCAM.Core.ViewModels.Settings.Options;

namespace MiniCAM.Core.ViewModels.Settings;

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
    private const string PropertyDecimalPlaces = nameof(DecimalPlaces);

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

    [ObservableProperty]
    private string _decimalPlaces = CodeGenerationDefaults.DecimalPlaces.ToString();

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

    // Property headers dictionary for change tracking
    private readonly Dictionary<string, PropertyHeaderViewModel> _headers = new();

    private readonly ISettingsService _settingsService;

    // Public properties for header access
    public PropertyHeaderViewModel CodeGenUseLineNumbersHeader => _headers[PropertyUseLineNumbers];
    public PropertyHeaderViewModel CodeGenStartLineNumberHeader => _headers[PropertyStartLineNumber];
    public PropertyHeaderViewModel CodeGenLineNumberStepHeader => _headers[PropertyLineNumberStep];
    public PropertyHeaderViewModel CodeGenGenerateCommentsHeader => _headers[PropertyGenerateComments];
    public PropertyHeaderViewModel CodeGenAllowArcsHeader => _headers[PropertyAllowArcs];
    public PropertyHeaderViewModel CodeGenFormatCommandsHeader => _headers[PropertyFormatCommands];
    public PropertyHeaderViewModel CodeGenSetWorkCoordinateSystemHeader => _headers[PropertySetWorkCoordinateSystem];
    public PropertyHeaderViewModel CodeGenCoordinateSystemLabelHeader => _headers[PropertyCoordinateSystem];
    public PropertyHeaderViewModel CodeGenSetAbsoluteCoordinatesHeader => _headers[PropertySetAbsoluteCoordinates];
    public PropertyHeaderViewModel CodeGenAllowRelativeCoordinatesHeader => _headers[PropertyAllowRelativeCoordinates];
    public PropertyHeaderViewModel CodeGenSetZerosAtStartHeader => _headers[PropertySetZerosAtStart];
    public PropertyHeaderViewModel CodeGenX0Header => _headers[PropertyX0];
    public PropertyHeaderViewModel CodeGenY0Header => _headers[PropertyY0];
    public PropertyHeaderViewModel CodeGenZ0Header => _headers[PropertyZ0];
    public PropertyHeaderViewModel CodeGenMoveToPointAtEndHeader => _headers[PropertyMoveToPointAtEnd];
    public PropertyHeaderViewModel CodeGenXHeader => _headers[PropertyX];
    public PropertyHeaderViewModel CodeGenYHeader => _headers[PropertyY];
    public PropertyHeaderViewModel CodeGenZHeader => _headers[PropertyZ];
    public PropertyHeaderViewModel CodeGenDecimalPlacesHeader => _headers[PropertyDecimalPlaces];

    public CodeGenerationTabViewModel(ISettingsService settingsService)
    {
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
        _coordinateSystemsHelper = new OptionCollectionHelper<CoordinateSystemOption>(CoordinateSystems);
        
        // Initialize headers
        _headers[PropertyUseLineNumbers] = new PropertyHeaderViewModel(Resources.CodeGenUseLineNumbers);
        _headers[PropertyStartLineNumber] = new PropertyHeaderViewModel(Resources.CodeGenStartLineNumber);
        _headers[PropertyLineNumberStep] = new PropertyHeaderViewModel(Resources.CodeGenLineNumberStep);
        _headers[PropertyGenerateComments] = new PropertyHeaderViewModel(Resources.CodeGenGenerateComments);
        _headers[PropertyAllowArcs] = new PropertyHeaderViewModel(Resources.CodeGenAllowArcs);
        _headers[PropertyFormatCommands] = new PropertyHeaderViewModel(Resources.CodeGenFormatCommands);
        _headers[PropertySetWorkCoordinateSystem] = new PropertyHeaderViewModel(Resources.CodeGenSetWorkCoordinateSystem);
        _headers[PropertyCoordinateSystem] = new PropertyHeaderViewModel(Resources.CodeGenCoordinateSystemLabel);
        _headers[PropertySetAbsoluteCoordinates] = new PropertyHeaderViewModel(Resources.CodeGenSetAbsoluteCoordinates);
        _headers[PropertyAllowRelativeCoordinates] = new PropertyHeaderViewModel(Resources.CodeGenAllowRelativeCoordinates);
        _headers[PropertySetZerosAtStart] = new PropertyHeaderViewModel(Resources.CodeGenSetZerosAtStart);
        _headers[PropertyX0] = new PropertyHeaderViewModel(Resources.CodeGenX0);
        _headers[PropertyY0] = new PropertyHeaderViewModel(Resources.CodeGenY0);
        _headers[PropertyZ0] = new PropertyHeaderViewModel(Resources.CodeGenZ0);
        _headers[PropertyMoveToPointAtEnd] = new PropertyHeaderViewModel(Resources.CodeGenMoveToPointAtEnd);
        _headers[PropertyX] = new PropertyHeaderViewModel(Resources.CodeGenX);
        _headers[PropertyY] = new PropertyHeaderViewModel(Resources.CodeGenY);
        _headers[PropertyZ] = new PropertyHeaderViewModel(Resources.CodeGenZ);
        _headers[PropertyDecimalPlaces] = new PropertyHeaderViewModel(Resources.CodeGenDecimalPlaces);
        
        InitializeCoordinateSystems();
        LoadFromSettings(_settingsService.Current);
        RegisterTrackedProperties();
        UpdateCodeGenerationDependentControls();
        HeaderTracker.UpdateAllHeadersImmediate();
    }

    /// <summary>
    /// Registers all properties for change tracking, organized by logical groups.
    /// </summary>
    private void RegisterTrackedProperties()
    {
        RegisterLineNumberProperties();
        RegisterGeneralCodeGenerationProperties();
        RegisterCoordinateSystemProperties();
        RegisterCoordinateModeProperties();
        RegisterInitialPositionProperties();
        RegisterEndPositionProperties();
        RegisterDecimalPlacesProperty();
    }

    /// <summary>
    /// Registers line number related properties for tracking.
    /// </summary>
    private void RegisterLineNumberProperties()
    {
        RegisterProperty<bool>(PropertyUseLineNumbers, UseLineNumbers, () => Resources.CodeGenUseLineNumbers);
        RegisterProperty<string>(PropertyStartLineNumber, StartLineNumber, () => Resources.CodeGenStartLineNumber);
        RegisterProperty<string>(PropertyLineNumberStep, LineNumberStep, () => Resources.CodeGenLineNumberStep);
    }

    /// <summary>
    /// Registers general code generation properties for tracking.
    /// </summary>
    private void RegisterGeneralCodeGenerationProperties()
    {
        RegisterProperty<bool>(PropertyGenerateComments, GenerateComments, () => Resources.CodeGenGenerateComments);
        RegisterProperty<bool>(PropertyAllowArcs, AllowArcs, () => Resources.CodeGenAllowArcs);
        RegisterProperty<bool>(PropertyFormatCommands, FormatCommands, () => Resources.CodeGenFormatCommands);
    }

    /// <summary>
    /// Registers coordinate system related properties for tracking.
    /// </summary>
    private void RegisterCoordinateSystemProperties()
    {
        RegisterProperty<bool>(PropertySetWorkCoordinateSystem, SetWorkCoordinateSystem, () => Resources.CodeGenSetWorkCoordinateSystem);
        HeaderTracker.Register(
            PropertyCoordinateSystem,
            SelectedCoordinateSystem?.Key ?? Core.Settings.CoordinateSystems.Default,
            () => Resources.CodeGenCoordinateSystemLabel,
            _headers[PropertyCoordinateSystem]);
    }

    /// <summary>
    /// Registers coordinate mode properties for tracking.
    /// </summary>
    private void RegisterCoordinateModeProperties()
    {
        RegisterProperty<bool>(PropertySetAbsoluteCoordinates, SetAbsoluteCoordinates, () => Resources.CodeGenSetAbsoluteCoordinates);
        RegisterProperty<bool>(PropertyAllowRelativeCoordinates, AllowRelativeCoordinates, () => Resources.CodeGenAllowRelativeCoordinates);
        RegisterProperty<bool>(PropertySetZerosAtStart, SetZerosAtStart, () => Resources.CodeGenSetZerosAtStart);
    }

    /// <summary>
    /// Registers initial position (X0, Y0, Z0) properties for tracking.
    /// </summary>
    private void RegisterInitialPositionProperties()
    {
        RegisterProperty<string>(PropertyX0, X0, () => Resources.CodeGenX0);
        RegisterProperty<string>(PropertyY0, Y0, () => Resources.CodeGenY0);
        RegisterProperty<string>(PropertyZ0, Z0, () => Resources.CodeGenZ0);
    }

    /// <summary>
    /// Registers end position properties for tracking.
    /// </summary>
    private void RegisterEndPositionProperties()
    {
        RegisterProperty<bool>(PropertyMoveToPointAtEnd, MoveToPointAtEnd, () => Resources.CodeGenMoveToPointAtEnd);
        RegisterProperty<string>(PropertyX, X, () => Resources.CodeGenX);
        RegisterProperty<string>(PropertyY, Y, () => Resources.CodeGenY);
        RegisterProperty<string>(PropertyZ, Z, () => Resources.CodeGenZ);
    }

    /// <summary>
    /// Registers decimal places property for tracking.
    /// </summary>
    private void RegisterDecimalPlacesProperty()
    {
        RegisterProperty<string>(PropertyDecimalPlaces, DecimalPlaces, () => Resources.CodeGenDecimalPlaces);
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
        UpdateCoordinateSystemDisplayNames();
        HeaderTracker.UpdateAllHeaders();
    }

    /// <summary>
    /// Initializes the coordinate systems collection with available options.
    /// </summary>
    private void InitializeCoordinateSystems()
    {
        _coordinateSystemsHelper.Clear();
        _coordinateSystemsHelper
            .Add(Core.Settings.CoordinateSystems.G54, () => Resources.CodeGenCoordinateSystemG54, (k, d) => new CoordinateSystemOption(k, d))
            .Add(Core.Settings.CoordinateSystems.G55, () => Resources.CodeGenCoordinateSystemG55, (k, d) => new CoordinateSystemOption(k, d))
            .Add(Core.Settings.CoordinateSystems.G56, () => Resources.CodeGenCoordinateSystemG56, (k, d) => new CoordinateSystemOption(k, d))
            .Add(Core.Settings.CoordinateSystems.G57, () => Resources.CodeGenCoordinateSystemG57, (k, d) => new CoordinateSystemOption(k, d))
            .Add(Core.Settings.CoordinateSystems.G58, () => Resources.CodeGenCoordinateSystemG58, (k, d) => new CoordinateSystemOption(k, d))
            .Add(Core.Settings.CoordinateSystems.G59, () => Resources.CodeGenCoordinateSystemG59, (k, d) => new CoordinateSystemOption(k, d));
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

    partial void OnDecimalPlacesChanged(string value)
    {
        HeaderTracker.Update(PropertyDecimalPlaces, value);
    }

    partial void OnSelectedCoordinateSystemChanged(CoordinateSystemOption? value)
    {
        HeaderTracker.Update(PropertyCoordinateSystem, value?.Key ?? Core.Settings.CoordinateSystems.Default);
    }

    /// <summary>
    /// Updates the enabled state of dependent controls based on current property values.
    /// </summary>
    private void UpdateCodeGenerationDependentControls()
    {
        IsLineNumberOptionsEnabled = UseLineNumbers;
        IsCoordinateSystemComboEnabled = SetWorkCoordinateSystem;
        IsSetZerosOptionsEnabled = SetZerosAtStart;
        IsMoveToPointOptionsEnabled = MoveToPointAtEnd;
    }

    public override void LoadFromSettings(AppSettings settings)
    {
        LoadBoolProperty(settings, s => s.UseLineNumbers, CodeGenerationDefaults.UseLineNumbers, PropertyUseLineNumbers, v => UseLineNumbers = v);
        LoadStringProperty(settings, s => s.StartLineNumber, CodeGenerationDefaults.StartLineNumber, PropertyStartLineNumber, v => StartLineNumber = v);
        LoadStringProperty(settings, s => s.LineNumberStep, CodeGenerationDefaults.LineNumberStep, PropertyLineNumberStep, v => LineNumberStep = v);
        LoadBoolProperty(settings, s => s.GenerateComments, CodeGenerationDefaults.GenerateComments, PropertyGenerateComments, v => GenerateComments = v);
        LoadBoolProperty(settings, s => s.AllowArcs, CodeGenerationDefaults.AllowArcs, PropertyAllowArcs, v => AllowArcs = v);
        LoadBoolProperty(settings, s => s.FormatCommands, CodeGenerationDefaults.FormatCommands, PropertyFormatCommands, v => FormatCommands = v);
        LoadBoolProperty(settings, s => s.SetWorkCoordinateSystem, CodeGenerationDefaults.SetWorkCoordinateSystem, PropertySetWorkCoordinateSystem, v => SetWorkCoordinateSystem = v);
        
        var coordinateSystem = LoadStringProperty(settings, s => s.CoordinateSystem, Core.Settings.CoordinateSystems.Default, PropertyCoordinateSystem, _ => { });
        SelectedCoordinateSystem = _coordinateSystemsHelper.FindByKey(coordinateSystem)
                                   ?? _coordinateSystemsHelper.FindByKey(Core.Settings.CoordinateSystems.Default);
        
        LoadBoolProperty(settings, s => s.SetAbsoluteCoordinates, CodeGenerationDefaults.SetAbsoluteCoordinates, PropertySetAbsoluteCoordinates, v => SetAbsoluteCoordinates = v);
        LoadBoolProperty(settings, s => s.AllowRelativeCoordinates, CodeGenerationDefaults.AllowRelativeCoordinates, PropertyAllowRelativeCoordinates, v => AllowRelativeCoordinates = v);
        LoadBoolProperty(settings, s => s.SetZerosAtStart, CodeGenerationDefaults.SetZerosAtStart, PropertySetZerosAtStart, v => SetZerosAtStart = v);
        LoadStringProperty(settings, s => s.X0, CodeGenerationDefaults.Coordinate, PropertyX0, v => X0 = v);
        LoadStringProperty(settings, s => s.Y0, CodeGenerationDefaults.Coordinate, PropertyY0, v => Y0 = v);
        LoadStringProperty(settings, s => s.Z0, CodeGenerationDefaults.Coordinate, PropertyZ0, v => Z0 = v);
        LoadBoolProperty(settings, s => s.MoveToPointAtEnd, CodeGenerationDefaults.MoveToPointAtEnd, PropertyMoveToPointAtEnd, v => MoveToPointAtEnd = v);
        LoadStringProperty(settings, s => s.X, CodeGenerationDefaults.Coordinate, PropertyX, v => X = v);
        LoadStringProperty(settings, s => s.Y, CodeGenerationDefaults.Coordinate, PropertyY, v => Y = v);
        LoadStringProperty(settings, s => s.Z, CodeGenerationDefaults.Coordinate, PropertyZ, v => Z = v);
        
        // Load DecimalPlaces as int, convert to string for UI
        var decimalPlaces = settings.DecimalPlaces ?? CodeGenerationDefaults.DecimalPlaces;
        DecimalPlaces = decimalPlaces.ToString();
        HeaderTracker.Update<string>(PropertyDecimalPlaces, DecimalPlaces);
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
        settings.CoordinateSystem = SelectedCoordinateSystem?.Key ?? Core.Settings.CoordinateSystems.Default;
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
        
        // Save DecimalPlaces as int, parse from string
        if (int.TryParse((string?)DecimalPlaces, out var decimalPlaces))
        {
            settings.DecimalPlaces = decimalPlaces;
        }
        else
        {
            settings.DecimalPlaces = CodeGenerationDefaults.DecimalPlaces;
        }
    }

    public override void ResetToOriginal()
    {
        ResetBoolProperty(PropertyUseLineNumbers, CodeGenerationDefaults.UseLineNumbers, v => UseLineNumbers = v);
        ResetStringProperty(PropertyStartLineNumber, CodeGenerationDefaults.StartLineNumber, v => StartLineNumber = v);
        ResetStringProperty(PropertyLineNumberStep, CodeGenerationDefaults.LineNumberStep, v => LineNumberStep = v);
        ResetBoolProperty(PropertyGenerateComments, CodeGenerationDefaults.GenerateComments, v => GenerateComments = v);
        ResetBoolProperty(PropertyAllowArcs, CodeGenerationDefaults.AllowArcs, v => AllowArcs = v);
        ResetBoolProperty(PropertyFormatCommands, CodeGenerationDefaults.FormatCommands, v => FormatCommands = v);
        ResetBoolProperty(PropertySetWorkCoordinateSystem, CodeGenerationDefaults.SetWorkCoordinateSystem, v => SetWorkCoordinateSystem = v);
        
        var originalCoordinateSystem = ResetStringProperty(PropertyCoordinateSystem, Core.Settings.CoordinateSystems.Default, _ => { });
        SelectedCoordinateSystem = _coordinateSystemsHelper.FindByKey(originalCoordinateSystem)
                                   ?? _coordinateSystemsHelper.FindByKey(Core.Settings.CoordinateSystems.Default);
        
        ResetBoolProperty(PropertySetAbsoluteCoordinates, CodeGenerationDefaults.SetAbsoluteCoordinates, v => SetAbsoluteCoordinates = v);
        ResetBoolProperty(PropertyAllowRelativeCoordinates, CodeGenerationDefaults.AllowRelativeCoordinates, v => AllowRelativeCoordinates = v);
        ResetBoolProperty(PropertySetZerosAtStart, CodeGenerationDefaults.SetZerosAtStart, v => SetZerosAtStart = v);
        ResetStringProperty(PropertyX0, CodeGenerationDefaults.Coordinate, v => X0 = v);
        ResetStringProperty(PropertyY0, CodeGenerationDefaults.Coordinate, v => Y0 = v);
        ResetStringProperty(PropertyZ0, CodeGenerationDefaults.Coordinate, v => Z0 = v);
        ResetBoolProperty(PropertyMoveToPointAtEnd, CodeGenerationDefaults.MoveToPointAtEnd, v => MoveToPointAtEnd = v);
        ResetStringProperty(PropertyX, CodeGenerationDefaults.Coordinate, v => X = v);
        ResetStringProperty(PropertyY, CodeGenerationDefaults.Coordinate, v => Y = v);
        ResetStringProperty(PropertyZ, CodeGenerationDefaults.Coordinate, v => Z = v);
        ResetStringProperty(PropertyDecimalPlaces, CodeGenerationDefaults.DecimalPlaces.ToString(), v => DecimalPlaces = v);
        
        UpdateCodeGenerationDependentControls();
        HeaderTracker.UpdateAllHeadersImmediate();
    }
}
