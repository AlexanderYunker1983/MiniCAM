using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MiniCAM.Core.Localization;
using MiniCAM.Core.ViewModels.Base;

namespace MiniCAM.Core.ViewModels.Main;

/// <summary>
/// ViewModel for 2D primitives tab.
/// </summary>
public partial class Primitives2DViewModel : LocalizedViewModelBase
{
    public ObservableCollection<Primitive2DItemViewModel> Primitives { get; } = new();

    [ObservableProperty]
    private Primitive2DItemViewModel? _selectedPrimitive;

    [ObservableProperty]
    private string _deleteButtonText = Resources.ButtonDelete;

    [ObservableProperty]
    private bool _isSelectionEnabled = true;

    protected override void UpdateLocalizedStrings()
    {
        DeleteButtonText = Resources.ButtonDelete;
        
        // Update primitive names and children when language changes
        foreach (var primitive in Primitives)
        {
            if (primitive is Point2DViewModel pointViewModel)
            {
                primitive.Name = Resources.PrimitivePoint;
                // Update localized property names in children
                pointViewModel.UpdateLocalizedChildren();
            }
            else if (primitive is Line2DViewModel lineViewModel)
            {
                primitive.Name = Resources.PrimitiveLine;
                // Update localized property names in children
                lineViewModel.UpdateLocalizedChildren();
            }
            else if (primitive is Ellipse2DViewModel ellipseViewModel)
            {
                primitive.Name = Resources.PrimitiveEllipse;
                // Update localized property names in children
                ellipseViewModel.UpdateLocalizedChildren();
            }
        }
    }

    public Primitives2DViewModel()
    {
    }

    private bool CanDeletePrimitive()
    {
        return SelectedPrimitive != null;
    }

    [RelayCommand(CanExecute = nameof(CanDeletePrimitive))]
    private void DeletePrimitive()
    {
        if (SelectedPrimitive == null) return;
        
        Primitives.Remove(SelectedPrimitive);
        SelectedPrimitive = null;
    }

    partial void OnSelectedPrimitiveChanged(Primitive2DItemViewModel? oldValue, Primitive2DItemViewModel? newValue)
    {
        DeletePrimitiveCommand.NotifyCanExecuteChanged();
    }

    partial void OnIsSelectionEnabledChanged(bool value)
    {
        // Clear selection when selection is disabled
        if (!value)
        {
            SelectedPrimitive = null;
        }
    }
}
