using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MiniCAM.Core.ViewModels.Common;

/// <summary>
/// Helper class for managing collections of Option items with automatic display name updates.
/// </summary>
/// <typeparam name="TOption">The type of option, must inherit from OptionBase.</typeparam>
public class OptionCollectionHelper<TOption> where TOption : OptionBase
{
    private readonly ObservableCollection<TOption> _collection;
    private readonly Dictionary<string, Func<string>> _displayNameProviders = new();

    /// <summary>
    /// Initializes a new instance of the OptionCollectionHelper class.
    /// </summary>
    /// <param name="collection">The observable collection to manage.</param>
    public OptionCollectionHelper(ObservableCollection<TOption> collection)
    {
        _collection = collection ?? throw new ArgumentNullException(nameof(collection));
    }

    /// <summary>
    /// Adds an option to the collection with a display name provider.
    /// </summary>
    /// <param name="key">The key for the option.</param>
    /// <param name="displayNameProvider">Function that provides the display name (can be localized).</param>
    /// <param name="factory">Factory function to create the option instance.</param>
    /// <returns>This instance for method chaining.</returns>
    public OptionCollectionHelper<TOption> Add(string key, Func<string> displayNameProvider, Func<string, string, TOption> factory)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("Key cannot be null or empty.", nameof(key));
        if (displayNameProvider == null)
            throw new ArgumentNullException(nameof(displayNameProvider));
        if (factory == null)
            throw new ArgumentNullException(nameof(factory));

        _displayNameProviders[key] = displayNameProvider;
        var option = factory(key, displayNameProvider());
        _collection.Add(option);
        return this;
    }

    /// <summary>
    /// Updates all display names in the collection using the registered providers.
    /// </summary>
    public void UpdateDisplayNames()
    {
        foreach (var option in _collection)
        {
            if (_displayNameProviders.TryGetValue(option.Key, out var provider))
            {
                option.DisplayName = provider();
            }
        }
    }

    /// <summary>
    /// Finds an option by its key.
    /// </summary>
    /// <param name="key">The key to search for.</param>
    /// <returns>The option with the matching key, or null if not found.</returns>
    public TOption? FindByKey(string key)
    {
        return _collection.FirstOrDefault(x => x.Key == key);
    }

    /// <summary>
    /// Clears the collection and all registered display name providers.
    /// </summary>
    public void Clear()
    {
        _collection.Clear();
        _displayNameProviders.Clear();
    }
}
