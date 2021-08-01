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
        public string[] Upgrades { get; }

        /// <summary>
        /// 
        /// </summary>
        public string[] AppliedUpgrades { get; }

        /// <summary>
        /// 
        /// </summary>
        public int UpgradeCapacity { get; }

        /// <summary>
        /// 
        /// </summary>
        public float FemaleSquadChance { get; }

        /// <summary>
        /// Array of types bound to the <see cref="SquadBlueprint"/>.
        /// </summary>
        public TypeList Types { get; }

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
            string[] types, string[] abilities, string[] upgrades, string[] appliedUpgrades,
            int upgradeCapacity, int slotCapacity, bool canPickup, bool isTeamWpn, bool hasCrew, float femaleChance) {

            // Set properties
            this.Name = name;
            this.PBGID = pbgid;
            this.UI = ui;
            this.Cost = cost;
            this.Army = faction;
            this.IsTeamWeapon = isTeamWpn;
            this.Types = new(types, isTeamWpn);
            this.Loadout = loadout;
            this.Veterancy = veterancy;
            this.PickupCapacity = slotCapacity;
            this.CanPickupItems = canPickup;
            this.Abilities = abilities;
            this.FemaleSquadChance = femaleChance;
            this.HasCrew = hasCrew;
            this.Upgrades = upgrades;
            this.AppliedUpgrades = appliedUpgrades;
            this.UpgradeCapacity = upgradeCapacity;

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

        public string[] GetAbilities(bool getEntityAbilities) {

            // If not get entity abilities, return this ability list and be done
            if (!getEntityAbilities) {
                return this.Abilities;
            }

            // Select all entity abilities
            string[] ebpabps = this.Loadout.SelectMany(x => BlueprintManager.FromBlueprintName<EntityBlueprint>(x.EntityBlueprint)?.Abilities ?? Array.Empty<string>());

            // Return union
            return ebpabps.Union(this.Abilities).ToArray();

        }

        public string[] GetUpgrades(bool getEntityUpgrades, bool getAppliedUpgrades) {

            // If not get entity abilities, return this ability list and be done
            if (!getEntityUpgrades) {
                return getAppliedUpgrades ? this.Upgrades.Union(this.AppliedUpgrades).ToArray() : this.Upgrades;
            }

            // Select all entity upgrades
            string[] ebpubps = this.Loadout.SelectMany(
                x => (BlueprintManager.FromBlueprintName<EntityBlueprint>(x.EntityBlueprint)?.Upgrades ?? Array.Empty<string>())
                .Union(getAppliedUpgrades ? BlueprintManager.FromBlueprintName<EntityBlueprint>(x.EntityBlueprint)?.AppliedUpgrades ?? Array.Empty<string>()
                : Array.Empty<string>()).ToArray());

            // Return union
            return ebpubps.Union(this.Upgrades).Union(getAppliedUpgrades ? this.AppliedUpgrades : Array.Empty<string>()).ToArray();

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
                    "UpgradeCapacity" => reader.GetInt32(),
                    "Upgrades" => reader.GetStringArray(),
                    "AppliedUpgrades" => reader.GetStringArray(),
                    _ => throw new NotImplementedException(prop)
                };
            }
            Faction fac = __lookup.GetValueOrDefault("Army", "NULL") is "NULL" ? null : Faction.FromName(__lookup.GetValueOrDefault("Army", "NULL"));
            ModGuid modguid = __lookup.ContainsKey("ModGUID") ? ModGuid.FromGuid(__lookup["ModGUID"] as string) : ModGuid.BaseGame;
            BlueprintUID pbgid = new BlueprintUID(__lookup.GetValueOrDefault("PBGID", 0ul), modguid);
            return new(__lookup.GetValueOrDefault("Name", string.Empty), 
                pbgid, 
                fac,
                __lookup.GetValueOrDefault("Display", new UIExtension()), 
                __lookup.GetValueOrDefault("SquadCost", new CostExtension()),
                __lookup.GetValueOrDefault("Entities", new LoadoutExtension(Array.Empty<LoadoutExtension.Entry>())),
                __lookup.GetValueOrDefault("Veterancy", new VeterancyExtension(Array.Empty<VeterancyExtension.Rank>())),
                __lookup.GetValueOrDefault("Types", Array.Empty<string>()),
                __lookup.GetValueOrDefault("Abilities", Array.Empty<string>()),
                __lookup.GetValueOrDefault("Upgrades", Array.Empty<string>()),
                __lookup.GetValueOrDefault("AppliedUpgrades", Array.Empty<string>()),
                __lookup.GetValueOrDefault("UpgradeCapacity", 0),
                __lookup.GetValueOrDefault("SlotPickupCapacity", 0), 
                __lookup.GetValueOrDefault("CanPickupItems", false), 
                __lookup.GetValueOrDefault("IsSyncWeapon", false),
                __lookup.GetValueOrDefault("HasCrew", false),
                __lookup.GetValueOrDefault("FemaleChance", 0.0f));
        }

        public override void Write(Utf8JsonWriter writer, SquadBlueprint value, JsonSerializerOptions options) => writer.WriteStringValue(value.Name);

    }

}
