using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;

using Battlegrounds.ErrorHandling.CommonExceptions;
using Battlegrounds.Functional;
using Battlegrounds.Game.Database;
using Battlegrounds.Game.Database.Management;
using Battlegrounds.Game.DataSource;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Locale;
using Battlegrounds.Modding.Content;
using Battlegrounds.Modding.Content.Companies;
using Battlegrounds.Modding.Loaders;

namespace Battlegrounds.Modding;

/// <summary>
/// Class representing a package of mods working together as an overhaul experience.
/// </summary>
[JsonConverter(typeof(ModPackageLoader))]
public class ModPackage {

    public readonly struct CustomOptions {

    }

    public readonly struct ModLocale {

        [JsonIgnore]
        public ModType ModType => Enum.Parse<ModType>(this.Type);
        public string Type { get; }
        public string Path { get; }
        [JsonConstructor]
        public ModLocale(string Type, string Path) {
            this.Type = Type;
            this.Path = Path;
        }

        public UcsFile? GetLocale(string modID, string language) {
            string locFile = this.Path.Replace("%LANG%", language);
            try {
                string locpath = BattlegroundsInstance.GetRelativePath(BattlegroundsPaths.BINARY_FOLDER, locFile);
                string locpath2 = BattlegroundsInstance.GetRelativePath(BattlegroundsPaths.MOD_USER_FOLDER, locFile);
                if (File.Exists(locpath)) {
                    return UcsFile.LoadFromFile(locpath);
                } else if (File.Exists(locpath2)) {
                    return UcsFile.LoadFromFile(locpath2);
                } else if (locpath != BattlegroundsInstance.GetRelativePath(BattlegroundsPaths.MOD_USER_FOLDER)) {
                    Trace.WriteLine($"Failed to locate ucs file '{locpath}'", nameof(ModPackage));
                }
            } catch (Exception locex) {
                Trace.WriteLine($"Failed to load ucs file for mod package '{modID}' ({locex.Message})", nameof(ModPackage));
            }
            return null;
        }

    }

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

    public Dictionary<Faction, FactionData> FactionSettings { get; init; }

    public Gamemode[] Gamemodes { get; init; }

    public Dictionary<string, Dictionary<string, string>> TeamWeaponCaptureSquads { get; init; }

    public ModPackage() {
        this.ID = "$invalid";
        this.PackageName = "Invalid Mod Package";
        this.Gamemodes = Array.Empty<Gamemode>();
        this.FactionSettings = new();
        this.ParadropUnits = Array.Empty<string>();
        this.LocaleFiles = Array.Empty<ModLocale>();
        this.TeamWeaponCaptureSquads = new();
        this.VerificationUpgrade = this.IsTowedUpgrade = this.IsTowingUpgrade = string.Empty;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="modType"></param>
    /// <param name="language"></param>
    /// <returns></returns>
    public UcsFile? GetLocale(ModType modType, string language)
        => this.LocaleFiles.FirstOrDefault(x => x.ModType == modType) is ModLocale loc ? loc.GetLocale(this.ID, language) : null;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="modType"></param>
    /// <param name="language"></param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    public UcsFile? GetLocale(ModType modType, LocaleLanguage language)
        => this.GetLocale(modType, language switch {
            LocaleLanguage.Default => "english",
            LocaleLanguage.German => "german",
            LocaleLanguage.French => "french",
            LocaleLanguage.Spanish => "spanish",
            //LocaleLanguage.Russian => "russian",
            _ => throw new NotSupportedException()
        });

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public FactionCompanyType? GetCompanyType(string id)
        => this.FactionSettings.Values.SelectMany(x => x.Companies.Types).FirstOrDefault(x => x.Id == id);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="faction"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    public FactionCompanyType? GetCompanyType(Faction faction, string id) => this.FactionSettings[faction].Companies.Types?.FirstOrDefault(x => x.Id == id) ?? null;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="ebp"></param>
    /// <param name="faction"></param>
    /// <returns></returns>
    /// <exception cref="ObjectNotFoundException"></exception>
    public SquadBlueprint GetCaptureSquad(EntityBlueprint ebp, Faction faction) {
        var ebpstr = ebp.GetScarName();
        foreach (var (k,v) in this.TeamWeaponCaptureSquads) {
            if (k == ebpstr) {
                return BlueprintManager.FromBlueprintName<SquadBlueprint>(v[faction.Name]);
            }
        }
        throw new ObjectNotFoundException($"Failed to find capture squad blueprint for team weapon '{ebp}' for faction '{faction}'.");
    }

    /// <summary>
    /// Get a list of all capture squads defiend by the package.
    /// </summary>
    /// <remarks>
    /// Internally filters based on blueprint name containing '_capture_'
    /// </remarks>
    /// <returns>Hashset containing all blueprint names of capture squads.</returns>
    public HashSet<string> GetCaptureSquads() {
        HashSet<string> result = new HashSet<string>();
        foreach (var (_, v) in this.TeamWeaponCaptureSquads) {
            v.Values.ForEach(x => {
                if (x.Contains("_capture_"))
                        result.Add(x);
            });
        }
        return result;
    }

}
