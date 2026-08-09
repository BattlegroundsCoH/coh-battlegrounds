using System.Windows;

using Battlegrounds.Services;
using Battlegrounds.Views;
using Battlegrounds.Views.Dev;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Serilog;

using Velopack;

namespace Battlegrounds;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application {

    private IServiceProvider? _serviceProvider = null!;

    /// <summary>Logging for the --gallery path, which never builds the DI container.</summary>
    private ILoggerFactory? _galleryLoggerFactory;

    public App() {
        VelopackApp.Build().Run();
    }

    protected override void OnStartup(StartupEventArgs e) {

        base.OnStartup(e);

        // --gallery opens the design-system gallery instead of the app.
        if (e.Args.Contains("--gallery")) {
            _galleryLoggerFactory = LoggerFactory.Create(builder => builder.AddSerilog(new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console(outputTemplate: "{Timestamp:HH:mm:ss} [{Level:u3}] {Message}{NewLine}{Exception}")
                .CreateLogger(), dispose: true));
            new StyleGalleryWindow(_galleryLoggerFactory).Show();
            return;
        }

        var bgApp = new BattlegroundsApp(e.Args);
        bgApp.ConfigureFileStorage();

        var services = new ServiceCollection();
        services.AddSingleton(bgApp);

        bgApp.ConfigureServices(services);

        _serviceProvider = services.BuildServiceProvider();
        bgApp.ServiceProvider = _serviceProvider;
        bgApp.FinishStartup();

        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Title = "Company of Heroes: Battlegrounds";
        mainWindow.Show();

    }

    private static readonly TimeSpan LogOutGracePeriod = TimeSpan.FromSeconds(3);

    protected override void OnExit(ExitEventArgs e) {

        if (_serviceProvider is not null) {
            _serviceProvider.GetRequiredService<IUserService>()
                .WaitForPendingLogOutAsync(LogOutGracePeriod)
                .GetAwaiter().GetResult();
            (_serviceProvider as IDisposable)?.Dispose(); // Stops the background token refresh
        }

        _galleryLoggerFactory?.Dispose(); // Flushes the gallery's console sink; a no-op in the normal path.
        base.OnExit(e);

    }

}
