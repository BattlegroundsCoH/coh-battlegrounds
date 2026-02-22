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
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public sealed class APIConfiguration {
        private string _loginUrlOverride = string.Empty; // Allows overriding the login URL for testing or custom servers
        public string BaseUrl { get; set; } = "https://api.test.cohbattlegrounds.com";
        public string LoginEndpoint { get; set; } = "https://bjcgardajdviqkwgryin.supabase.co/auth/v1/token?grant_type=password";
        public string RefreshEndpoint { get; set; } = "/refresh";
        public string PublicKeyEndpoint { get; set; } = "/publickey";
        public string AuthStatusEndpoint { get; set; } = "/auth/<IdP>/status"; // Endpoint to check authentication status
        public string AuthStartEndpoint { get; set; } = "/auth/<IdP>/start"; // Endpoint to start authentication with a provider
        public string LoginUrlOverride {
            get => string.IsNullOrEmpty(_loginUrlOverride) ? BaseUrl : _loginUrlOverride;
            set => _loginUrlOverride = value;
        }
        public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(15); // Default timeout for API requests
        public bool IsHS256 { get; set; } = true; // Use HS256 for JWT tokens
    }

    public sealed class CoH3Configuration {

        public string InstallPath { get; set; } = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Company of Heroes 3";

        public string ModProjectPath { get; set; } = "E:\\coh3-dev\\coh3-bg-wincondition\\bg_wincondition\\bg_wincondition.coh3mod";

        public string MatchDataPath { get; set; } = "E:\\coh3-dev\\coh3-bg-wincondition\\bg_wincondition\\assets\\scar\\winconditions\\match_data.scar";

        public string ModBuildPath { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "my games", "CoHBattlegrounds", "build", "coh3");

        [JsonIgnore]
        public bool HasInstallPath => !string.IsNullOrEmpty(InstallPath) && Directory.Exists(InstallPath);

    }

    public sealed class CoH2Configuration {

        public string InstallPath { get; set; } = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Company of Heroes 2";

        [JsonIgnore]
        public bool HasInstallPath => !string.IsNullOrEmpty(InstallPath) && Directory.Exists(InstallPath);

    }

    public sealed class LobbySetup(string gameId) {

        public sealed record Slot(bool IsLocal, string Faction, string CompanyId, AIDifficulty Difficulty, bool IsHidden, bool IsLocked);

        public LobbySetup() : this(Playing.CoH3.GameId) { } // Default constructor initializes with CoH3 game ID

        public Dictionary<string, int> Settings { get; set; } = new Dictionary<string, int>() {
            { LobbySetting.SETTING_GAMEMODE, 0 },
            // TODO: Other settings can be added here as needed
        };

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
    /// Gets or sets the configuration settings for Company of Heroes 2.
    /// </summary>
    public CoH2Configuration CoH2 { get; set; } = new CoH2Configuration(); // Configuration for Company of Heroes 2

    /// <summary>
    /// Gets or sets the configuration settings for Company of Heroes 3.
    /// </summary>
    public CoH3Configuration CoH3 { get; set; } = new CoH3Configuration(); // Configuration for Company of Heroes 3

    /// <summary>
    /// Gets or sets the host URL for the Battlegrounds server.
    /// </summary>
    public string BattlegroundsServerHost { get; set; } = "bg.test.service.cohbattlegrounds.com";

    /// <summary>
    /// Gets or sets the port number used by the Battlegrounds HTTP server.
    /// </summary>
    /// <remarks>Ensure that the specified port is not already in use by another application and is 
    /// accessible through the network firewall, if applicable.</remarks>
    public int BattlegroundsHttpServerPort { get; set; } = 443;

    /// <summary>
    /// Gets or sets the port number used by the Battlegrounds gRPC server.
    /// </summary>
    public int BattlegroundsGrpcServerPort { get; set; } = 8082;

    /// <summary>
    /// Gets or sets a value indicating whether movies should be skipped in the game.
    /// </summary>
    public bool SkipMovies { get; set; } = false; // Should '-nomovies' be passed to the game?

    /// <summary>
    /// Gets or sets a value indicating whether the game should run in windowed mode.
    /// </summary>
    public bool WindowedMode { get; set; } = false; // Should the '-windowed' flag be passed to the game?

    /// <summary>
    /// Gets or sets a value indicating whether the game should run in developer mode.
    /// </summary>
    public bool GameDevMode { get; set; } = false; // Should the '-dev' flag be passed to the game?

    /// <summary>
    /// Gets or sets a value indicating whether the game should run in debug mode.
    /// </summary>
    public bool GameDebugMode { get; set; } = false; // Should the '-debug' flag be passed to the game?

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
