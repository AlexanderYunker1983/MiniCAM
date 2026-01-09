using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MiniCAM.Core.Localization;
using MiniCAM.Core.Settings;
using MiniCAM.Core.Settings.Models;
using MiniCAM.Core.ViewModels;

namespace MiniCAM.Core.CodeGeneration;

/// <summary>
/// Generates and optimizes G-code from operations list.
/// </summary>
public class GCodeGenerator
{
    private readonly CodeGenerationSettings _codeGenSettings;
    private readonly SpindleSettings _spindleSettings;
    private readonly CoolantSettings _coolantSettings;
    private int _currentLineNumber;
    private int _decimalPlaces;
    
    // Current tool position tracking
    private double? _currentX;
    private double? _currentY;
    private double? _currentZ;

    /// <summary>
    /// Initializes a new instance of the GCodeGenerator class.
    /// </summary>
    /// <param name="codeGenSettings">Code generation settings.</param>
    /// <param name="spindleSettings">Spindle control settings.</param>
    /// <param name="coolantSettings">Coolant control settings.</param>
    public GCodeGenerator(CodeGenerationSettings codeGenSettings, SpindleSettings spindleSettings, CoolantSettings coolantSettings)
    {
        _codeGenSettings = codeGenSettings ?? throw new ArgumentNullException(nameof(codeGenSettings));
        _spindleSettings = spindleSettings ?? throw new ArgumentNullException(nameof(spindleSettings));
        _coolantSettings = coolantSettings ?? throw new ArgumentNullException(nameof(coolantSettings));
    }

    /// <summary>
    /// Generates G-code from the list of operations.
    /// </summary>
    /// <param name="operations">List of operations to generate G-code for.</param>
    /// <returns>List of G-code lines.</returns>
    public List<string> Generate(IEnumerable<OperationItem> operations)
    {
        var result = new List<string>();
        _currentLineNumber = GetStartLineNumber();
        _decimalPlaces = GetDecimalPlaces();
        
        // Initialize current position (after G92 if set)
        ResetCurrentPosition();

        // Filter enabled operations
        var enabledOperations = operations.Where(op => op.IsEnabled).ToList();

        // Program header - ALWAYS without line numbers
        if (_codeGenSettings.GenerateComments == true)
        {
            AddProgramHeader(result);
            AddFileCreatedComment(result);
        }
        else
        {
            // Even without comments, add program number (O0001) - without line number
            result.Add("O0001");
        }

        // Now start numbering from here
        // Generate program initialization commands
        GenerateProgramStart(result);

        // Generate G-code for each operation
        foreach (var operation in enabledOperations)
        {
            GenerateOperationCode(result, operation);
        }

        // Generate program end commands
        GenerateProgramEnd(result);

        // Remove all empty lines
        result.RemoveAll(string.IsNullOrWhiteSpace);

        return result;
    }

    private void ResetCurrentPosition()
    {
        // Initialize current position from G92 settings if set
        if (_codeGenSettings.SetZerosAtStart == true)
        {
            _currentX = TryParseCoordinate(_codeGenSettings.X0);
            _currentY = TryParseCoordinate(_codeGenSettings.Y0);
            _currentZ = TryParseCoordinate(_codeGenSettings.Z0);
        }
    }

    private int GetDecimalPlaces()
    {
        return _codeGenSettings.DecimalPlaces ?? CodeGenerationDefaults.DecimalPlaces;
    }

    private double? TryParseCoordinate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;
        
