using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MiniCAM.Core.Localization;

namespace MiniCAM.Core.ViewModels.Main;

/// <summary>
/// ViewModel for 2D primitives tab.
/// </summary>
public partial class Primitives2DViewModel : LocalizedViewModelBase
{
    public ObservableCollection<Primitive2DItem> Primitives { get; } = new();

    [ObservableProperty]
    private Primitive2DItem? _selectedPrimitive;

    [ObservableProperty]
    private string _deleteButtonText = Resources.ButtonDelete;

    protected override void UpdateLocalizedStrings()
    {
        DeleteButtonText = Resources.ButtonDelete;
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

    partial void OnSelectedPrimitiveChanged(Primitive2DItem? oldValue, Primitive2DItem? newValue)
    {
        DeletePrimitiveCommand.NotifyCanExecuteChanged();
    }
}
