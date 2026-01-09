using System;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MiniCAM.Core.Localization;
using MiniCAM.Core.Settings;

namespace MiniCAM.Core.ViewModels;

/// <summary>
/// View model for the Application Settings window.
/// Manages application-level settings such as language and theme.
/// </summary>
public partial class ApplicationSettingsViewModel : SettingsTabViewModelBase
{
    private const string PropertyLanguage = nameof(SelectedLanguage);
    private const string PropertyTheme = nameof(SelectedTheme);

    public ObservableCollection<LanguageOption> Languages { get; } = new();
    public ObservableCollection<ThemeOption> Themes { get; } = new();

    [ObservableProperty]
    private LanguageOption? _selectedLanguage;

    [ObservableProperty]
    private ThemeOption? _selectedTheme;

    [ObservableProperty]
    private string _windowTitle = Resources.ApplicationSettingsTitle;

    [ObservableProperty]
    private string _languageLabel = Resources.LanguageLabel;

    [ObservableProperty]
    private string _themeLabel = Resources.ThemeLabel;

    [ObservableProperty]
    private string _languageHeaderText = Resources.LanguageLabel;

    [ObservableProperty]
    private string _themeHeaderText = Resources.ThemeLabel;

    [ObservableProperty]
    private FontStyle _languageHeaderFontStyle = FontStyle.Normal;

    [ObservableProperty]
    private FontStyle _themeHeaderFontStyle = FontStyle.Normal;

    [ObservableProperty]
    private string _applyButtonText = Resources.ButtonApply;

    [ObservableProperty]
    private string _resetButtonText = Resources.ButtonReset;

    public ApplicationSettingsViewModel()
    {
        BuildLanguages();
        BuildThemes();
        LoadFromSettings();
        RegisterTrackedProperties();
        HeaderTracker.UpdateAllHeadersImmediate();
    }

    protected override void OnCultureChanged(object? sender, CultureChangedEventArgs e)
    {
        base.OnCultureChanged(sender, e);
        UpdateLocalizedTexts();
        UpdateLanguageDisplayNames();
        UpdateThemeDisplayNames();

        // Keep selection consistent after language change
        if (SelectedLanguage != null)
        {
            SetSelectedCulture(SelectedLanguage.CultureName);
        }

        if (SelectedTheme != null)
        {
            SetSelectedTheme(SelectedTheme.Key);
        }
    }

    protected override void UpdateLocalizedStrings()
    {
        UpdateLocalizedTexts();
    }

    private void UpdateLocalizedTexts()
    {
        WindowTitle = Resources.ApplicationSettingsTitle;
        LanguageLabel = Resources.LanguageLabel;
        ThemeLabel = Resources.ThemeLabel;
        ApplyButtonText = Resources.ButtonApply;
        ResetButtonText = Resources.ButtonReset;
        HeaderTracker.UpdateAllHeadersImmediate();
    }

    private void RegisterTrackedProperties()
    {
        var currentCulture = SelectedLanguage?.Key ?? AppSettings.CultureAuto;
        var currentTheme = SelectedTheme?.Key ?? AppSettings.ThemeAuto;

        HeaderTracker.Register(
            PropertyLanguage,
            currentCulture,
            () => Resources.LanguageLabel,
            value => LanguageHeaderText = value,
            value => LanguageHeaderFontStyle = value);

        HeaderTracker.Register(
            PropertyTheme,
            currentTheme,
            () => Resources.ThemeLabel,
            value => ThemeHeaderText = value,
            value => ThemeHeaderFontStyle = value);
    }

    private void BuildLanguages()
    {
        Languages.Clear();
        Languages.Add(new LanguageOption(AppSettings.CultureAuto, GetDisplayName(AppSettings.CultureAuto)));
        Languages.Add(new LanguageOption(AppSettings.CultureEnglish, GetDisplayName(AppSettings.CultureEnglish)));
        Languages.Add(new LanguageOption(AppSettings.CultureRussian, GetDisplayName(AppSettings.CultureRussian)));
    }

    private void BuildThemes()
    {
        Themes.Clear();
        Themes.Add(new ThemeOption(AppSettings.ThemeAuto, GetThemeDisplayName(AppSettings.ThemeAuto)));
        Themes.Add(new ThemeOption(AppSettings.ThemeLight, GetThemeDisplayName(AppSettings.ThemeLight)));
        Themes.Add(new ThemeOption(AppSettings.ThemeDark, GetThemeDisplayName(AppSettings.ThemeDark)));
    }

    private void UpdateLanguageDisplayNames()
    {
        foreach (var language in Languages)
        {
            language.DisplayName = GetDisplayName(language.CultureName);
        }
    }

    private void UpdateThemeDisplayNames()
    {
        foreach (var theme in Themes)
        {
            theme.DisplayName = GetThemeDisplayName(theme.Key);
        }
    }

    private static string GetDisplayName(string cultureName) =>
        cultureName switch
        {
            AppSettings.CultureAuto => Resources.LanguageAuto,
            AppSettings.CultureEnglish => Resources.LanguageEnglish,
            AppSettings.CultureRussian => Resources.LanguageRussian,
            _ => cultureName
        };

    private static string GetThemeDisplayName(string key) =>
        key switch
        {
            AppSettings.ThemeAuto => Resources.ThemeAuto,
            AppSettings.ThemeLight => Resources.ThemeLight,
            AppSettings.ThemeDark => Resources.ThemeDark,
            _ => key
        };