        if (double.TryParse(value, System.Globalization.NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
            return result;
        
        return null;
    }

    /// <summary>
    /// Formats a coordinate value with the specified number of decimal places.
    /// Always includes decimal point and all decimal places, even if they are zeros.
    /// </summary>
    private string FormatCoordinate(double value)
    {
        return value.ToString($"F{_decimalPlaces}", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Compares two coordinate values considering only the first n decimal places.
    /// For example, if n = 3, then 30.0012 and 30.0019 are considered equal.
    /// </summary>
    private bool AreCoordinatesEqual(double? coord1, double? coord2)
    {
        if (!coord1.HasValue && !coord2.HasValue)
            return true;
        
        if (!coord1.HasValue || !coord2.HasValue)
            return false;

        // Round both values to the specified decimal places for comparison
        var multiplier = Math.Pow(10, _decimalPlaces);
        var rounded1 = Math.Round(coord1.Value * multiplier) / multiplier;
        var rounded2 = Math.Round(coord2.Value * multiplier) / multiplier;
        
        return Math.Abs(rounded1 - rounded2) < 0.0001;
    }

    private void AddProgramHeader(List<string> result)
    {
        // O0001 (Program) - O always in English, Program always in English
        // ALWAYS without line number
        result.Add("O0001 (Program)");
    }

    private void AddFileCreatedComment(List<string> result)
    {
        // Format: ;(файл создан: 23:10:42 09.01.2026)
        // ALWAYS without line number
        var now = DateTime.Now;
        var dateStr = now.ToString("HH:mm:ss", CultureInfo.InvariantCulture) + " " + now.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
        var comment = ";(" + Resources.GCodeFileCreated + " " + dateStr + ")";
        result.Add(comment);
    }

    private void GenerateProgramStart(List<string> result)
    {
        // 1. Work coordinate system (G54-G59)
        if (_codeGenSettings.SetWorkCoordinateSystem == true && !string.IsNullOrWhiteSpace(_codeGenSettings.CoordinateSystem))
        {
            AddLine(result, _codeGenSettings.CoordinateSystem);
        }

        // 2. Absolute coordinate system (G90)
        if (_codeGenSettings.SetAbsoluteCoordinates == true)
        {
            AddLine(result, FormatCommand("G90"));
        }

        // 3. Set zeros via G92
        if (_codeGenSettings.SetZerosAtStart == true)
        {
            var g92Line = FormatCommand("G92");
            var coordinates = new List<string>();

            if (!string.IsNullOrWhiteSpace(_codeGenSettings.X0))
            {
                var coordValue = TryParseCoordinate(_codeGenSettings.X0);
                if (coordValue.HasValue)
                {
                    coordinates.Add($"X{FormatCoordinate(coordValue.Value)}");
                    _currentX = coordValue;
                }
            }
            if (!string.IsNullOrWhiteSpace(_codeGenSettings.Y0))
            {
                var coordValue = TryParseCoordinate(_codeGenSettings.Y0);
                if (coordValue.HasValue)
                {
                    coordinates.Add($"Y{FormatCoordinate(coordValue.Value)}");
                    _currentY = coordValue;
                }
            }
            if (!string.IsNullOrWhiteSpace(_codeGenSettings.Z0))
            {
                var coordValue = TryParseCoordinate(_codeGenSettings.Z0);
                if (coordValue.HasValue)
                {
                    coordinates.Add($"Z{FormatCoordinate(coordValue.Value)}");
                    _currentZ = coordValue;
                }
            }

            if (coordinates.Count > 0)
            {
                g92Line += " " + string.Join(" ", coordinates);
                AddLine(result, g92Line);
            }
        }

        // 4. Spindle speed and enable
        if (_spindleSettings.AddSpindleCode == true && _spindleSettings.EnableSpindleBeforeOperations == true && !string.IsNullOrWhiteSpace(_spindleSettings.SpindleEnableCommand))
        {
            // Build spindle command with speed if configured
            // M3/M4 must include S parameter if speed is set
            var spindleCommand = _spindleSettings.SpindleEnableCommand;
            
            // Add speed parameter if enabled and value is provided
            if (_spindleSettings.SetSpindleSpeed == true && !string.IsNullOrWhiteSpace(_spindleSettings.SpindleSpeed))
            {
                spindleCommand += $" S{_spindleSettings.SpindleSpeed}";
            }
            
            AddLine(result, spindleCommand);

            // Add delay after enable if configured
            if (_spindleSettings.AddSpindleDelayAfterEnable == true && 
                !string.IsNullOrWhiteSpace(_spindleSettings.SpindleDelayParameter) &&
                !string.IsNullOrWhiteSpace(_spindleSettings.SpindleDelayValue))
            {
                var delayLine = FormatCommand("G4");
                var delayParam = _spindleSettings.SpindleDelayParameter == "Pxx." ? "P" : _spindleSettings.SpindleDelayParameter;
                delayLine += $" {delayParam}{_spindleSettings.SpindleDelayValue}";
                AddLine(result, delayLine);
            }
        }

        // 5. Enable coolant at start
        if (_coolantSettings.AddCoolantCode == true && _coolantSettings.EnableCoolantAtStart == true)
        {
            AddLine(result, "M8");
        }
    }

    private void GenerateOperationCode(List<string> result, OperationItem operation)
    {
        // Add comment for operation if comments are enabled
        if (_codeGenSettings.GenerateComments == true)
        {
            AddLine(result, $"; {operation.Name}");
        }

        // Placeholder implementation - in real scenario, this would generate actual G-code
        // based on operation type and parameters
        AddLine(result, FormatCommand("G90")); // Absolute positioning
        AddMoveCommand(result, FormatCommand("G0"), "0", "0", "0"); // Rapid move to origin
    }

    private void GenerateProgramEnd(List<string> result)
    {
        // 1. Move to point at end (G0) - BEFORE disabling spindle and coolant
        if (_codeGenSettings.MoveToPointAtEnd == true)
        {
            AddMoveCommand(result, FormatCommand("G0"), _codeGenSettings.X, _codeGenSettings.Y, _codeGenSettings.Z);
        }

        // 2. Disable spindle after operations
        if (_spindleSettings.AddSpindleCode == true && _spindleSettings.DisableSpindleAfterOperations == true)
        {
            AddLine(result, "M5");
        }

        // 3. Disable coolant at end
        if (_coolantSettings.AddCoolantCode == true && _coolantSettings.DisableCoolantAtEnd == true)
        {
            AddLine(result, "M9");
        }

        // 4. Program end (M30) - with or without line number depending on settings
        AddLine(result, "M30");

        // 5. Program end marker (%) - ALWAYS without line number
        result.Add("%");
    }

    /// <summary>
    /// Processes a move command (G0, G1, G2, G3) with coordinates.
    /// Removes coordinates that are equal to current position.
    /// Skips the command completely if no movement is needed.
    /// Updates current position after successful move.
    /// </summary>
    /// <param name="result">List to add the command to.</param>
    /// <param name="command">The move command (G0, G1, G2, G3, etc.).</param>
    /// <param name="x">Target X coordinate (can be null or empty).</param>
    /// <param name="y">Target Y coordinate (can be null or empty).</param>
    /// <param name="z">Target Z coordinate (can be null or empty).</param>
    /// <returns>True if command was added, false if it was skipped (no movement needed).</returns>
    private bool AddMoveCommand(List<string> result, string command, string? x, string? y, string? z)
    {
        var targetX = TryParseCoordinate(x);
        var targetY = TryParseCoordinate(y);
        var targetZ = TryParseCoordinate(z);

        var coordinates = new List<string>();
        bool hasMovement = false;

        // Check X coordinate
        if (targetX.HasValue)
        {
            if (!AreCoordinatesEqual(_currentX, targetX))
            {
                // Format coordinate with specified decimal places
                coordinates.Add($"X{FormatCoordinate(targetX.Value)}");
                hasMovement = true;
                _currentX = targetX;
            }
        }

        // Check Y coordinate
        if (targetY.HasValue)
        {
            if (!AreCoordinatesEqual(_currentY, targetY))
            {
                coordinates.Add($"Y{FormatCoordinate(targetY.Value)}");
                hasMovement = true;
                _currentY = targetY;
            }
        }

        // Check Z coordinate
        if (targetZ.HasValue)
        {
            if (!AreCoordinatesEqual(_currentZ, targetZ))
            {
                coordinates.Add($"Z{FormatCoordinate(targetZ.Value)}");
                hasMovement = true;
                _currentZ = targetZ;
            }
        }

        // If no movement is needed, skip the command completely (don't increment line number)
        if (!hasMovement)
        {
            return false;
        }

        // Build the command line
        var moveLine = command;
        if (coordinates.Count > 0)
        {
            moveLine += " " + string.Join(" ", coordinates);
        }

        AddLine(result, moveLine);
        return true;
    }

    private string FormatCommand(string command)
    {
        // If FormatCommands is enabled, format G0/G1 as G00/G01, etc.
        if (_codeGenSettings.FormatCommands == true)
        {
            if (command == "G0")
                return "G00";
            if (command == "G1")
                return "G01";
            if (command == "G2")
                return "G02";
            if (command == "G3")
                return "G03";
            if (command == "G4")
                return "G04";
        }

        return command;
    }

    private void AddLine(List<string> result, string line)
    {
        if (string.IsNullOrEmpty(line))
        {
            AddEmptyLine(result);
            return;
        }

        if (_codeGenSettings.UseLineNumbers == true)
        {
            var numberedLine = $"N{_currentLineNumber:D4} {line}";
            result.Add(numberedLine);
            _currentLineNumber += GetLineNumberStep();
        }
        else
        {
            result.Add(line);
        }
    }

    private void AddEmptyLine(List<string> result)
    {
        // Empty lines are always added without line numbers
        result.Add("");
    }

    private int GetStartLineNumber()
    {
        if (int.TryParse(_codeGenSettings.StartLineNumber, out var start))
        {
            return start;
        }
        return 10; // Default
    }

    private int GetLineNumberStep()
    {
        if (int.TryParse(_codeGenSettings.LineNumberStep, out var step))
        {
            return step;
        }
        return 10; // Default
    }
}
