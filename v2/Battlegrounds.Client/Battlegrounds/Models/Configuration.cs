using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

using Battlegrounds.Models.Lobbies;
using Battlegrounds.Models.Playing;

namespace Battlegrounds.Models;

/// <summary>
/// Represents the configuration settings for the application, including game-specific settings, API configurations,
/// server details, and other application-level options.
/// </summary>
/// <remarks>This class provides a centralized structure for managing various configuration settings used
/// throughout the application. It includes settings for Company of Heroes 2 and 3, API endpoints, server ports, logging
/// levels, and more. The configuration can be serialized to and from JSON for persistence or transfer.</remarks>
public sealed class Configuration {

    public static readonly JsonSerializerOptions JsonSerializerOptions = new() {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    [ConfigurationSection("API", "Settings related to the Battlegrounds API", developerModeOnly: true, priority: 100)]
    public sealed class APIConfiguration {
        private string _loginUrlOverride = string.Empty; // Allows overriding the login URL for testing or custom servers

        [ConfigurationProperty("Base URL", "The base URL for the Battlegrounds API. This is the root endpoint for all API requests. Ensure that the URL is correct and accessible from the application environment.")]
        public string BaseUrl { get; set; } = "https://api.test.cohbattlegrounds.com";

        [ConfigurationProperty("Login Endpoint", "The endpoint for user authentication. This URL is used to obtain access tokens for API requests. Ensure that the endpoint is correct and that the authentication service is operational.")]
        public string LoginEndpoint { get; set; } = "https://api.test.cohbattlegrounds.com/auth/test/login";

        public string LoginUrlOverride {
            get => string.IsNullOrEmpty(_loginUrlOverride) ? BaseUrl : _loginUrlOverride;
            set => _loginUrlOverride = value;
        }
        public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(15); // Default timeout for API requests
    }

    [ConfigurationSection("Company of Heroes 3", "Settings for Company of Heroes 3", priority: 10)]
    public sealed class CoH3Configuration {
        
        [ConfigurationProperty("Install Path", "The file system path where Company of Heroes 3 is installed. This path is used to locate the game executable and related files. Ensure that the path is correct and that the application has appropriate permissions to access it.", propertyType: ConfigurationPropertyType.DirectoryPath)]
        public string InstallPath { get; set; } = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Company of Heroes 3";

        [ConfigurationProperty("Mod Project Path", "The file system path to the Company of Heroes 3 mod project. This path is used for mod development and should point to the root directory of the mod project. Ensure that the path is correct and that the application has appropriate permissions to access it.", propertyType: ConfigurationPropertyType.FilePath, developerModeOnly: true)]
        public string ModProjectPath { get; set; } = "E:\\coh3-dev\\coh3-bg-wincondition\\bg_wincondition\\bg_wincondition.coh3mod";

        public string MatchDataPath { get; set; } = "E:\\coh3-dev\\coh3-bg-wincondition\\bg_wincondition\\assets\\scar\\winconditions\\match_data.scar";

        public string ModBuildPath { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "my games", "CoHBattlegrounds", "build", "coh3");

        [JsonIgnore]
        public bool HasInstallPath => !string.IsNullOrEmpty(InstallPath) && Directory.Exists(InstallPath);

    }

    [ConfigurationSection("Company of Heroes 2", "Settings for Company of Heroes 2", isVisible: false, priority: 20)]
    public sealed class CoH2Configuration {

        [ConfigurationProperty("Install Path", "The file system path where Company of Heroes 2 is installed. This path is used to locate the game executable and related files. Ensure that the path is correct and that the application has appropriate permissions to access it.")]
        public string InstallPath { get; set; } = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Company of Heroes 2";

        [JsonIgnore]
        public bool HasInstallPath => !string.IsNullOrEmpty(InstallPath) && Directory.Exists(InstallPath);

    }

    /// <summary>
    /// The last known position, size and maximised state of the main window.
    /// </summary>
    /// <remarks>Stored in device-independent pixels, matching WPF's <see cref="System.Windows.Window.Left"/>
    /// and friends. Values are only meaningful when <see cref="HasValue"/> is true.
    ///
    /// The coordinates are nullable rather than NaN-sentinelled: System.Text.Json refuses to write NaN
    /// unless JsonNumberHandling.AllowNamedFloatingPointLiterals is set, so a NaN default would throw
    /// while writing the default config on first run.</remarks>
    public sealed class WindowPlacementConfiguration {

        public double? Left { get; set; }
        public double? Top { get; set; }
        public double? Width { get; set; }
        public double? Height { get; set; }
        public bool Maximized { get; set; } = false;

        /// <summary>
        /// Gets whether a usable placement has been recorded. False on first run.
        /// </summary>
        [JsonIgnore]
        public bool HasValue =>
            Left is double left && !double.IsNaN(left) && !double.IsInfinity(left)
            && Top is double top && !double.IsNaN(top) && !double.IsInfinity(top)
            && Width is double width && !double.IsNaN(width) && !double.IsInfinity(width) && width > 0
            && Height is double height && !double.IsNaN(height) && !double.IsInfinity(height) && height > 0;

    }

    public sealed class LobbySetup(string gameId) {

        public sealed record Slot(bool IsLocal, string Faction, string CompanyId, AIDifficulty Difficulty, bool IsHidden, bool IsLocked);

        public LobbySetup() : this(Playing.CoH3.GameId) { } // Default constructor initializes with CoH3 game ID

        public Dictionary<string, int> Settings { get; set; } = new Dictionary<string, int>() {
            { LobbySetting.SETTING_GAMEMODE, 0 },
            // TODO: Other settings can be added here as needed
        };

        public string ScenarioId { get; set; } = gameId is Playing.CoH3.GameId ? "pachino_2p" : "2p_angoville"; // Default scenario ID based on game

        public TeamType Team1Type { get; set; } = TeamType.Allies; // Default team type for team 1
        public TeamType Team2Type { get; set; } = TeamType.Axis; // Default team type for team 2

        public Slot[] Team1 { get; set; } = [
            new Slot(true, gameId is Playing.CoH3.GameId ? "british_africa" : "soviet", string.Empty, AIDifficulty.HUMAN, false, false),
            new Slot(false, string.Empty, string.Empty, AIDifficulty.HUMAN, true, false),
            new Slot(false, string.Empty, string.Empty, AIDifficulty.HUMAN, true, false),
            new Slot(false, string.Empty, string.Empty, AIDifficulty.HUMAN, true, false)];

        public Slot[] Team2 { get; set; } = [
            new Slot(false, gameId is Playing.CoH3.GameId ? "afrika_korps" : "german", string.Empty, AIDifficulty.NORMAL, false, false),
            new Slot(false, string.Empty, string.Empty, AIDifficulty.HUMAN, true, false),
            new Slot(false, string.Empty, string.Empty, AIDifficulty.HUMAN, true, false),
            new Slot(false, string.Empty, string.Empty, AIDifficulty.HUMAN, true, false)];

    }

    /// <summary>
    /// Gets or sets the file path where company data is stored.
    /// </summary>
    public string CompaniesPath { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "my games", "CoHBattlegrounds", "companies");

    /// <summary>
    /// Gets or sets the file path where doctrine data is stored.
    /// </summary>
    public string DoctrinesPath { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "my games", "CoHBattlegrounds", "doctrines");

    /// <summary>
    /// Gets or sets the file system path to the application's documents directory.
    /// </summary>
    /// <remarks>The default value points to a subdirectory within the user's My Documents folder. This path
    /// is typically used to store user-generated files or application data that should persist between
    /// sessions.</remarks>
    public string DocumentsPath { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "my games", "CoHBattlegrounds");

    /// <summary>
    /// Gets or sets the file system path where game statistics are stored.
    /// </summary>
    /// <remarks>The default path is set to the user's Documents folder under "my
    /// games\CoHBattlegrounds\statistics". Ensure the application has appropriate permissions to read from and write to
    /// this location.</remarks>
    public string StatisticsPath { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "my games", "CoHBattlegrounds", "statistics");

    /// <summary>
    /// Gets or sets the file system path where log files are stored.
    /// </summary>
    /// <remarks>The default value is a directory named "logs" located under "my games\CoHBattlegrounds" in
    /// the user's Documents folder. Ensure the application has write permissions to this location when setting a custom
    /// path.</remarks>
    public string LogsPath { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "my games", "CoHBattlegrounds", "logs");

    /// <summary>
    /// Gets or sets the file system path where downloaded remote images are cached.
    /// </summary>
    /// <remarks>Unlike the other paths this one lives under <c>%AppData%</c> rather than Documents:
    /// the contents are disposable and re-downloadable, not user data. Deliberately carries no
    /// <see cref="ConfigurationPropertyAttribute"/> — it is not something the user edits in Settings.</remarks>
    public string ImageCachePath { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CoHBattlegrounds", "cache", "images");

    /// <summary>
    /// Gets or sets the configuration settings for Company of Heroes 2.
    /// </summary>
    [ConfigurationInclude()]
    public CoH2Configuration CoH2 { get; set; } = new CoH2Configuration(); // Configuration for Company of Heroes 2

    /// <summary>
    /// Gets or sets the configuration settings for Company of Heroes 3.
    /// </summary>
    [ConfigurationInclude()]
    public CoH3Configuration CoH3 { get; set; } = new CoH3Configuration(); // Configuration for Company of Heroes 3

    /// <summary>
    /// Gets or sets the host URL for the Battlegrounds server.
    /// </summary>
    [ConfigurationSection("Server", "Settings related to the Battlegrounds server", developerModeOnly: true, priority: 90)]
    [ConfigurationProperty("Battlegrounds Server Host", "The host URL for the Battlegrounds server. This is the address that the application will use to connect to the server for game-related operations. Ensure that the host is correct and that the server is operational and accessible from the application environment.", developerModeOnly: true)]
    public string BattlegroundsServerHost { get; set; } = "bg.test.service.cohbg.com";

    /// <summary>
    /// Gets or sets the port number used by the Battlegrounds HTTP server.
    /// </summary>
    /// <remarks>Ensure that the specified port is not already in use by another application and is 
    /// accessible through the network firewall, if applicable.</remarks>
    [ConfigurationSection("Server", "Settings related to the Battlegrounds server", developerModeOnly: true, priority: 90)]
    [ConfigurationProperty("Battlegrounds HTTP Server Port", "The port number used by the Battlegrounds HTTP server. Ensure that the specified port is not already in use by another application and is accessible through the network firewall, if applicable.", developerModeOnly: true)]
    public int BattlegroundsHttpServerPort { get; set; } = 443;

    /// <summary>
    /// Gets or sets the port number used by the Battlegrounds gRPC server.
    /// </summary>
    [ConfigurationSection("Server", "Settings related to the Battlegrounds server", developerModeOnly: true, priority: 90)]
    [ConfigurationProperty("Battlegrounds gRPC Server Port", "The port number used by the Battlegrounds gRPC server. Ensure that the specified port is not already in use by another application and is accessible through the network firewall, if applicable.", developerModeOnly: true)]
    public int BattlegroundsGrpcServerPort { get; set; } = 8082;

    /// <summary>
    /// Gets or sets the display language for the application.
    /// </summary>
    [ConfigurationSection("General", "General application settings", priority: 0)]
    [ConfigurationProperty("Language", "The display language for the application.", propertyType: ConfigurationPropertyType.Selection, Options = ["English", "Spanish", "French", "German", "Polish"])]
    public string Language { get; set; } = "English";

    /// <summary>
    /// Gets or sets a value indicating whether companies are automatically synchronized with the server.
    /// </summary>
    /// <summary>
    /// Gets or sets the interface scale, as a percentage string.
    /// </summary>
    /// <remarks>Applied by <see cref="Services.IUiScaleService"/>, which swaps the design system's size
    /// tokens rather than transforming the visual tree. Keep the options in step with
    /// <c>UiScaleService.AvailableScales</c>.</remarks>
    [ConfigurationSection("General", "General application settings", priority: 0)]
    [ConfigurationProperty("UI Scale", "Scales text, controls and spacing throughout the app. Increase this if the interface looks small on a high-resolution or large display.", propertyType: ConfigurationPropertyType.Selection, Options = ["100%", "110%", "125%", "150%"])]
    public string UiScale { get; set; } = "100%";

    [ConfigurationSection("General", "General application settings", priority: 0)]
    [ConfigurationProperty("Auto Sync Companies", "Indicates whether companies should be automatically synchronized with the server.", propertyType: ConfigurationPropertyType.Boolean)]
    public bool AutoSyncCompanies { get; set; } = true; // Should companies be automatically synced with the server?

    /// <summary>
    /// Gets or sets a value indicating whether wincondition source files are automatically synchronized with the
    /// server.
    /// </summary>
    [ConfigurationSection("General", "General application settings", priority: 0)]
    [ConfigurationProperty("Auto Sync Wincondition Source Files", "Indicates whether wincondition source files should be automatically synchronized with the server.", propertyType: ConfigurationPropertyType.Boolean)]
    public bool AutoSyncWinconditionSourceFiles { get; set; } = true; // Should wincondition source files be automatically synced with the server?

    /// <summary>
    /// Gets or sets a value indicating whether telemetry data is sent to the server.
    /// </summary>
    [ConfigurationSection("General", "General application settings", priority: 0)]
    [ConfigurationProperty("Enable Telemetry", "Indicates whether telemetry should be sent to the server.", propertyType: ConfigurationPropertyType.Boolean)]
    public bool EnableTelemetry { get; set; } = false; // Should telemetry be sent to the server?

    /// <summary>
    /// Gets or sets the URL of the GitHub repository associated with the project.
    /// </summary>
    /// <remarks>This property can be used to provide users with a reference to the project's source code and
    /// issue tracker. It is intended for informational purposes and may be used in developer mode or for support
    /// scenarios.</remarks>
    [ConfigurationSection("General", "General application settings", priority: 0)]
    [ConfigurationProperty("GitHub Repository", "The URL of the GitHub repository for the project. This can be used for reference or to direct users to the source code and issue tracker.", propertyType: ConfigurationPropertyType.String, developerModeOnly: true)]
    public string GithubRepository { get; set; } = "https://github.com/BattlegroundsCoH/coh-battlegrounds";

    /// <summary>
    /// Gets or sets the base URL of the public Battlegrounds website.
    /// </summary>
    /// <remarks>The client shows news previews only; clicking one hands off to
    /// <c>{WebsiteUrl}/news/{slug}</c> in the user's browser for the article itself. This is
    /// deliberately independent of <see cref="APIConfiguration.BaseUrl"/>, which defaults to the
    /// test environment — a slug that only exists on the test API will 404 on the live site.</remarks>
    [ConfigurationSection("General", "General application settings", priority: 0)]
    [ConfigurationProperty("Website URL", "The base URL of the public Battlegrounds website. Used to open news articles in the browser.", propertyType: ConfigurationPropertyType.String, developerModeOnly: true)]
    public string WebsiteUrl { get; set; } = "https://cohbattlegrounds.com";

    /// <summary>
    /// Gets or sets the last known placement of the main window.
    /// </summary>
    /// <remarks>Deliberately carries no <see cref="ConfigurationPropertyAttribute"/>: it is persisted state,
    /// not something the user edits in Settings. <see cref="WindowPlacement.HasValue"/> is false on first run,
    /// in which case the window falls back to its default size.</remarks>
    public WindowPlacementConfiguration WindowPlacement { get; set; } = new WindowPlacementConfiguration();

    /// <summary>
    /// Gets or sets the logging level for the application.
    /// </summary>
    public string LogLevel { get; set; } =
        #if DEBUG 
            "trace"; // Default log level testing
        #else
            "info"; // Default log level for production
        #endif

    /// <summary>
    /// Gets or sets the configuration settings for the Battlegrounds API.
    /// </summary>
    public APIConfiguration API { get; set; } = new APIConfiguration(); // Configuration for the Battlegrounds API

    /// <summary>
    /// Gets or sets the collection of lobby setups, indexed by game ID.
    /// </summary>
    /// <remarks>The dictionary is pre-populated with a default lobby setup for Company of Heroes 3,
    /// identified by its game ID. Additional lobby setups can be added or modified as needed.</remarks>
    public Dictionary<string, LobbySetup> LobbySetups { get; set; } = new Dictionary<string, LobbySetup>() {
        { Playing.CoH3.GameId, new LobbySetup(Playing.CoH3.GameId) } // Default lobby setup for Company of Heroes 3
    };

    public string? ToJson() => JsonSerializer.Serialize(this, JsonSerializerOptions);

    /// <summary>
    /// Retrieves the latest lobby settings for the specified game.
    /// </summary>
    /// <param name="gameId">The unique identifier of the game for which to retrieve lobby settings. Cannot be null or empty.</param>
    /// <returns>A dictionary containing the key-value pairs of the lobby settings for the specified game.  Returns an empty
    /// dictionary if no settings are found for the given <paramref name="gameId"/>.</returns>
    public Dictionary<string, int> GetLatestLobbySettings(string gameId) {
        if (LobbySetups.TryGetValue(gameId, out var setup)) {
            return setup.Settings;
        }
        return [];
    }

    /// <summary>
    /// Retrieves the type of the specified team for a given game.
    /// </summary>
    /// <param name="teamIdx">The index of the team. Must be 1 or 2.</param>
    /// <param name="gameId">The unique identifier of the game.</param>
    /// <returns>The <see cref="TeamType"/> of the specified team. If the game is found in the lobby setup, the team type is
    /// determined by the setup. Otherwise, default team types are returned: <see cref="TeamType.Allies"/> for team 1
    /// and <see cref="TeamType.Axis"/> for team 2.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="teamIdx"/> is not 1 or 2.</exception>
    public TeamType GetTeamType(int teamIdx, string gameId) {
        if (LobbySetups.TryGetValue(gameId, out var setup)) {
            return teamIdx switch {
                1 => setup.Team1Type,
                2 => setup.Team2Type,
                _ => throw new ArgumentOutOfRangeException(nameof(teamIdx), "Invalid team index. Must be 1 or 2.")
            };
        }
        return teamIdx switch {
            1 => TeamType.Allies,
            2 => TeamType.Axis,
            _ => throw new ArgumentOutOfRangeException(nameof(teamIdx), "Invalid team index. Must be 1 or 2.")
        };
    }

    /// <summary>
    /// Retrieves a specific team slot from the lobby setup for the given game.
    /// </summary>
    /// <param name="teamIdx">The index of the team to retrieve the slot from. Must be <see langword="1"/> for Team 1 or <see langword="2"/>
    /// for Team 2.</param>
    /// <param name="i">The index of the slot within the specified team.</param>
    /// <param name="gameId">The unique identifier of the game whose lobby setup is being queried.</param>
    /// <returns>The <see cref="LobbySetup.Slot"/> object representing the specified team slot. If no lobby setup is found for
    /// the given <paramref name="gameId"/>, a default slot is returned with no player assigned.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="teamIdx"/> is not <see langword="1"/> or <see langword="2"/>.</exception>
    public LobbySetup.Slot GetTeamSlot(int teamIdx, int i, string gameId) {
        if (LobbySetups.TryGetValue(gameId, out var setup)) {
            return teamIdx switch {
                1 => setup.Team1[i],
                2 => setup.Team2[i],
                _ => throw new ArgumentOutOfRangeException(nameof(teamIdx), "Invalid team index. Must be 1 or 2.")
            };
        }
        return new LobbySetup.Slot(false, string.Empty, string.Empty, AIDifficulty.HUMAN, false, false); // Default slot if no setup found
    }

    public static Configuration? FromJson(FileStream stream) => JsonSerializer.Deserialize<Configuration>(stream, JsonSerializerOptions);

}
