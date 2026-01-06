using System;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MiniCAM.Core.Localization;
using MiniCAM.Core.Settings;

namespace MiniCAM.Core.ViewModels;

public partial class ApplicationSettingsViewModel : ViewModelBase, IDisposable
{
    private string _originalCulture = AppSettings.CultureAuto;
    private string _originalTheme = AppSettings.ThemeAuto;

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
        Resources.CultureChanged += OnCultureChanged;
        BuildLanguages();
        BuildThemes();
        LoadFromSettings();
        UpdateHeaders();
    }

    private void OnCultureChanged(object? sender, CultureChangedEventArgs e)
    {
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

    private void UpdateLocalizedTexts()
    {
        WindowTitle = Resources.ApplicationSettingsTitle;
        LanguageLabel = Resources.LanguageLabel;
        ThemeLabel = Resources.ThemeLabel;
        ApplyButtonText = Resources.ButtonApply;
        ResetButtonText = Resources.ButtonReset;
        UpdateHeaders();
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
        var culture = SettingsManager.Current.Culture;
        if (string.IsNullOrWhiteSpace(culture))
        {
            culture = AppSettings.CultureAuto;
        }

        _originalCulture = culture;
        SetSelectedCulture(culture);

        var theme = SettingsManager.Current.Theme;
        if (string.IsNullOrWhiteSpace(theme))
        {
            theme = AppSettings.ThemeAuto;
        }

        _originalTheme = theme;
        SetSelectedTheme(theme);

        UpdateHeaders();
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
        var culture = SelectedLanguage?.CultureName ?? AppSettings.CultureAuto;
        LocalizationInitializer.SwitchCulture(culture);
        _originalCulture = culture;

        var theme = SelectedTheme?.Key ?? AppSettings.ThemeAuto;
        ThemeInitializer.ApplyTheme(theme);
        _originalTheme = theme;

        UpdateHeaders();
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

        _originalCulture = culture;
        _originalTheme = theme;
        UpdateHeaders();
    }

    partial void OnSelectedLanguageChanged(LanguageOption? oldValue, LanguageOption? newValue)
    {
        UpdateHeaders();
    }

    partial void OnSelectedThemeChanged(ThemeOption? oldValue, ThemeOption? newValue)
    {
        UpdateHeaders();
    }

    partial void OnLanguageLabelChanged(string value)
    {
        UpdateHeaders();
    }

    partial void OnThemeLabelChanged(string value)
    {
        UpdateHeaders();
    }

    private void UpdateHeaders()
    {
        var currentCulture = SelectedLanguage?.CultureName ?? AppSettings.CultureAuto;
        var currentTheme = SelectedTheme?.Key ?? AppSettings.ThemeAuto;

        var isLanguageModified = !string.Equals(currentCulture, _originalCulture, StringComparison.Ordinal);
        var isThemeModified = !string.Equals(currentTheme, _originalTheme, StringComparison.Ordinal);

        LanguageHeaderText = isLanguageModified ? $"{LanguageLabel} *" : LanguageLabel;
        ThemeHeaderText = isThemeModified ? $"{ThemeLabel} *" : ThemeLabel;

        LanguageHeaderFontStyle = isLanguageModified ? FontStyle.Italic : FontStyle.Normal;
        ThemeHeaderFontStyle = isThemeModified ? FontStyle.Italic : FontStyle.Normal;
    }

    public void Dispose()
    {
        Resources.CultureChanged -= OnCultureChanged;
    }
}

public partial class LanguageOption : ObservableObject
{
    public string CultureName { get; }

    [ObservableProperty]
    private string _displayName;

    public LanguageOption(string cultureName, string displayName)
    {
        CultureName = cultureName;
        _displayName = displayName;
    }
}

public partial class ThemeOption : ObservableObject
{
    public string Key { get; }

    [ObservableProperty]
    private string _displayName;

    public ThemeOption(string key, string displayName)
    {
        Key = key;
        _displayName = displayName;
    }
}

