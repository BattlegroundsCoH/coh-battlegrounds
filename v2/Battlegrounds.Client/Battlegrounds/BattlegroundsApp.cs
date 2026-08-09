using System.IO;
using System.Net.Http;

using Battlegrounds.Facades.API;
using Battlegrounds.Factories;
using Battlegrounds.Logging;
using Battlegrounds.Models;
using Battlegrounds.Models.Lobbies;
using Battlegrounds.Models.Playing;
using Battlegrounds.Parsers;
using Battlegrounds.Serializers;
using Battlegrounds.Services;
using Battlegrounds.Services.Data;
using Battlegrounds.Services.Infrastructure;
using Battlegrounds.Services.Playing;
using Battlegrounds.ViewModels;
using Battlegrounds.ViewModels.Modals;
using Battlegrounds.Views;
using Battlegrounds.Views.Modals;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Serilog;

namespace Battlegrounds;

/// <summary>
/// Represents the main application class that manages the lifecycle, configuration, and dependency injection for the
/// CoH Battlegrounds application.
/// </summary>
/// <remarks>Implements a singleton pattern through the <see cref="Instance"/> property. Handles first-run
/// detection, file storage configuration in AppData and My Documents folders, and supports a no-play mode for testing
/// and development purposes when launched with the --noplay argument.</remarks>
public sealed class BattlegroundsApp {

    public static BattlegroundsApp? Instance { get; private set; }

    private IServiceProvider? _serviceProvider = null!;
    private bool _isFirstRun = false;