    private void LoadFromSettings()
    {
        LoadFromSettings(SettingsManager.Current);
    }

    private void SetSelectedCulture(string cultureName)
    {
        var match = Languages.FirstOrDefault(x => x.CultureName == cultureName)
                    ?? Languages.FirstOrDefault(x => x.CultureName == AppSettings.CultureAuto);

        SelectedLanguage = match;
    }

    private void SetSelectedTheme(string key)
    {
        var match = Themes.FirstOrDefault(x => x.Key == key)
                    ?? Themes.FirstOrDefault(x => x.Key == AppSettings.ThemeAuto);

        SelectedTheme = match;
    }

    [RelayCommand]
    private void Apply()
    {
        var culture = SelectedLanguage?.Key ?? AppSettings.CultureAuto;
        LocalizationInitializer.SwitchCulture(culture);

        var theme = SelectedTheme?.Key ?? AppSettings.ThemeAuto;
        ThemeInitializer.ApplyTheme(theme);

        // Save to settings
        var settings = SettingsManager.Current;
        settings.Culture = culture;
        settings.Theme = theme;
        SettingsManager.SaveCurrent();

        // Update original values in tracker (settings now contain the saved values)
        HeaderTracker.UpdateOriginal(PropertyLanguage, culture);
        HeaderTracker.UpdateOriginal(PropertyTheme, theme);
    }

    [RelayCommand]
    private void Reset()
    {
        SettingsManager.Reload();
        var culture = SettingsManager.Current.Culture;
        if (string.IsNullOrWhiteSpace(culture))
        {
            culture = AppSettings.CultureAuto;
        }

        SetSelectedCulture(culture);
        LocalizationInitializer.SwitchCulture(culture);

        var theme = SettingsManager.Current.Theme;
        if (string.IsNullOrWhiteSpace(theme))
        {
            theme = AppSettings.ThemeAuto;
        }

        SetSelectedTheme(theme);
        ThemeInitializer.ApplyTheme(theme);

        // Update original values in tracker
        HeaderTracker.UpdateOriginal(PropertyLanguage, culture);
        HeaderTracker.UpdateOriginal(PropertyTheme, theme);
        HeaderTracker.UpdateAllHeadersImmediate();
    }

    partial void OnSelectedLanguageChanged(LanguageOption? oldValue, LanguageOption? newValue)
    {
        HeaderTracker.Update(PropertyLanguage, newValue?.Key ?? AppSettings.CultureAuto);
    }

    partial void OnSelectedThemeChanged(ThemeOption? oldValue, ThemeOption? newValue)
    {
        HeaderTracker.Update(PropertyTheme, newValue?.Key ?? AppSettings.ThemeAuto);
    }

    partial void OnLanguageLabelChanged(string value)
    {
        HeaderTracker.UpdateAllHeaders();
    }

    partial void OnThemeLabelChanged(string value)
    {
        HeaderTracker.UpdateAllHeaders();
    }

    // Implementation of abstract methods from SettingsTabViewModelBase
    public override void LoadFromSettings(AppSettings settings)
    {
        var culture = settings.Culture;
        if (string.IsNullOrWhiteSpace(culture))
        {
            culture = AppSettings.CultureAuto;
        }

        SetSelectedCulture(culture);

        var theme = settings.Theme;
        if (string.IsNullOrWhiteSpace(theme))
        {
            theme = AppSettings.ThemeAuto;
        }

        SetSelectedTheme(theme);

        HeaderTracker.UpdateOriginal(PropertyLanguage, culture);
        HeaderTracker.UpdateOriginal(PropertyTheme, theme);
    }

    public override void SaveToSettings(AppSettings settings)
    {
        settings.Culture = SelectedLanguage?.Key ?? AppSettings.CultureAuto;
        settings.Theme = SelectedTheme?.Key ?? AppSettings.ThemeAuto;
    }

    public override void ResetToOriginal()
    {
        var originalCulture = HeaderTracker.GetOriginalValue<string>(PropertyLanguage) ?? AppSettings.CultureAuto;
        var originalTheme = HeaderTracker.GetOriginalValue<string>(PropertyTheme) ?? AppSettings.ThemeAuto;

        SetSelectedCulture(originalCulture);
        SetSelectedTheme(originalTheme);

        LocalizationInitializer.SwitchCulture(originalCulture);
        ThemeInitializer.ApplyTheme(originalTheme);

        HeaderTracker.UpdateAllHeadersImmediate();
    }
}

/// <summary>
/// Represents a language/culture option for application localization.
/// </summary>
public partial class LanguageOption : OptionBase
{
    /// <summary>
    /// Gets the culture name (alias for Key for backward compatibility).
    /// </summary>
    public string CultureName => Key;

    /// <summary>
    /// Initializes a new instance of the LanguageOption class.
    /// </summary>
    /// <param name="cultureName">The culture name (e.g., "en-US", "ru-RU").</param>
    /// <param name="displayName">The localized display name.</param>
    public LanguageOption(string cultureName, string displayName)
        : base(cultureName, displayName)
    {
    }
}

/// <summary>
/// Represents a theme option for application appearance.
/// </summary>
public partial class ThemeOption : OptionBase
{
    /// <summary>
    /// Initializes a new instance of the ThemeOption class.
    /// </summary>
    /// <param name="key">The theme key (e.g., "Auto", "Light", "Dark").</param>
    /// <param name="displayName">The localized display name.</param>
    public ThemeOption(string key, string displayName)
        : base(key, displayName)
    {
    }
}

