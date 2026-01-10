using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MiniCAM.Core.Settings;
using Avalonia.Markup.Xaml;
using MiniCAM.Core.Localization;
using MiniCAM.Core.Views;
using MainViewModel = MiniCAM.Core.ViewModels.Main.MainViewModel;

namespace MiniCAM.Core;

public partial class App : Application
{
    public static IServiceProvider? Services { get; private set; }

    public override void Initialize()
    {
        // Setup dependency injection
        var serviceCollection = new ServiceCollection();
        
        // Add logging
        serviceCollection.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.AddDebug();
            builder.SetMinimumLevel(LogLevel.Information);
        });
        
        // Register settings service with logging
        serviceCollection.AddSingleton<ISettingsService>(sp =>
        {
            var logger = sp.GetService<ILogger<SettingsService>>();
            return new SettingsService(logger);
        });
        
        Services = serviceCollection.BuildServiceProvider();

        // Get settings service for initialization
        var settingsService = Services.GetRequiredService<ISettingsService>();

        // Initialize localization before loading XAML
        LocalizationInitializer.Initialize(settingsService);
        
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();

            var settingsService = Services?.GetRequiredService<ISettingsService>();
            if (settingsService == null)
                throw new InvalidOperationException("ISettingsService is not registered in DI container.");

            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainViewModel(settingsService)
            };

            // Apply theme from settings after main window is created
            ThemeInitializer.Initialize(settingsService);
        }
        
        base.OnFrameworkInitializationCompleted();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}