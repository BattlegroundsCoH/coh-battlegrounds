using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

using Battlegrounds.Functional;
using Battlegrounds.Game.Database.Extensions;
using Battlegrounds.Game.Database.Management;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Modding;

namespace Battlegrounds.Game.Database {
    
    /// <summary>
    /// 
    /// </summary>
    [JsonConverter(typeof(EntityBlueprintConverter))]
    public sealed class EntityBlueprint : Blueprint {

        /// <summary>
        /// The unique PropertyBagGroupdID assigned to this blueprint.
        /// </summary>
        public override BlueprintUID PBGID { get; }

        public override BlueprintType BlueprintType => BlueprintType.EBP;

        public override string Name { get; }

        public CostExtension Cost { get; }

        public UIExtension Display { get; }

        public DriverExtension Drivers { get; }

        public Faction Faction { get; }

        public string[] Abilities { get; }

        public string[] Hardpoints { get; }

        public float Health { get; }

        public EntityBlueprint(string name, BlueprintUID pbgid, Faction faction, 
            CostExtension cost, UIExtension ui, DriverExtension driverExtension,
            string[] abilities, string[] hardpoints, float health) {
            this.Name = name;
            this.PBGID = pbgid;
            this.Display = ui;
            this.Cost = cost;
            this.Faction = faction;
            this.Abilities = abilities;
            this.Hardpoints = hardpoints;
            this.Health = health;
            this.Drivers = driverExtension;
        }

    }

    public class EntityBlueprintConverter : JsonConverter<EntityBlueprint> {
        
        public override EntityBlueprint Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            Dictionary<string, object> __lookup = new();
            while (reader.Read() && reader.TokenType is not JsonTokenType.EndObject) {
                string prop = reader.ReadProperty();
                __lookup[prop] = prop switch {
                    "Cost" => CostExtension.FromJson(ref reader),
                    "Display" => UIExtension.FromJson(ref reader),
                    "Army" => reader.GetString(),
                    "PBGID" => reader.GetUInt64(),
                    "Name" => reader.GetString(),
                    "ModGUID" => reader.GetString(),
                    "Health" => reader.GetSingle(),
                    "Abilities" => reader.GetStringArray(),
                    "Hardpoints" => reader.GetStringArray(),
                    "Drivers" => DriverExtension.FromJson(ref reader),
                    _ => throw new NotImplementedException(prop)
                };
            }
            var cost = __lookup.GetValueOrDefault("Cost", null) as CostExtension;
            var ui = __lookup.GetValueOrDefault("Display", null) as UIExtension;
            var driver = __lookup.GetValueOrDefault("Display", new DriverExtension(Array.Empty<DriverExtension.Entry>())) as DriverExtension;
            var abilities = __lookup.GetValueOrDefault("Abilities", Array.Empty<string[]>()) as string[];
            var hardpoints = __lookup.GetValueOrDefault("Hardpoints", Array.Empty<string[]>()) as string[];
            var hp = (float)__lookup.GetValueOrDefault("Health", 0.0f);
            var fac = __lookup.GetValueOrDefault("Army", "NULL") is "NULL" ? null : Faction.FromName(__lookup.GetValueOrDefault("Army", "NULL") as string);
            var modguid = __lookup.ContainsKey("ModGUID") ? ModGuid.FromGuid(__lookup["ModGUID"] as string) : ModGuid.BaseGame;
            var pbgid = new BlueprintUID((ulong)__lookup.GetValueOrDefault("PBGID", 0ul), modguid);
            return new(__lookup.GetValueOrDefault("Name", string.Empty) as string, pbgid, fac, cost, ui, driver, abilities, hardpoints, hp);
        }
        
        public override void Write(Utf8JsonWriter writer, EntityBlueprint value, JsonSerializerOptions options) => writer.WriteStringValue(value.Name);

    }

}
