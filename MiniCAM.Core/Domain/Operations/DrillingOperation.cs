using System.Collections.Generic;
using MiniCAM.Core.Domain.Geometry;
using MiniCAM.Core.Domain.ToolPath;

namespace MiniCAM.Core.Domain.Operations;

/// <summary>
/// Represents a drilling operation that drills holes at specified points.
/// </summary>
public class DrillingOperation : CamOperation
{
    /// <summary>
    /// Gets or sets the list of drill points (X, Y coordinates).
    /// </summary>
    public List<Point2D> DrillPoints { get; set; } = new();

    /// <summary>
    /// Gets or sets the drilling depth in millimeters (negative value, e.g., -10 means 10mm down).
    /// </summary>
    public double Depth { get; set; }

    /// <summary>
    /// Gets or sets the retract height in millimeters (height to retract to after drilling).
    /// </summary>
    public double RetractHeight { get; set; } = 1.0;

    /// <summary>
    /// Gets or sets the feed rate for drilling in millimeters per minute.
    /// </summary>
    public double FeedRate { get; set; } = 100.0;

    /// <summary>
    /// Gets or sets the rapid height in millimeters (safe height for rapid movements).
    /// </summary>
    public double RapidHeight { get; set; } = 10.0;

    /// <summary>
    /// Gets or sets the dwell time at the bottom of the hole in seconds (optional).
    /// </summary>
    public double? DwellTime { get; set; }

    public override ToolPath.ToolPath GenerateToolPath(OperationParameters parameters)
    {
        var toolPath = new ToolPath.ToolPath();
        
        // Set initial position to first drill point at rapid height
        if (DrillPoints.Count > 0)
        {
            var firstPoint = DrillPoints[0];
            toolPath.SetInitialPosition(new Point3D(firstPoint.X, firstPoint.Y, RapidHeight));
        }

        foreach (var point in DrillPoints)
        {
            // Rapid move to position above the hole
            toolPath.AddMove(new MoveCommand(
                MoveType.Rapid,
                point.X,
                point.Y,
                RapidHeight));

            // Rapid move down to retract height
            toolPath.AddMove(new MoveCommand(
                MoveType.Rapid,
                point.X,
                point.Y,
                RetractHeight));

            // Linear move to depth (drilling)
            toolPath.AddMove(new MoveCommand(
                MoveType.Linear,
                point.X,
                point.Y,
                Depth,
                FeedRate));

            // Dwell at bottom if specified
            if (DwellTime.HasValue && DwellTime.Value > 0)
            {
                // Note: Dwell is typically implemented as G4 command, not a move
                // For now, we'll add a rapid move with zero distance to represent dwell
                // This will need to be handled specially in G-code generation
            }

            // Retract to retract height
            toolPath.AddMove(new MoveCommand(
                MoveType.Rapid,
                point.X,
                point.Y,
                RetractHeight));

            // Retract to rapid height
            toolPath.AddMove(new MoveCommand(
                MoveType.Rapid,
                point.X,
                point.Y,
                RapidHeight));
        }

        return toolPath;
    }

    public override ValidationResult Validate(OperationParameters parameters)
    {
        var errors = new List<string>();

        if (DrillPoints.Count == 0)
        {
            errors.Add("Drilling operation must have at least one drill point.");
        }

        if (Depth >= 0)
        {
            errors.Add("Drilling depth must be negative (below the surface).");
        }

        if (RetractHeight <= Depth)
        {
            errors.Add("Retract height must be greater than drilling depth.");
        }

        if (RapidHeight <= RetractHeight)
        {
            errors.Add("Rapid height must be greater than retract height.");
        }

        if (FeedRate <= 0)
        {
            errors.Add("Feed rate must be greater than zero.");
        }

        if (DwellTime.HasValue && DwellTime.Value < 0)
        {
            errors.Add("Dwell time cannot be negative.");
        }

        return errors.Count == 0
            ? ValidationResult.Success()
            : ValidationResult.Failure(errors);
    }
}
