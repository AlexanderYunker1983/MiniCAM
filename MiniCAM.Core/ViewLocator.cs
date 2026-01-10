using System;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using MiniCAM.Core.ViewModels.Base;

namespace MiniCAM.Core;

/// <summary>
/// Given a view model, returns the corresponding view if possible.
/// </summary>
[RequiresUnreferencedCode(
    "Default implementation of ViewLocator involves reflection which may be trimmed away.",
    Url = "https://docs.avaloniaui.net/docs/concepts/view-locator")]
public class ViewLocator : IDataTemplate
{
    /// <summary>
    /// Builds a view control for the given view model.
    /// </summary>
    /// <param name="param">The view model instance.</param>
    /// <returns>The corresponding view control, or null if param is null.</returns>
    public Control? Build(object? param)
    {
        if (param is null)
            return null;

        var name = param.GetType().FullName!.Replace("ViewModel", "View", StringComparison.Ordinal);
        var type = Type.GetType(name);

        if (type != null)
        {
            return (Control)Activator.CreateInstance(type)!;
        }

        return new TextBlock { Text = "Not Found: " + name };
    }

    /// <summary>
    /// Determines if this locator can build a view for the given data.
    /// </summary>
    /// <param name="data">The data object (typically a view model).</param>
    /// <returns>True if data is a ViewModelBase, false otherwise.</returns>
    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}