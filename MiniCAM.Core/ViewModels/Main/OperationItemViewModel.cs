using System;
using MiniCAM.Core.Domain.Operations;
using MiniCAM.Core.ViewModels.Base;

namespace MiniCAM.Core.ViewModels.Main;

/// <summary>
/// ViewModel for a CAM operation. Wraps domain model for UI binding.
/// </summary>
public partial class OperationItemViewModel : ViewModelBase
{
    private readonly CamOperation _operation;

    public OperationItemViewModel(CamOperation operation)
    {
        _operation = operation ?? throw new ArgumentNullException(nameof(operation));
    }

    /// <summary>
    /// Gets the underlying domain operation.
    /// </summary>
    public CamOperation Operation => _operation;

    /// <summary>
    /// Gets the unique identifier of the operation.
    /// </summary>
    public Guid Id => _operation.Id;

    /// <summary>
    /// Gets or sets the name of the operation.
    /// </summary>
    public string Name
    {
        get => _operation.Name;
        set
        {
            if (_operation.Name != value)
            {
                _operation.Name = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the operation is enabled.
    /// </summary>
    public bool IsEnabled
    {
        get => _operation.IsEnabled;
        set
        {
            if (_operation.IsEnabled != value)
            {
                _operation.IsEnabled = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the order of execution.
    /// </summary>
    public int Order
    {
        get => _operation.Order;
        set
        {
            if (_operation.Order != value)
            {
                _operation.Order = value;
                OnPropertyChanged();
            }
        }
    }
}
