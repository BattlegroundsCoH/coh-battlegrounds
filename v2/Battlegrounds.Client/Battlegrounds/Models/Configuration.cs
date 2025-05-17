namespace Battlegrounds.Models;

public sealed class Configuration {

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

}
