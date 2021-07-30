using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

using Battlegrounds.Functional;
using Battlegrounds.Game.Database.Extensions;
using Battlegrounds.Game.Database.Management;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Modding;

namespace Battlegrounds.Game.Database {

    /// <summary>
    /// Representation of a <see cref="Blueprint"/> with <see cref="Squad"/> specific values. Inherits from <see cref="Blueprint"/>. This class cannot be inherited.
    /// </summary>
    [JsonConverter(typeof(SquadBlueprintConverter))]
    public sealed class SquadBlueprint : Blueprint {

        /// <summary>
        /// The unique PropertyBagGroupdID assigned to this blueprint.
        /// </summary>
        public override BlueprintUID PBGID { get; }

        public override BlueprintType BlueprintType => BlueprintType.SBP;

        public override string Name { get; }

        /// <summary>
        /// The army the <see cref="SquadBlueprint"/> can be used by.
        /// </summary>
        public Faction Army { get; }

        /// <summary>
        /// 
        /// </summary>
        public UIExtension UI { get; }

        /// <summary>
        /// The base <see cref="CostExtension"/> to field instances of the <see cref="SquadBlueprint"/>.
        /// </summary>
        public CostExtension Cost { get; }

        /// <summary>
        /// 
        /// </summary>
        public VeterancyExtension Veterancy { get; }

        /// <summary>
        /// 
        /// </summary>
        public LoadoutExtension Loadout { get; }

        /// <summary>
        /// Does the squad the bluperint is for, require a crew.
        /// </summary>
        public bool HasCrew { get; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsTeamWeapon { get; }

        /// <summary>
        /// 
        /// </summary>
        public bool CanPickupItems { get; }

        /// <summary>
        /// 
        /// </summary>
        public int PickupCapacity { get; }

        /// <summary>
        /// 
        /// </summary>
        public string[] Abilities { get; }

        /// <summary>
        /// 
        /// </summary>
        public float FemaleSquadChance { get; }

        /// <summary>
        /// Array of types bound to the <see cref="SquadBlueprint"/>.
        /// </summary>
        public HashSet<string> Types { get; }

        /// <summary>
        /// Can the <see cref="SquadBlueprint"/> be considered a heavy artillery piece.
        /// </summary>
        [JsonIgnore]
        public bool IsHeavyArtillery
            => this.Types.ToArray().ContainsWithout("team_weapon", "wg_team_weapons", "mortar", "hmg"); // 'wg_team_weapons' is to block the raketenwerfer be considered a heavy artillery piece

        /// <summary>
        /// Can the <see cref="SquadBlueprint"/> be considered an anti-tank gun.
        /// </summary>
        [JsonIgnore] public bool IsAntiTank => this.Types.Contains("at_gun");

        /// <summary>
        /// Can the <see cref="SquadBlueprint"/> be considered infantry.
        /// </summary>
        [JsonIgnore] public bool IsInfantry => this.Types.Contains("infantry") && !IsTeamWeapon;

        /// <summary>
        /// Can the <see cref="SquadBlueprint"/> be considered a vehicle (not a tank).
        /// </summary>
        [JsonIgnore]
        public bool IsVehicle
            => ((!IsArmour && this.Types.Contains("vehicle")) || this.Types.Contains("light_vehicle")) && !this.Types.Contains("250_mortar_halftrack"); // Remove the change of mortar vehicles in this category

        /// <summary>
        /// Can the <see cref="SquadBlueprint"/> be considered a tank.
        /// </summary>
        [JsonIgnore] public bool IsArmour => this.Types.Contains("vehicle") && !this.Types.Contains("light_vehicle") && !IsHeavyArmour;

        /// <summary>
        /// Can the <see cref="SquadBlueprint"/> be considered a heavy tank.
        /// </summary>
        [JsonIgnore] public bool IsHeavyArmour => this.Types.Contains("heavy_tank");

        /// <summary>
        /// Can the <see cref="SquadBlueprint"/> be considered a vehicle crew.
        /// </summary>
        [JsonIgnore] public bool IsVehicleCrew => this.Types.Contains("aef_vehicle_crew");

        /// <summary>
        /// Can the <see cref="SquadBlueprint"/> be considered a special (elite) infantry.
        /// </summary>
        [JsonIgnore]
        public bool IsSpecialInfantry
            => this.Types.Contains("guard_troops") || this.Types.Contains("shock_troops") || this.Types.Contains("stormtrooper");

        /// <summary>
        /// Can the <see cref="SquadBlueprint"/> be considered to be an officer unit.
        /// </summary>
        [JsonIgnore] public bool IsOfficer => this.Types.Contains("sov_officer");

        /// <summary>
        /// Can the <see cref="SquadBlueprint"/> be considered to be a command unit.
        /// </summary>
        [JsonIgnore] public bool IsCommandUnit => IsOfficer || this.Types.Contains("command_panzer");

        /// <summary>
        /// Can the <see cref="SquadBlueprint"/> be considered to be artillery.
        /// </summary>
        [JsonIgnore] public bool IsArtillery => this.Types.Contains("artillery");

        /// <summary>
        /// Can the <see cref="SquadBlueprint"/> be considered a sniper unit.
        /// </summary>
        [JsonIgnore] public bool IsSniper => this.Types.Contains("sniper_soviet") || this.Types.Contains("sniper_german");

        /// <summary>
        /// Can the <see cref="SquadBlueprint"/> be considered a transport unit.
        /// </summary>
        [JsonIgnore] public bool IsTransportVehicle => this.Types.Contains("m5_halftrack") || this.Types.Contains("m3a1_scout_car") || this.Types.Contains("251_halftrack");

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="pbgid"></param>
        /// <param name="faction"></param>
        /// <param name="ui"></param>
        /// <param name="cost"></param>
        /// <param name="loadout"></param>
        /// <param name="veterancy"></param>
        /// <param name="types"></param>
        /// <param name="abilities"></param>
        /// <param name="slotCapacity"></param>
        /// <param name="canPickup"></param>
        /// <param name="isTeamWpn"></param>
        /// <param name="femaleChance"></param>
        public SquadBlueprint(string name, BlueprintUID pbgid, Faction faction,
            UIExtension ui, CostExtension cost, LoadoutExtension loadout, VeterancyExtension veterancy,
            string[] types, string[] abilities, int slotCapacity, bool canPickup, bool isTeamWpn, bool hasCrew, float femaleChance) {

            // Set properties
            this.Name = name;
            this.PBGID = pbgid;
            this.UI = ui;
            this.Cost = cost;
            this.Army = faction;
            this.IsTeamWeapon = isTeamWpn;
            this.Types = new(types);
            this.Loadout = loadout;
            this.Veterancy = veterancy;
            this.PickupCapacity = slotCapacity;
            this.CanPickupItems = canPickup;
            this.Abilities = abilities;
            this.FemaleSquadChance = femaleChance;
            this.HasCrew = hasCrew;

        }

        /// <summary>
        /// Get the <see cref="SquadBlueprint"/> that is the crew of the <see cref="SquadBlueprint"/>
        /// </summary>
        /// <param name="faction">The faction to get crew SBP from.</param>
        /// <returns>The crew <see cref="SquadBlueprint"/>. If not found for <paramref name="faction"/>, the default crew is returned.</returns>
        public SquadBlueprint GetCrewBlueprint(Faction faction = null) {

            // Make sure there's actually a crew to get
            if (!this.HasCrew) {
                return null;
            }

            // Loop over entities in loadout
            for (int i = 0; i < this.Loadout.Count; i++) {
                var e = this.Loadout.GetEntity(i);
                if (e.Drivers is DriverExtension drivers) {
                    return faction is null ? drivers.GetSquad(this.Army) : drivers.GetSquad(faction);
                }
            }

            // No driver was found
            return null;

        }

    }

