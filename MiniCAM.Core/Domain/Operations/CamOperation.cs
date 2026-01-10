using System;

namespace MiniCAM.Core.Domain.Operations;

/// <summary>
/// Base class for all CAM operations.
/// </summary>
public abstract class CamOperation
{
    /// <summary>
    /// Gets the unique identifier of the operation.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the name of the operation.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the operation is enabled.
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the order of execution (lower values execute first).
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Generates the tool path for this operation.
    /// </summary>
    /// <param name="parameters">Operation parameters (tool diameter, feed rates, etc.).</param>
    /// <returns>The generated tool path.</returns>
    public abstract ToolPath.ToolPath GenerateToolPath(OperationParameters parameters);

    /// <summary>
    /// Validates the operation parameters.
    /// </summary>
    /// <param name="parameters">Operation parameters to validate.</param>
    /// <returns>Validation result indicating success or failure with error messages.</returns>
    public abstract ValidationResult Validate(OperationParameters parameters);
}
