using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;

using Battlegrounds.Game.DataCompany;
using Battlegrounds.Game.DataSource;
using Battlegrounds.Game.Gameplay;

namespace Battlegrounds.Modding {

    /// <summary>
    /// Class representing a package of mods working together as an overhaul experience.
    /// </summary>
    [JsonConverter(typeof(ModPackageLoader))]
    public class ModPackage {

        public readonly struct FactionData {

            [JsonConverter(typeof(ModFactionAbilityLoader))]
            public class FactionAbility { // Class because too many variables for it to make sense to being a struct

                public readonly struct VeterancyRequirement {
                    public bool RequireVeterancy { get; }
                    public float Veterancy { get; }
                    public VeterancyRequirement(bool RequireVeterancy, float Veterancy) {
                        this.RequireVeterancy = RequireVeterancy;
                        this.Veterancy = Veterancy;
                    }
                }

                public readonly struct AbilityVeterancy {
                    public string ScreenName { get; }
                    public float Experience { get; }
                    public Modifier[] Modifiers { get; }
                    public AbilityVeterancy(string ScreenName, float Experience, Modifier[] Modifiers) {
                        this.ScreenName = ScreenName;
                        this.Experience = Experience;
                        this.Modifiers = Modifiers;
                    }
                }

                /// <summary>
                /// Get the blueprint name of the ability.
                /// </summary>
                public string Blueprint { get; }

                /// <summary>
                /// Get the ability category
                /// </summary>
                public SpecialAbilityCategory AbilityCategory { get; }

                /// <summary>
                /// Get the max use in a match (-1 = infinite)
                /// </summary>
                [DefaultValue(0)]
                public int MaxUsePerMatch { get; }

                /// <summary>
                /// Get if requires granting units to be off-map
                /// </summary>
                [DefaultValue(false)]
                public bool RequireOffmap { get; }

                /// <summary>
                /// Get the effectiveness multiplier when multiple units are off-map
                /// </summary>
                [DefaultValue(0.0f)]
                public float OffmapCountEffectivenesss { get; }

                /// <summary>
                /// Get if the ability grants veterancy.
                /// </summary>
                [DefaultValue(false)]
                public bool CanGrantVeterancy { get; }

                /// <summary>
                /// Get the ranks associated with this ability. Empty list means no special abilities.
                /// </summary>
                [DefaultValue(null)]
                public AbilityVeterancy[] VeterancyRanks { get; }

                /// <summary>
                /// Get the veterancy requirement on units before this ability can be used.
                /// </summary>
                [DefaultValue(null)]
                public VeterancyRequirement? VeterancyUsageRequirement { get; }

                /// <summary>
                /// Get the amount of experience granted after each use.
                /// </summary>
                [DefaultValue(0.0f)]
                public float VeterancyExperienceGain { get; }

                public FactionAbility(string Blueprint, SpecialAbilityCategory AbilityCategory, int MaxUsePerMatch, bool RequireOffmap, float OffmapCountEffectivenesss,
                    bool CanGrantVeterancy, AbilityVeterancy[] VeterancyRanks, VeterancyRequirement? VeterancyUsageRequirement,
                    float VeterancyExperienceGain) {
                    this.Blueprint = Blueprint;
                    this.AbilityCategory = AbilityCategory;
                    this.MaxUsePerMatch = MaxUsePerMatch;
                    this.RequireOffmap = RequireOffmap;
                    this.OffmapCountEffectivenesss = OffmapCountEffectivenesss;
                    this.CanGrantVeterancy = CanGrantVeterancy;
                    this.VeterancyRanks = VeterancyRanks;
                    this.VeterancyUsageRequirement = VeterancyUsageRequirement;
                    this.VeterancyExperienceGain = VeterancyExperienceGain;
                }

            }

            public readonly struct UnitAbility {

                public string Blueprint { get; }

                public FactionAbility[] Abilities { get; }

                [JsonConstructor]
                public UnitAbility(string Blueprint, FactionAbility[] Abilities) {
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

            public FactionAbility[] Abilities { get; }

            public UnitAbility[] UnitAbilities { get; }

            public string[] Transports { get; }

            public string[] TowTransports { get; }

            public bool CanHaveParadropInCompanies { get; }

            public bool CanHaveGliderInCompanies { get; }

            [JsonConstructor]
            public FactionData(string Faction, Driver[] Drivers, FactionAbility[] Abilities, UnitAbility[] UnitAbilities, string[] Transports, string[] TowTransports, bool CanHaveParadropInCompanies, bool CanHaveGliderInCompanies) {
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
