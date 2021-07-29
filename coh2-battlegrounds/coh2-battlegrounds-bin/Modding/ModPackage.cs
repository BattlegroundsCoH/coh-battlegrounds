using System.Collections.Generic;
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

            public string[] Transports { get; }

            public string[] TowTransports { get; }

            public bool CanHaveParadropInCompanies { get; }

            public bool CanHaveGliderInCompanies { get; }

            [JsonConstructor]
            public FactionData(string Faction, Driver[] Drivers, string[] Transports, string[] TowTransports, bool CanHaveParadropInCompanies, bool CanHaveGliderInCompanies) {
                this.Faction = Faction;
                this.Drivers = Drivers;
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

            public string[] Files { get; }

            public GamemodeOption[] Options { get; }

            [JsonConstructor]
            public Gamemode(string ID, string[] Files, GamemodeOption[] Options) {
                this.ID = ID;
                this.Files = Files;
                this.Options = Options;
            }

        }

        public readonly struct CustomOptions {

        }

        public string ID { get; init; }

        public ModGuid TuningGUID { get; init; }

        public ModGuid GamemodeGUID { get; init; }

        public ModGuid AssetGUID { get; init; }

        public UcsFile[] LocaleFiles { get; init; }

        public string VerificationUpgrade { get; init; }

        public bool IsTowEnabled { get; init; }

        public string IsTowedUpgrade { get; init; }

        public string IsTowingUpgrade { get; init; }

        public string[] ParadropUnits { get; init; }

        public bool AllowSupplySystem { get; init; }

        public bool AllowWeatherSystem { get; init; }

        public Dictionary<Faction, FactionData> FactionSettings { get; init; }

        public Gamemode[] Gamemodes { get; init; }

    }

}
