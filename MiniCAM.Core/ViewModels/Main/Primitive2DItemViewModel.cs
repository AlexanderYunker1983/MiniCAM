using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using MiniCAM.Core.Domain.Primitives;
using MiniCAM.Core.ViewModels.Base;

namespace MiniCAM.Core.ViewModels.Main;

/// <summary>
/// Base ViewModel for 2D primitives. Wraps domain model for UI binding.
/// </summary>
public abstract partial class Primitive2DItemViewModel : ViewModelBase
{
    private readonly Primitive2D _primitive;

    protected Primitive2DItemViewModel(Primitive2D primitive)
    {
        _primitive = primitive ?? throw new ArgumentNullException(nameof(primitive));
    }

    /// <summary>
    /// Gets the underlying domain primitive.
    /// </summary>
    public Primitive2D Primitive => _primitive;

    /// <summary>
    /// Gets or sets the name of the primitive.
    /// </summary>
    public string Name
    {
        get => _primitive.Name;
        set
        {
            if (_primitive.Name != value)
            {
                _primitive.Name = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets the unique identifier of the primitive.
    /// </summary>
    public Guid Id => _primitive.Id;

    /// <summary>
    /// Gets or sets whether the item is expanded in the tree view.
    /// </summary>
    [ObservableProperty]
    private bool _isExpanded = true;

    /// <summary>
    /// Gets the collection of child items for tree view display.
    /// </summary>
    public ObservableCollection<Primitive2DChildItem> Children { get; } = new();
}

/// <summary>
/// Represents a child item of a 2D primitive (text type) for tree view display.
/// </summary>
public partial class Primitive2DChildItem : ObservableObject
{
    [ObservableProperty]
    private string _text;

    public Primitive2DChildItem(string text)
    {
        _text = text;
    }
}
