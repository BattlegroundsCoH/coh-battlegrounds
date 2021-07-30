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
    [JsonConverter(typeof(AbilityBlueprintConverter))]
    public sealed class AbilityBlueprint : Blueprint {

        /// <summary>
        /// The unique PropertyBagGroupdID assigned to this blueprint.
        /// </summary>
        public override BlueprintUID PBGID { get; }

        /// <summary>
        /// 
        /// </summary>
        public override BlueprintType BlueprintType => BlueprintType.ABP;

        /// <summary>
        /// 
        /// </summary>
        public override string Name { get; }

        /// <summary>
        /// 
        /// </summary>
        public CostExtension Cost { get; }

        /// <summary>
        /// 
        /// </summary>
        public UIExtension UI { get; }

        /// <summary>
        /// 
        /// </summary>
        public Faction Faction { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="blueprintUID"></param>
        /// <param name="faction"></param>
        /// <param name="cost"></param>
        /// <param name="ui"></param>
        public AbilityBlueprint(string name, BlueprintUID blueprintUID, Faction faction, CostExtension cost, UIExtension ui) {
            this.Cost = cost;
            this.UI = ui;
            this.Faction = faction;
            this.Name = name;
            this.PBGID = blueprintUID;
        }

    }

    /// <summary>
    /// Converter for converting a string into an <see cref="AbilityBlueprint"/>.
    /// </summary>
    public sealed class AbilityBlueprintConverter : JsonConverter<AbilityBlueprint> {

        public override AbilityBlueprint Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
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
                    _ => throw new NotImplementedException(prop)
                };
            }
            var cost = __lookup.GetValueOrDefault("Cost", null) as CostExtension;
            var ui = __lookup.GetValueOrDefault("Display", null) as UIExtension;
            var fac = __lookup.GetValueOrDefault("Army", "NULL") is "NULL" ? null : Faction.FromName(__lookup.GetValueOrDefault("Army", "NULL") as string);
            var modguid = __lookup.ContainsKey("ModGUID") ? ModGuid.FromGuid(__lookup["ModGUID"] as string) : ModGuid.BaseGame;
            var pbgid = new BlueprintUID((ulong)__lookup.GetValueOrDefault("PBGID", 0ul), modguid);
            return new(__lookup.GetValueOrDefault("Name", string.Empty) as string, pbgid, fac, cost, ui);
        }

        public override void Write(Utf8JsonWriter writer, AbilityBlueprint value, JsonSerializerOptions options) => writer.WriteStringValue(value.Name);

    }

}
