using System;
using System.Linq.Expressions;
using System.Reflection;

namespace MiniCAM.Core.Settings;

/// <summary>
/// Extension methods for AppSettings to simplify getting values with defaults.
/// </summary>
public static class AppSettingsExtensions
{
    /// <summary>
    /// Gets a value from AppSettings or returns the default value if null.
    /// </summary>
    /// <typeparam name="T">The value type (must be a struct).</typeparam>
    /// <param name="settings">The AppSettings instance.</param>
    /// <param name="propertySelector">Expression selecting the property.</param>
    /// <param name="defaultValue">The default value to return if the property is null.</param>
    /// <returns>The property value or the default value.</returns>
    public static T GetValueOrDefault<T>(this AppSettings settings, 
        Expression<Func<AppSettings, T?>> propertySelector, 
        T defaultValue) 
        where T : struct
    {
        if (settings == null)
            throw new ArgumentNullException(nameof(settings));
        if (propertySelector == null)
            throw new ArgumentNullException(nameof(propertySelector));

        var property = GetPropertyInfo(propertySelector);
        var value = (T?)property.GetValue(settings);
        return value ?? defaultValue;
    }

    /// <summary>
    /// Gets a string value from AppSettings or returns the default value if null or empty.
    /// </summary>
    /// <param name="settings">The AppSettings instance.</param>
    /// <param name="propertySelector">Expression selecting the property.</param>
    /// <param name="defaultValue">The default value to return if the property is null or empty.</param>
    /// <returns>The property value or the default value.</returns>
    public static string GetStringOrDefault(this AppSettings settings,
        Expression<Func<AppSettings, string?>> propertySelector,
        string defaultValue)
    {
        if (settings == null)
            throw new ArgumentNullException(nameof(settings));
        if (propertySelector == null)
            throw new ArgumentNullException(nameof(propertySelector));

        var property = GetPropertyInfo(propertySelector);
        var value = (string?)property.GetValue(settings);
        return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
    }

    private static PropertyInfo GetPropertyInfo<T>(Expression<Func<AppSettings, T>> expression)
    {
        if (expression.Body is not MemberExpression memberExpression)
        {
            throw new ArgumentException("Expression must be a property access.", nameof(expression));
        }

        if (memberExpression.Member is not PropertyInfo propertyInfo)
        {
            throw new ArgumentException("Expression must access a property.", nameof(expression));
        }

        return propertyInfo;
    }
}
