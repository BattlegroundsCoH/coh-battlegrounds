using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

using Battlegrounds.Functional;
using Battlegrounds.Game.Database.Extensions;
using Battlegrounds.Game.Database.Management;
using Battlegrounds.Modding;

namespace Battlegrounds.Game.Database {
    
    /// <summary>
    /// 
    /// </summary>
    [JsonConverter(typeof(SlotItemBlueprintConverter))]
    public sealed class SlotItemBlueprint : Blueprint {

        /// <summary>
        /// The unique PropertyBagGroupdID assigned to this blueprint.
        /// </summary>
        public override BlueprintUID PBGID { get; }

        public override BlueprintType BlueprintType => BlueprintType.IBP;

        public override string Name { get; }

        public UIExtension UI { get; }

        public int SlotWeight { get; }

        public string Weapon { get; }

        public SlotItemBlueprint(string name, BlueprintUID pbgid, UIExtension ui, int weight, string wpn) {
            this.Name = name;
            this.PBGID = pbgid;
            this.UI = ui;
            this.SlotWeight = weight;
            this.Weapon = wpn;
        }

    }

    public class SlotItemBlueprintConverter : JsonConverter<SlotItemBlueprint> {

        public override SlotItemBlueprint Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            Dictionary<string, object> __lookup = new();
            while (reader.Read() && reader.TokenType is not JsonTokenType.EndObject) {
                string prop = reader.ReadProperty();
                __lookup[prop] = prop switch {
                    "Display" => UIExtension.FromJson(ref reader),
                    "PBGID" => reader.GetUInt64(),
                    "Name" => reader.GetString(),
                    "ModGUID" => reader.GetString(),
                    "SlotSize" => reader.GetInt32(),
                    "WPB" => reader.GetString(),
                    _ => throw new NotImplementedException(prop)
                };
            }
            var ui = __lookup.GetValueOrDefault("Display", null) as UIExtension;
            var weight = (int)__lookup.GetValueOrDefault("SlotSize", 0);
            string wpn = __lookup.GetValueOrDefault("WPB", string.Empty) as string;
            var modguid = __lookup.ContainsKey("ModGUID") ? ModGuid.FromGuid(__lookup["ModGUID"] as string) : ModGuid.BaseGame;
            var pbgid = new BlueprintUID((ulong)__lookup.GetValueOrDefault("PBGID", 0ul), modguid);
            return new(__lookup.GetValueOrDefault("Name", string.Empty) as string, pbgid, ui, weight, wpn);
        }

        public override void Write(Utf8JsonWriter writer, SlotItemBlueprint value, JsonSerializerOptions options) => writer.WriteStringValue(value.Name);

    }

}
