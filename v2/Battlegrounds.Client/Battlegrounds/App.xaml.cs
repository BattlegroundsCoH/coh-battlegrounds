using System.Windows;

using Battlegrounds.Views;

using Microsoft.Extensions.DependencyInjection;

namespace Battlegrounds;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application {

    private IServiceProvider? _serviceProvider = null!;

    protected override void OnStartup(StartupEventArgs e) {
        
        base.OnStartup(e);

        var bgApp = new BattlegroundsApp();
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

}
