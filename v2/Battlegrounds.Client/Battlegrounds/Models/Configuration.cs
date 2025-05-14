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

}
