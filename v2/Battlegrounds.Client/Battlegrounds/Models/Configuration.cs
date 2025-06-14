using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Battlegrounds.Models;

public sealed class Configuration {

    public static readonly JsonSerializerOptions JsonSerializerOptions = new() {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public sealed class APIConfiguration {
        private string _loginUrlOverride =
#if DEBUG
            "http://bg.test.service.cohbattlegrounds.com:8087";
            #else
            string.Empty;
#endif
        public string BaseUrl { get; set; } = "https://api.cohbattlegrounds.com";
        public string LoginEndpoint { get; set; } = "/login";
        public string RefreshEndpoint { get; set; } = "/refresh";
        public string PublicKeyEndpoint { get; set; } = "/publickey";
        public string LoginUrlOverride {
            get => string.IsNullOrEmpty(_loginUrlOverride) ? BaseUrl : _loginUrlOverride;
            set => _loginUrlOverride = value;
        }
        public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(15); // Default timeout for API requests
    }

    public sealed class CoH3Configuration {

        public string InstallPath { get; set; } = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Company of Heroes 3";

        public string ModProjectPath { get; set; } = "E:\\coh3-dev\\coh3-bg-wincondition\\bg_wincondition\\bg_wincondition.coh3mod";

        public string MatchDataPath { get; set; } = "E:\\coh3-dev\\coh3-bg-wincondition\\bg_wincondition\\assets\\scar\\winconditions\\match_data.scar";

        public string ModBuildPath { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "my games", "CoHBattlegrounds", "build", "coh3");

        [JsonIgnore]
        public bool HasInstallPath => !string.IsNullOrEmpty(InstallPath) && Directory.Exists(InstallPath);

    }

    public string CompaniesPath { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "my games", "CoHBattlegrounds", "companies");

    public CoH3Configuration CoH3 { get; set; } = new CoH3Configuration(); // Configuration for Company of Heroes 3

    public string CompanyOfHeroes2InstallPath { get; set; } = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Company of Heroes 2";

    [JsonIgnore]
    public bool HasCompanyOfHeroes2InstallPath => !string.IsNullOrEmpty(CompanyOfHeroes2InstallPath) && Directory.Exists(CompanyOfHeroes2InstallPath);

    public string BattlegroundsServerHost { get; set; } = "https://bg.prod.service.cohbattlegrounds.com";

    public int BattlegroundsHttpServerPort { get; set; } = 11443;

    public int BattlegroundsGrpcServerPort { get; set; } = 11007;

    public bool SkipMovies { get; set; } = false; // Should '-nomovies' be passed to the game?

    public bool WindowedMode { get; set; } = false; // Should the '-windowed' flag be passed to the game?

    public bool GameDevMode { get; set; } = false; // Should the '-dev' flag be passed to the game?

    public bool GameDebugMode { get; set; } = false; // Should the '-debug' flag be passed to the game?

    public string LogLevel { get; set; } = "info"; // Default log level for the application

    public APIConfiguration API { get; set; } = new APIConfiguration(); // Configuration for the Battlegrounds API

    public string? ToJson() => JsonSerializer.Serialize(this, JsonSerializerOptions);

    public static Configuration? FromJson(FileStream stream) => JsonSerializer.Deserialize<Configuration>(stream, JsonSerializerOptions);

}