    private readonly string _appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CoHBattlegrounds");
    public static readonly string DocumentsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "my games", "CoHBattlegrounds");

    private readonly string _configFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "my games", "CoHBattlegrounds", "config.json");

    private Configuration _configuration = new Configuration();

    /// <summary>
    /// Gets the version string of the application assembly.
    /// </summary>
    /// <remarks>The version is retrieved from the assembly metadata. If the version information is
    /// unavailable, the property returns "v0.0.0".</remarks>
    public static string Version => typeof(BattlegroundsApp).Assembly.GetName().Version?.ToString() ?? "v0.0.0";

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

    public bool IsFirstRun => _isFirstRun;

    public bool IsNoPlayModeConfigured { get; }

    /// <summary>
    /// Persists the current in-memory configuration to disk.
    /// </summary>
    public void SaveConfiguration() {
        File.WriteAllText(_configFilePath, _configuration.ToJson());
    }

    public BattlegroundsApp(params string[] args) {
        if (Instance is not null) {
            throw new InvalidOperationException("BattlegroundsApp instance already exists.");
        }
        var argsAsHashset = new HashSet<string>();
        Instance = this;
        IsNoPlayModeConfigured = args.Contains("--noplay"); // Instructs the app to not actually launch Company of Heroes
    }

    public void ConfigureFileStorage() {

        if (!Directory.Exists(_appDataPath)) {
            Directory.CreateDirectory(_appDataPath);
            _isFirstRun = true;
        }

        InitMyGamesFolder();

    }

    private void InitMyGamesFolder() {

        if (!Directory.Exists(DocumentsPath)) {
            Directory.CreateDirectory(DocumentsPath);
            _isFirstRun = true;
        }

        if (!File.Exists(_configFilePath)) {
            // Create a default config file if it doesn't exist
            File.WriteAllText(_configFilePath, _configuration.ToJson());
            _isFirstRun = true;
        } else {
            try {
                using var stream = File.OpenRead(_configFilePath);
                _configuration = Configuration.FromJson(stream) ?? DefaultConfig();
            } catch (Exception) {
                // If reading the config file fails, log the error and create a new config file
                // TODO: Add logging here
                _configuration = DefaultConfig();
                File.WriteAllText(_configFilePath, _configuration.ToJson());
            }
        }

        if (!Directory.Exists(_configuration.CompaniesPath)) {
            Directory.CreateDirectory(_configuration.CompaniesPath);
            _isFirstRun = true;
        }

    }

    private static Configuration DefaultConfig() {
        return new Configuration {
            CompaniesPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "my games", "CoHBattlegrounds", "companies"), // May override the one in configuration or elsewhere
        };
    }

    public void ConfigureServices(ServiceCollection services) {

        // Create Logger
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Is(_configuration.LogLevel switch {
                "trace" => Serilog.Events.LogEventLevel.Verbose,
                "debug" => Serilog.Events.LogEventLevel.Debug,
                "info" => Serilog.Events.LogEventLevel.Information,
                "warning" => Serilog.Events.LogEventLevel.Warning,
                "error" => Serilog.Events.LogEventLevel.Error,
                "fatal" => Serilog.Events.LogEventLevel.Fatal,
                _ => Serilog.Events.LogEventLevel.Information
            })
            .Enrich.FromLogContext() // Enrich with source context (class name)
            .Enrich.With<ClassSourceEnricher>()
            .WriteTo.Console(
                outputTemplate: "{Timestamp:HH:mm:ss} [{Level:u3}] ({ClassName}) {Message}{NewLine}{Exception}"
            )  // Custom format with full log level and class name
            .WriteTo.File(Path.Combine(DocumentsPath, "logs", $"battlegrounds-{timestamp}.log"), retainedFileCountLimit: 7,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] ({ClassName}) {Message}{NewLine}{Exception}"
            )
            .CreateLogger();

        Log.ForContext<BattlegroundsApp>()
            .Information("Battlegrounds is starting up...");

        // Register Serilog as the logging provider
        services.AddLogging(builder => builder.AddSerilog(dispose: true));

        // Register self
        services.AddSingleton(this);

        // Register configuration
        services.AddSingleton(x => _configuration);

        // Register commands
        // TODO: ...

        // Register main window
        services.AddTransient<MainWindow>();
        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<UserViewModel>();

        // Register Home view
        services.AddTransient<HomeView>();
        services.AddSingleton<HomeViewModel>();

        // Register Multiplayer view
        services.AddTransient<MultiplayerView>();
        services.AddSingleton<MultiplayerViewModel>();

        // Register Login view model
        services.AddTransient<LoginView>();
        services.AddSingleton<LoginViewModel>();

        // Register Company Browser view model
        services.AddTransient<CompanyBrowserView>();
        services.AddSingleton<CompanyBrowserViewModel>();

        // Register News view
        services.AddTransient<NewsView>();
        services.AddSingleton<NewsViewModel>();

        // Register Company Editor view model
        services.AddTransient<CompanyEditorView>();
        services.AddSingleton<CompanyEditorViewModel>();

        // Register Settings view
        services.AddTransient<SettingsView>();
        services.AddTransient<SettingsViewModel>();

        // Register other view models as needed

        // Regiser modal for create lobby
        services.AddTransient<CreateLobbyModalView>();
        services.AddTransient<CreateLobbyModalViewModel>(); // Note: this is transient, so a new instance will be created each time it's requested

        // Register modal for create company
        services.AddTransient<CreateCompanyModalView>();
        services.AddTransient<CreateCompanyModalViewModel>(); // Note: this is transient, so a new instance will be created each time it's requested

        // Register modal for fixing a broken doctrine
        services.AddTransient<FixDoctrineModalView>();
        services.AddTransient<FixDoctrineModalViewModel>(); // Note: this is transient, so a new instance will be created each time it's requested

        // Register generic modal
        services.AddTransient<DialogModalView>();
        services.AddTransient<DialogModalViewModel>(); // Note: this is transient, so a new instance will be created each time it's requested

        // Register services
        services.AddSingleton<IUpdateService, UpdateService>();
        services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton<ILobbyService, LobbyService>();
        services.AddSingleton<IReplayService, ReplayService>();
        services.AddSingleton<IGameService, GameService>();
        services.AddSingleton<IGameMapService, GameMapService>();
        services.AddSingleton<IArchiverService, CoH3ArchiverService>();
        services.AddSingleton<CoH3ArchiverService>();
        services.AddSingleton<CoH3ReplayParser>();
        services.AddSingleton<ScenarioParser<CoH3>>();
        services.AddSingleton<IUserService>(sp => new UserService(
            sp.GetRequiredService<ILogger<UserService>>(),
            sp.GetRequiredService<IBattlegroundsWebAPI>(),
            sp.GetRequiredService<IBrowserService>()));
        services.AddSingleton<ICompanyService, CompanyService>();
        services.AddSingleton<IGameLocaleService, GameLocaleService>();
        services.AddSingleton<IBlueprintService, BlueprintService>();
        services.AddSingleton<IStatisticsService, StatisticsService>();
        services.AddSingleton<IBrowserService, BrowserService>();
        services.AddSingleton<INewsService, NewsService>();
        services.AddSingleton<IImageCacheService, ImageCacheService>();
        services.AddSingleton<IUiScaleService, UiScaleService>();
        services.AddSingleton<IDoctrineService, DoctrineService>();
        services.AddSingleton<ICompanySerializer, BinaryCompanySerializer>();
        services.AddSingleton<ICompanyDeserializer, BinaryCompanyDeserializer>();
        services.AddSingleton<IBattlegroundsServerAPI, HttpBattlegroundsServerAPI>();
        services.AddSingleton<IBattlegroundsWebAPI, HttpBattlegroundsWebAPI>();
        services.AddSingleton(TimeProvider.System);
        services.AddTransient<GrpcServerClientFactory>();
        services.AddTransient<LobbySetupFromConfigFactory>();

        if (IsNoPlayModeConfigured) {
            Log.ForContext<BattlegroundsApp>()
                .Information("Battlegrounds is configured to be in no-play mode...");
            services.AddSingleton(new SimulationParameters {
                LaunchSuccessful = true,
                RunParameters = new SimulatedAppRunParameters {

                }
            });
            services.AddSingleton<IPlayService, SimulatedPlayService>(); // Use the simulated play service which does not actually launch games, for testing and development purposes
        } else {
            Log.ForContext<BattlegroundsApp>()
                .Information("Battlegrounds is configured to be in play mode...");
            services.AddSingleton<IPlayService, PlayService>();
        }

        // Add getters
        services.AddSingleton(services => services.GetRequiredService<IGameService>().GetGame<CoH3>());
        // TODO: Add getter for CoH2

        // Add factories
        services.AddSingleton<MultiplayerLobbyFactory>();

        // Register default HTTP client
        services.AddSingleton(new HttpClient());
        services.AddSingleton<IAsyncHttpClient, AsyncHttpClient>();

    }

    public async void FinishStartup() {

        if (ServiceProvider is null)
            throw new Exception("ServiceProvider is not set.");

        var logger = ServiceProvider.GetRequiredService<ILogger<BattlegroundsApp>>();
        logger.LogInformation("Battlegrounds is finishing startup...");

        // Apply the UI scale before anything can await: App.OnStartup resolves MainWindow as soon as
        // this method yields, and the window reads the scaled size tokens while it is being built.
        ServiceProvider.GetRequiredService<IUiScaleService>().Apply(_configuration.UiScale);

        // Trigger async loading of blueprints
        LoadData(ServiceProvider);

        // Trigger async loading of statistics
        LoadStats(ServiceProvider);

        // Trigger async loading of gamemode sources
        LoadGamemodeSources(ServiceProvider);

        // Trigger auto login
        var loginViewModel = ServiceProvider.GetRequiredService<LoginViewModel>();
        if (!_isFirstRun) {
            if (!await loginViewModel.AutoLoginAsync()) {
                logger.LogWarning("Auto-login failed -- user must log in again.");
            }
        }

        var userService = ServiceProvider.GetRequiredService<IUserService>();
        await userService.IsUserLoggedIn;

        var updateService = ServiceProvider.GetRequiredService<IUpdateService>();
        if (await updateService.CheckForUpdatesAsync()) {
            logger.LogInformation("An update is available.");
            var homeViewModel = ServiceProvider.GetRequiredService<HomeViewModel>();
            homeViewModel.NotifyUpdateAvailable(updateService.AvailableVersion ?? string.Empty);
        }

    }

    private static async void LoadData(IServiceProvider serviceProvider) {

        var logger = serviceProvider.GetRequiredService<ILogger<BattlegroundsApp>>();

        // Trigger async loading of locales
        var localeService = serviceProvider.GetRequiredService<IGameLocaleService>();
        if (!await localeService.LoadLocalesAsync()) {
            logger.LogError("Failed to load game locales. The application may not function correctly.");
        } else {
            logger.LogInformation("Game locales loaded successfully.");
        }

        var gameMapService = serviceProvider.GetRequiredService<IGameMapService>();
        var loadMapsTask = gameMapService.LoadMapsAsync();

        var blueprintService = serviceProvider.GetRequiredService<IBlueprintService>();
        var doctrineService = serviceProvider.GetRequiredService<IDoctrineService>();
        var loadBlueprintsAndDoctrinesTask = blueprintService.LoadBlueprints().ContinueWith(_ => doctrineService.LoadDoctrines(CancellationToken.None));

        await Task.WhenAll(loadMapsTask, loadBlueprintsAndDoctrinesTask);

        var companyService = serviceProvider.GetRequiredService<ICompanyService>();
        int companyCount = await companyService.LoadPlayerCompaniesAsync();
        logger.LogInformation("Loaded {Count} companies from local store", companyCount);

        // Notify relevant view models that data has been loaded
        var homeViewModel = serviceProvider.GetRequiredService<HomeViewModel>();
        homeViewModel.OnDataLoaded();

        // Sync with server to get any updates to companies
        await companyService.SyncWithServerAsync();

    }

    private static async void LoadStats(IServiceProvider serviceProvider) {
        var logger = serviceProvider.GetRequiredService<ILogger<BattlegroundsApp>>();
        var statisticsService = serviceProvider.GetRequiredService<IStatisticsService>();
        await statisticsService.LoadStatisticsAsync();
        logger.LogInformation("Loaded player statistics from local store");
    }

    private static async void LoadGamemodeSources(IServiceProvider serviceProvider) {
        var playService = serviceProvider.GetRequiredService<IPlayService>();
        await playService.EnsureModSourceIsAvailable();
    }

}
