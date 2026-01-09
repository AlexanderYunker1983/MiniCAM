using System;
using System.Collections.Generic;
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
    private readonly ISettingsService _settingsService;
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

    // Property headers dictionary for change tracking
    private readonly Dictionary<string, PropertyHeaderViewModel> _headers = new();

    /// <summary>
    /// Gets the header view model for language property.
    /// </summary>
    public PropertyHeaderViewModel LanguageHeader => _headers[PropertyLanguage];

    /// <summary>
    /// Gets the header view model for theme property.
    /// </summary>
    public PropertyHeaderViewModel ThemeHeader => _headers[PropertyTheme];

    [ObservableProperty]
    private string _applyButtonText = Resources.ButtonApply;

    [ObservableProperty]
    private string _resetButtonText = Resources.ButtonReset;

    public ApplicationSettingsViewModel(ISettingsService settingsService)
    {
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
        
        // Initialize headers
        _headers[PropertyLanguage] = new PropertyHeaderViewModel(Resources.LanguageLabel);
        _headers[PropertyTheme] = new PropertyHeaderViewModel(Resources.ThemeLabel);
        
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
            _headers[PropertyLanguage]);

        HeaderTracker.Register(
            PropertyTheme,
            currentTheme,
            () => Resources.ThemeLabel,
            _headers[PropertyTheme]);
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
        LoadFromSettings(_settingsService.Current);
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
        LocalizationInitializer.SwitchCulture(culture, _settingsService);

        var theme = SelectedTheme?.Key ?? AppSettings.ThemeAuto;
        ThemeInitializer.ApplyTheme(theme, _settingsService);

        // Save to settings
        var settings = _settingsService.Current;
        settings.Culture = culture;
        settings.Theme = theme;
        _settingsService.SaveCurrent();

        // Update original values in tracker (settings now contain the saved values)
        HeaderTracker.UpdateOriginal(PropertyLanguage, culture);
        HeaderTracker.UpdateOriginal(PropertyTheme, theme);
    }

    [RelayCommand]
    private void Reset()
    {
        _settingsService.Reload();
        var culture = _settingsService.Current.Culture;
        if (string.IsNullOrWhiteSpace(culture))
        {
            culture = AppSettings.CultureAuto;
        }

        SetSelectedCulture(culture);
        LocalizationInitializer.SwitchCulture(culture, _settingsService);

        var theme = _settingsService.Current.Theme;
        if (string.IsNullOrWhiteSpace(theme))
        {
            theme = AppSettings.ThemeAuto;
        }

        SetSelectedTheme(theme);
        ThemeInitializer.ApplyTheme(theme, _settingsService);

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


    // Implementation of abstract methods from SettingsTabViewModelBase
    public override void LoadFromSettings(AppSettings settings)
    {
        var culture = LoadStringProperty(settings, s => s.Culture, AppSettings.CultureAuto, PropertyLanguage, _ => { });
        SetSelectedCulture(culture);

        var theme = LoadStringProperty(settings, s => s.Theme, AppSettings.ThemeAuto, PropertyTheme, _ => { });
        SetSelectedTheme(theme);
    }

    public override void SaveToSettings(AppSettings settings)
    {
        settings.Culture = SelectedLanguage?.Key ?? AppSettings.CultureAuto;
        settings.Theme = SelectedTheme?.Key ?? AppSettings.ThemeAuto;
    }

    public override void ResetToOriginal()
    {
        var originalCulture = ResetStringProperty(PropertyLanguage, AppSettings.CultureAuto, _ => { });
        SetSelectedCulture(originalCulture);

        var originalTheme = ResetStringProperty(PropertyTheme, AppSettings.ThemeAuto, _ => { });
        SetSelectedTheme(originalTheme);

        LocalizationInitializer.SwitchCulture(originalCulture, _settingsService);
        ThemeInitializer.ApplyTheme(originalTheme, _settingsService);

        HeaderTracker.UpdateAllHeadersImmediate();
    }
}

