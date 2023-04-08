using System;
using System.Collections.Generic;

using Battlegrounds.Game;
using Battlegrounds.Game.Blueprints;
using Battlegrounds.Game.Database.Management;
using Battlegrounds.Game.DataSource;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Locale;
using Battlegrounds.Modding.Content;
using Battlegrounds.Modding.Content.Companies;

namespace Battlegrounds.Modding.Vanilla;

public class VanillaModPackage : IModPackage {

    private IModDb? modDataSource;

    public string ID { get; init; }

    public string PackageName { get; init; }

    public ModGuid TuningGUID { get; init; }

    public ModGuid GamemodeGUID { get; init; }

    public ModGuid AssetGUID { get; init; }

    public ModLocale[] LocaleFiles { get; init; }

    public string VerificationUpgrade { get; init; }

    public bool IsTowEnabled { get; init; }

    public string IsTowedUpgrade { get; init; }

    public string IsTowingUpgrade { get; init; }

    public string[] ParadropUnits { get; init; }

    public bool AllowSupplySystem { get; init; }

    public bool AllowWeatherSystem { get; init; }

    public string DataSourcePath => BattlegroundsContext.GetRelativePath(BattlegroundsPaths.DATABASE_FOLDER);

    public Dictionary<Faction, FactionData> FactionSettings { get; init; }

    public Gamemode[] Gamemodes { get; init; }

    public Dictionary<string, Dictionary<string, string>> TeamWeaponCaptureSquads { get; init; }

    public GameCase SupportedGames { get; init; }

    public SquadBlueprint GetCaptureSquad(EntityBlueprint ebp, Faction faction) {
        throw new NotImplementedException();
    }

    public HashSet<string> GetCaptureSquads() {
        throw new NotImplementedException();
    }

    public FactionCompanyType? GetCompanyType(string id) {
        throw new NotImplementedException();
    }

    public FactionCompanyType? GetCompanyType(Faction faction, string id) {
        throw new NotImplementedException();
    }

    public UcsFile? GetLocale(ModType modType, string language) {
        throw new NotImplementedException();
    }

    public UcsFile? GetLocale(ModType modType, LocaleLanguage language) {
        throw new NotImplementedException();
    }

    public void SetDataSource(IModDb dataSrc)
        => modDataSource = dataSrc;

    public IModDb GetDataSource()
        => modDataSource ?? throw new InvalidOperationException();

}
