using System.Net.Http;

using Battlegrounds.Models;
using Battlegrounds.Services;
using Battlegrounds.ViewModels;
using Battlegrounds.ViewModels.Modals;
using Battlegrounds.Views;
using Battlegrounds.Views.Modals;

using Microsoft.Extensions.DependencyInjection;

namespace Battlegrounds;

public sealed class BattlegroundsApp {

    private IServiceProvider? _serviceProvider = null!;

    public IServiceProvider? ServiceProvider {
        get => _serviceProvider;
        set {
            if (_serviceProvider is null) {
                _serviceProvider = value;
            } else {
                throw new InvalidOperationException("ServiceProvider is already set.");
            }
        }
    }

    public void ConfigureServices(ServiceCollection services) {

        // Register self
        services.AddSingleton(this);

        // Register configuration
        services.AddSingleton(x => new Configuration());

        // Register commands
        // TODO: ...

        // Register main window
        services.AddTransient<MainWindow>();
        services.AddSingleton<MainWindowViewModel>();
        
        // Register Multiplayer view
        services.AddTransient<MultiplayerView>();
        services.AddSingleton<MultiplayerViewModel>();

        // Register other view models as needed

        // Regiser modal for create lobby
        services.AddTransient<CreateLobbyModalView>();
        services.AddTransient<CreateLobbyModalViewModel>(); // Note: this is transient, so a new instance will be created each time it's requested

        // Register services
        services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton<ILobbyBrowserService, LobbyBrowserService>();
        services.AddSingleton<ILobbyService, LobbyService>();
        services.AddSingleton<IPlayService, PlayService>();
        services.AddSingleton<IReplayService, ReplayService>();
        services.AddSingleton<IGameService, GameService>();
        services.AddSingleton<IGameMapService, GameMapService>();
        services.AddSingleton<IArchiverService, CoH3ArchiverService>();
        services.AddSingleton<CoH3ArchiverService>();
        services.AddSingleton<IUserService, UserService>();
        services.AddSingleton<ICompanyService, CompanyService>();
        services.AddSingleton<IBlueprintService, BlueprintService>();

        // Register default HTTP client
        services.AddSingleton(new HttpClient()); // TODO: Make a wrapper for HttpClient and specify an interface to decouple it from the implementation

    }

    public void FinishStartup() {

        if (ServiceProvider is null)
            throw new Exception("ServiceProvider is not set.");

        // Trigger async loading of blueprints
        var blueprintService = ServiceProvider.GetRequiredService<IBlueprintService>();
        blueprintService.LoadBlueprints();

    }

}
