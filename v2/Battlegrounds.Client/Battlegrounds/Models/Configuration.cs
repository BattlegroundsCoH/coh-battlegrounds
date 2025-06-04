namespace Battlegrounds.Models;

public sealed class Configuration {

    private const bool _isLocalBuild = false; // Set to false for production builds

    public sealed class APIConfiguration {
        private string _loginUrlOverride =
#if DEBUG
            _isLocalBuild ? "http://localhost:8087" : "http://bg.test.service.cohbattlegrounds.com:8087";
            #else
            string.Empty;
#endif
        public string BaseUrl { get; set; } = "https://api.cohbattlegrounds.com";
        public string LoginEndpoint { get; set; } = "/login";
        public string LoginUrlOverride {
            get => string.IsNullOrEmpty(_loginUrlOverride) ? BaseUrl : _loginUrlOverride;
            set => _loginUrlOverride = value;
        }

    }

    public string CompanyOfHeroes3InstallPath { get; set; } = string.Empty;

    public bool HasCompanyOfHeroes3InstallPath => !string.IsNullOrEmpty(CompanyOfHeroes3InstallPath);

    public string CompanyOfHeroes2InstallPath { get; set; } = string.Empty;

    public bool HasCompanyOfHeroes2InstallPath => !string.IsNullOrEmpty(CompanyOfHeroes2InstallPath);

    public string BattlegroundsServerHost { get; set; } = string.Empty;

    public int BattlegroundsServerPort { get; set; } = 0;

    public string BattlegroundsAPIServerHost { get; set; } = string.Empty;

    public int BattlegroundsAPIServerPort { get; set; } = 0;

    public bool SkipMovies { get; set; } = false; // Should '-nomovies' be passed to the game?

    public bool WindowedMode { get; set; } = false; // Should the '-windowed' flag be passed to the game?

    public bool GameDevMode { get; set; } = false; // Should the '-dev' flag be passed to the game?

    public bool GameDebugMode { get; set; } = false; // Should the '-debug' flag be passed to the game?

    public APIConfiguration API { get; set; } = new APIConfiguration(); // Configuration for the Battlegrounds API

}
