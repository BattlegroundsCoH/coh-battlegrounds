using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;

using Battlegrounds.Game.DataSource;
using Battlegrounds.Game.Gameplay;

namespace Battlegrounds.Modding {

    /// <summary>
    /// Class representing a package of mods working together as an overhaul experience.
    /// </summary>
    [JsonConverter(typeof(ModPackageLoader))]
    public class ModPackage {

        public readonly struct FactionData {

            public readonly struct UnitAbility {

                public string Blueprint { get; }

                public string[] Abilities { get; }

                [JsonConstructor]
                public UnitAbility(string Blueprint, string[] Abilities) { 
                    this.Blueprint = Blueprint;
                    this.Abilities = Abilities;
                }

            }

            public readonly struct Driver {

                public string Blueprint { get; }
                public string WhenType { get; }

                [JsonConstructor]
                public Driver(string Blueprint, string WhenType) {
                    this.Blueprint = Blueprint;
                    this.WhenType = WhenType;
                }

            }

            public string Faction { get; }

            public Driver[] Drivers { get; }

            public string[] Abilities { get; }

            public UnitAbility[] UnitAbilities { get; }

            public string[] Transports { get; }

            public string[] TowTransports { get; }

            public bool CanHaveParadropInCompanies { get; }

            public bool CanHaveGliderInCompanies { get; }

            [JsonConstructor]
            public FactionData(string Faction, Driver[] Drivers, string[] Abilities, UnitAbility[] UnitAbilities, string[] Transports, string[] TowTransports, bool CanHaveParadropInCompanies, bool CanHaveGliderInCompanies) {
                this.Faction = Faction;
                this.Drivers = Drivers;
                this.Abilities = Abilities;
                this.UnitAbilities = UnitAbilities;
                this.Transports = Transports;
                this.TowTransports = TowTransports;
                this.CanHaveGliderInCompanies = CanHaveGliderInCompanies;
                this.CanHaveParadropInCompanies = CanHaveParadropInCompanies;
            }

        }

        public readonly struct Gamemode {

            public readonly struct GamemodeOption {
                public string LocStr { get; }
                public int Value { get; }

                [JsonConstructor]
                public GamemodeOption(string LocStr, int Value) {
                    this.LocStr = LocStr;
                    this.Value = Value;
                }
            }

            public string ID { get; }

            public string Display { get; }

            public string DisplayDesc { get; }

            public int DefaultOption { get; }

            public string[] Files { get; }

            public GamemodeOption[] Options { get; }

            [JsonConstructor]
            public Gamemode(string ID, string Display, string DisplayDesc, int DefaultOption, string[] Files, GamemodeOption[] Options) {
                this.ID = ID;
                this.Display = Display;
                this.DisplayDesc = DisplayDesc;
                this.DefaultOption = DefaultOption;
                this.Files = Files;
                this.Options = Options;
            }

        }

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

            public UcsFile GetLocale(string modID, string language) {
                string locFile = this.Path.Replace("%LANG%", language);
                try {
                    string locpath = BattlegroundsInstance.GetRelativePath(BattlegroundsPaths.MOD_OTHER_FOLDER, locFile);
                    if (File.Exists(locpath)) {
                        return UcsFile.LoadFromFile(locpath);
                    } else if (locpath != BattlegroundsInstance.GetRelativePath(BattlegroundsPaths.MOD_OTHER_FOLDER)) {
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

        public UcsFile GetLocale(ModType modType, string language) 
            => this.LocaleFiles.FirstOrDefault(x => x.ModType == modType) is ModLocale loc ? loc.GetLocale(this.ID, language) : null;

    }

}