    /// <summary>
    /// 
    /// </summary>
    public class SquadBlueprintConverter : JsonConverter<SquadBlueprint> {

        public override SquadBlueprint Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            Dictionary<string, object> __lookup = new();
            while (reader.Read() && reader.TokenType is not JsonTokenType.EndObject) {
                string prop = reader.ReadProperty();
                __lookup[prop] = prop switch {
                    "SquadCost" => CostExtension.FromJson(ref reader),
                    "Display" => UIExtension.FromJson(ref reader),
                    "Entities" => LoadoutExtension.FromJson(ref reader),
                    "Veterancy" => VeterancyExtension.FromJson(ref reader),
                    "PBGID" => reader.GetUInt64(),
                    "Name" => reader.GetString(),
                    "Army" => reader.GetString(),
                    "ModGUID" => reader.GetString(),
                    "IsSyncWeapon" => reader.GetBoolean(),
                    "Abilities" => reader.GetStringArray(),
                    "Types" => reader.GetStringArray(),
                    "SlotPickupCapacity" => reader.GetInt32(),
                    "CanPickupItems" => reader.GetBoolean(),
                    "FemaleChance" => reader.GetSingle(),
                    "HasCrew" => reader.GetBoolean(),
                    _ => throw new NotImplementedException(prop)
                };
            }
            var cost = __lookup.GetValueOrDefault("SquadCost", null) as CostExtension;
            var ui = __lookup.GetValueOrDefault("Display", null) as UIExtension;
            var loadout = __lookup.GetValueOrDefault("Entities", new VeterancyExtension(Array.Empty<VeterancyExtension.Rank>())) as LoadoutExtension;
            var vet = __lookup.GetValueOrDefault("Veterancy", new VeterancyExtension(Array.Empty<VeterancyExtension.Rank>())) as VeterancyExtension;
            var femalechance = (float)__lookup.GetValueOrDefault("FemaleChance", 0.0f);
            var slotsize = (int)__lookup.GetValueOrDefault("SlotPickupCapacity", 0);
            var picker = (bool)__lookup.GetValueOrDefault("CanPickupItems", false);
            var issync = (bool)__lookup.GetValueOrDefault("IsSyncWeapon", false);
            var hascrew = (bool)__lookup.GetValueOrDefault("HasCrew", false);
            var types = __lookup.GetValueOrDefault("Types", Array.Empty<string>()) as string[];
            var abilities = __lookup.GetValueOrDefault("Abilities", Array.Empty<string>()) as string[];
            var fac = __lookup.GetValueOrDefault("Army", "NULL") is "NULL" ? null : Faction.FromName(__lookup.GetValueOrDefault("Army", "NULL") as string);
            var modguid = __lookup.ContainsKey("ModGUID") ? ModGuid.FromGuid(__lookup["ModGUID"] as string) : ModGuid.BaseGame;
            var pbgid = new BlueprintUID((ulong)__lookup.GetValueOrDefault("PBGID", 0ul), modguid);
            return new(__lookup.GetValueOrDefault("Name", string.Empty) as string, pbgid, fac, ui, cost, loadout, vet, types, abilities, slotsize, picker, issync, hascrew, femalechance);
        }

        public override void Write(Utf8JsonWriter writer, SquadBlueprint value, JsonSerializerOptions options) => writer.WriteStringValue(value.Name);

    }

}
