using System;
using System.Collections.Generic;
using System.Linq;

namespace MiniCAM.Core.Domain.Operations;

/// <summary>
/// Represents the result of operation validation.
/// </summary>
public class ValidationResult
{
    /// <summary>
    /// Gets a value indicating whether the validation was successful.
    /// </summary>
    public bool IsValid { get; }

    /// <summary>
    /// Gets the list of validation errors (empty if valid).
    /// </summary>
    public IReadOnlyList<string> Errors { get; }

    private ValidationResult(bool isValid, IReadOnlyList<string> errors)
    {
        IsValid = isValid;
        Errors = errors;
    }

    /// <summary>
    /// Creates a successful validation result.
    /// </summary>
    public static ValidationResult Success()
    {
        return new ValidationResult(true, Array.Empty<string>());
    }

    /// <summary>
    /// Creates a failed validation result with errors.
    /// </summary>
    public static ValidationResult Failure(params string[] errors)
    {
        return new ValidationResult(false, errors);
    }

    /// <summary>
    /// Creates a failed validation result with errors.
    /// </summary>
    public static ValidationResult Failure(IEnumerable<string> errors)
    {
        return new ValidationResult(false, errors.ToList().AsReadOnly());
    }
}
