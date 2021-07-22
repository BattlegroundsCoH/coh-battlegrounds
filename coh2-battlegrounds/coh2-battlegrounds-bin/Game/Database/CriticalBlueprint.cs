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
    [JsonConverter(typeof(CriticalBlueprintConverter))]
    public sealed class CriticalBlueprint : Blueprint {

        /// <summary>
        /// The unique PropertyBagGroupdID assigned to this blueprint.
        /// </summary>
        public override BlueprintUID PBGID { get; }

        /// <summary>
        /// 
        /// </summary>
        public override BlueprintType BlueprintType => BlueprintType.CBP;

        /// <summary>
        /// 
        /// </summary>
        public override string Name { get; }

        /// <summary>
        /// 
        /// </summary>
        public UIExtension Display { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="name"></param>
        /// <param name="ui"></param>
        public CriticalBlueprint(string name, BlueprintUID uid, UIExtension ui) {
            this.PBGID = uid;
            this.Name = name;
            this.Display = ui;
        }

    }

    /// <summary>
    /// Converter for converting a string into an <see cref="CriticalBlueprint"/>.
    /// </summary>
    public sealed class CriticalBlueprintConverter : JsonConverter<CriticalBlueprint> {
        
        public override CriticalBlueprint Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            Dictionary<string, object> __lookup = new();
            while (reader.Read() && reader.TokenType is not JsonTokenType.EndObject) {
                string prop = reader.ReadProperty();
                __lookup[prop] = prop switch {
                    "Display" => UIExtension.FromJson(ref reader),
                    "PBGID" => reader.GetUInt64(),
                    "Name" => reader.GetString(),
                    "ModGUID" => reader.GetString(),
                    _ => throw new NotImplementedException(prop)
                };
            }
            var ui = __lookup.GetValueOrDefault("Display", null) as UIExtension;
            var modguid = __lookup.ContainsKey("ModGUID") ? ModGuid.FromGuid(__lookup["ModGUID"] as string) : ModGuid.BaseGame;
            var pbgid = new BlueprintUID((ulong)__lookup.GetValueOrDefault("PBGID", 0ul), modguid);
            return new(__lookup.GetValueOrDefault("Name", string.Empty) as string, pbgid,  ui);
        }

        public override void Write(Utf8JsonWriter writer, CriticalBlueprint value, JsonSerializerOptions options) => writer.WriteStringValue(value.Name);

    }

}
