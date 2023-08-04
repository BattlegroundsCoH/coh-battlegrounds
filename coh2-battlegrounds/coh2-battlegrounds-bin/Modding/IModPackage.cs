using System.Collections.Generic;

using Battlegrounds.Game;
using Battlegrounds.Game.Blueprints;
using Battlegrounds.Game.Database.Management;
using Battlegrounds.Game.DataSource;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Locale;
using Battlegrounds.Modding.Content;
using Battlegrounds.Modding.Content.Companies;

namespace Battlegrounds.Modding;

public interface IModPackage {

    string ID { get; }

    string PackageName { get; }

    ModGuid TuningGUID { get; }

    ModGuid GamemodeGUID { get; }

    ModGuid AssetGUID { get; }

    ModLocale[] LocaleFiles { get; }

    string VerificationUpgrade { get; }

    bool IsTowEnabled { get; }

    string IsTowedUpgrade { get; }

    string IsTowingUpgrade { get; }

    string[] ParadropUnits { get; }

    bool AllowSupplySystem { get; }

    bool AllowWeatherSystem { get; }

    string DataSourcePath { get; }

    Dictionary<Faction, FactionData> FactionSettings { get; }

    Gamemode[] Gamemodes { get; }

    Dictionary<string, Dictionary<string, string>> TeamWeaponCaptureSquads { get; }

    GameCase SupportedGames { get; }

    UcsFile? GetLocale(ModType modType, string language);

    UcsFile? GetLocale(ModType modType, LocaleLanguage language);

    FactionCompanyType? GetCompanyType(string id);

    FactionCompanyType? GetCompanyType(Faction faction, string id);

    SquadBlueprint GetCaptureSquad(EntityBlueprint ebp, Faction faction);

    HashSet<string> GetCaptureSquads();

    void SetDataSource(IModDb dataSrc);

    IModDb GetDataSource();

}
