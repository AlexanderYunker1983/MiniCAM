using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MiniCAM.Core.ViewModels.Main;

/// <summary>
/// Represents a 2D primitive item with cascading child items.
/// </summary>
public partial class Primitive2DItem : ObservableObject
{
    [ObservableProperty]
    private string _name;

    [ObservableProperty]
    private bool _isExpanded = true;

    public ObservableCollection<Primitive2DChildItem> Children { get; } = new();

    public Primitive2DItem(string name)
    {
        _name = name;
    }
}

/// <summary>
/// Represents a child item of a 2D primitive (text type).
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

/// <summary>
/// Represents a 2D point primitive.
/// </summary>
public partial class Point2DPrimitive : Primitive2DItem
{
    [ObservableProperty]
    private double _x;

    [ObservableProperty]
    private double _y;

    public Point2DPrimitive(double x, double y, string name) : base(name)
    {
        _x = x;
        _y = y;
    }
}
