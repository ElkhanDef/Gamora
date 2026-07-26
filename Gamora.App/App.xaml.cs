using System.IO;
using System.Windows;
using Gamora.App.ViewModels;
using Gamora.Core.Abstractions;
using Gamora.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Gamora.App;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private ServiceProvider? _serviceProvider;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(
                Path.Combine(AppContext.BaseDirectory, "logs", "gamora-.log"),
                rollingInterval: RollingInterval.Day)
            .CreateLogger();

        var services = new ServiceCollection();
        services.AddSingleton<ICatalogService, CatalogService>();
        services.AddSingleton<IPathResolver, PathResolver>();
        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddSingleton<ILaunchStrategy, ExeLaunchStrategy>();
        services.AddSingleton<ILaunchStrategy, SteamLaunchStrategy>();
        services.AddSingleton<ILaunchStrategy, RiotLaunchStrategy>();
        services.AddSingleton<ILaunchStrategy, BattleNetLaunchStrategy>();
        services.AddSingleton<ILaunchStrategy, EpicLaunchStrategy>();
        services.AddSingleton<IGameLauncher, GameLauncher>();
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<MainWindow>();
        _serviceProvider = services.BuildServiceProvider();

        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();

        var mainViewModel = _serviceProvider.GetRequiredService<MainViewModel>();
        await mainViewModel.InitializeAsync();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Log.CloseAndFlush();
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }
}
