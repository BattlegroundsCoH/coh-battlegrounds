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

public sealed class BattlegroundsApp {

    public static BattlegroundsApp? Instance { get; private set; }

    private IServiceProvider? _serviceProvider = null!;
    private bool _isFirstRun = false;

    private readonly string _appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CoHBattlegrounds");
    public static readonly string DocumentsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "my games", "CoHBattlegrounds");

    private readonly string _configFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "my games", "CoHBattlegrounds", "config.json");

    private Configuration _configuration = new Configuration();

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

    public BattlegroundsApp() {
        if (Instance is not null) {
            throw new InvalidOperationException("BattlegroundsApp instance already exists.");
        }
        Instance = this;
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

        // Register Company Editor view model
        services.AddTransient<CompanyEditorView>();
        services.AddSingleton<CompanyEditorViewModel>();

        // Register other view models as needed

        // Regiser modal for create lobby
        services.AddTransient<CreateLobbyModalView>();
        services.AddTransient<CreateLobbyModalViewModel>(); // Note: this is transient, so a new instance will be created each time it's requested

        // Register modal for create company
        services.AddTransient<CreateCompanyModalView>();
        services.AddTransient<CreateCompanyModalViewModel>(); // Note: this is transient, so a new instance will be created each time it's requested

        // Register generic modal
        services.AddTransient<DialogModalView>();
        services.AddTransient<DialogModalViewModel>(); // Note: this is transient, so a new instance will be created each time it's requested

        // Register services
        services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton<ILobbyService, LobbyService>();
        services.AddSingleton<IPlayService, PlayService>();
        services.AddSingleton<IReplayService, ReplayService>();
        services.AddSingleton<IGameService, GameService>();
        services.AddSingleton<IGameMapService, GameMapService>();
        services.AddSingleton<IArchiverService, CoH3ArchiverService>();
        services.AddSingleton<CoH3ArchiverService>();
        services.AddSingleton<CoH3ReplayParser>();
        services.AddSingleton<IUserService, UserService>();
        services.AddSingleton<ICompanyService, CompanyService>();
        services.AddSingleton<IGameLocaleService, GameLocaleService>();
        services.AddSingleton<IBlueprintService, BlueprintService>();
        services.AddSingleton<IStatisticsService, StatisticsService>();
        services.AddSingleton<IBrowserService, BrowserService>();
        services.AddSingleton<ICompanySerializer, BinaryCompanySerializer>();
        services.AddSingleton<ICompanyDeserializer, BinaryCompanyDeserializer>();
        services.AddSingleton<IBattlegroundsServerAPI, HttpBattlegroundsServerAPI>();
        services.AddSingleton<IBattlegroundsWebAPI, HttpBattlegroundsWebAPI>();
        services.AddTransient<GrpcServerClientFactory>();
        services.AddTransient<LobbySetupFromConfigFactory>();

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

        // Trigger async loading of locales
        var localeService = ServiceProvider.GetRequiredService<IGameLocaleService>();
        if (!await localeService.LoadLocalesAsync()) {
            logger.LogError("Failed to load game locales. The application may not function correctly.");
        } else {
            logger.LogInformation("Game locales loaded successfully.");
        }

        // Trigger async loading of blueprints
        LoadData(ServiceProvider);

        // Trigger async loading of statistics
        LoadStats(ServiceProvider);

        // Trigger auto login
        var loginViewModel = ServiceProvider.GetRequiredService<LoginViewModel>();
        if (_isFirstRun) {
            logger.LogInformation("This is the first run of Battlegrounds. Unable to auto-login");
        } else {
            if (!await loginViewModel.AutoLoginAsync()) {
                logger.LogWarning("Auto-login failed. Please log in manually.");
            }
        }

    }

    private static async void LoadData(IServiceProvider serviceProvider) {

        var logger = serviceProvider.GetRequiredService<ILogger<BattlegroundsApp>>();
        var blueprintService = serviceProvider.GetRequiredService<IBlueprintService>();
        await blueprintService.LoadBlueprints();

        var companyService = serviceProvider.GetRequiredService<ICompanyService>();
        int companyCount = await companyService.LoadPlayerCompaniesAsync();
        logger.LogInformation("Loaded {Count} companies from local store", companyCount);

        // Notify relevant view models that data has been loaded
        var homeViewModel = serviceProvider.GetRequiredService<HomeViewModel>();
        homeViewModel.OnDataLoaded();

    }

    private static async void LoadStats(IServiceProvider serviceProvider) {
        var logger = serviceProvider.GetRequiredService<ILogger<BattlegroundsApp>>();
        var statisticsService = serviceProvider.GetRequiredService<IStatisticsService>();
        await statisticsService.LoadStatisticsAsync();
        logger.LogInformation("Loaded player statistics from local store");
    }

}
