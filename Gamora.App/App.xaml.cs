using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using Gamora.App.ViewModels;
using Gamora.App.ViewModels.Admin;
using Gamora.App.Views.Admin;
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

        DispatcherUnhandledException += OnDispatcherUnhandledException;

        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();

        var isAdminMode = e.Args.Any(a => string.Equals(a, "--admin", StringComparison.OrdinalIgnoreCase));

        if (isAdminMode)
        {
            await RunAdminModeAsync();
        }
        else
        {
            await RunCustomerModeAsync();
        }
    }

    private static void ConfigureServices(ServiceCollection services)
    {
        services.AddSingleton<ICatalogService, CatalogService>();
        services.AddSingleton<IPathResolver, PathResolver>();
        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddSingleton<IPasswordService, PasswordService>();
        services.AddSingleton<ILaunchStrategy, ExeLaunchStrategy>();
        services.AddSingleton<ILaunchStrategy, SteamLaunchStrategy>();
        services.AddSingleton<ILaunchStrategy, RiotLaunchStrategy>();
        services.AddSingleton<ILaunchStrategy, BattleNetLaunchStrategy>();
        services.AddSingleton<ILaunchStrategy, EpicLaunchStrategy>();
        services.AddSingleton<IStatsService, StatsService>();
        services.AddSingleton<IPopularityService, PopularityService>();
        services.AddSingleton<IGameLauncher, GameLauncher>();

        // Müşteri modu
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<MainWindow>();

        // Yönetici modu
        services.AddTransient<PasswordSetupViewModel>();
        services.AddTransient<PasswordSetupWindow>();
        services.AddTransient<PasswordLoginViewModel>();
        services.AddTransient<PasswordLoginWindow>();
        services.AddSingleton<AdminMainViewModel>();
        services.AddSingleton<AdminMainWindow>();
    }

    private async Task RunCustomerModeAsync()
    {
        var provider = _serviceProvider!;
        var mainWindow = provider.GetRequiredService<MainWindow>();
        mainWindow.Show();

        // ShowDialog() gerektirmeyen tek pencereli akış: artık kapanınca uygulama da
        // kapanmalı — bkz. RunAdminModeAsync'teki ShutdownMode açıklaması.
        ShutdownMode = ShutdownMode.OnLastWindowClose;

        var mainViewModel = provider.GetRequiredService<MainViewModel>();
        await mainViewModel.InitializeAsync();
    }

    private async Task RunAdminModeAsync()
    {
        var provider = _serviceProvider!;
        var settingsService = provider.GetRequiredService<ISettingsService>();
        var passwordService = provider.GetRequiredService<IPasswordService>();
        var settings = await settingsService.LoadAsync();

        bool authenticated;
        if (!passwordService.IsPasswordSet(settings.AdminLockPath))
        {
            Log.Information("Admin modu: sfr.lock yok, kurulum ekranı açılıyor");
            var setupWindow = provider.GetRequiredService<PasswordSetupWindow>();
            authenticated = setupWindow.ShowDialog() == true;
        }
        else
        {
            Log.Information("Admin modu: sfr.lock var, giriş ekranı açılıyor");
            var loginWindow = provider.GetRequiredService<PasswordLoginWindow>();
            authenticated = loginWindow.ShowDialog() == true;
        }

        Log.Information("Admin modu: giriş penceresi kapandı, sonuç={Authenticated}", authenticated);

        if (!authenticated)
        {
            Log.Information("Admin modu: kimlik doğrulanamadı, uygulama kapatılıyor");
            Shutdown();
            return;
        }

        // App.xaml'de ShutdownMode="OnExplicitShutdown" ayarlı: yukarıdaki ShowDialog()
        // penceresi kapanırken (o an açık tek pencere) WPF'in varsayılan
        // OnLastWindowClose davranışı devreye girip AdminMainWindow hiç açılmadan
        // uygulamayı sonlandırıyordu. Gerçek ana pencere gösterildikten sonra modu
        // tekrar OnLastWindowClose'a alıyoruz ki o pencere kapanınca uygulama düzgün çıksın.
        Log.Information("Admin modu: giriş başarılı, yönetici ana penceresi oluşturuluyor");
        var adminWindow = provider.GetRequiredService<AdminMainWindow>();

        Log.Information("Admin modu: yönetici ana penceresi oluşturuldu, gösteriliyor");
        adminWindow.Show();
        ShutdownMode = ShutdownMode.OnLastWindowClose;
        Log.Information("Admin modu: yönetici ana penceresi gösterildi");
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        Log.Fatal(e.Exception, "Yakalanmamış istisna — uygulama çökebilir");
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Log.CloseAndFlush();
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }
}
