using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

using Battlegrounds.Functional;
using Battlegrounds.Game.Database.Management;
using Battlegrounds.Modding;

namespace Battlegrounds.Game.Database {

    /// <summary>
    /// 
    /// </summary>
    [JsonConverter(typeof(WeaponBlueprintConverter))]
    public class WeaponBlueprint : Blueprint {

        /// <summary>
        /// The unique PropertyBagGroupdID assigned to this blueprint.
        /// </summary>
        public override BlueprintUID PBGID { get; }

        public override BlueprintType BlueprintType => BlueprintType.WBP;

        public override string Name { get; }

        /// <summary>
        /// 
        /// </summary>
        public float MaxDamage { get; }

        /// <summary>
        /// 
        /// </summary>
        public float MaxRange { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="pbgid"></param>
        /// <param name="dmg"></param>
        /// <param name="range"></param>
        public WeaponBlueprint(string name, BlueprintUID pbgid, float dmg, float range) {
            this.Name = name;
            this.PBGID = pbgid;
            this.MaxDamage = dmg;
            this.MaxRange = range;
        }

    }

    public class WeaponBlueprintConverter : JsonConverter<WeaponBlueprint> {

        public override WeaponBlueprint Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            Dictionary<string, object> __lookup = new();
            while (reader.Read() && reader.TokenType is not JsonTokenType.EndObject) {
                string prop = reader.ReadProperty();
                __lookup[prop] = prop switch {
                    "PBGID" => reader.GetUInt64(),
                    "Name" => reader.GetString(),
                    "Damage" => reader.GetSingle(),
                    "Range" => reader.GetSingle(),
                    "ModGUID" => reader.GetString(),
                    _ => throw new NotImplementedException(prop)
                };
            }
            var dmg = (float)__lookup.GetValueOrDefault("Display", 0.0f);
            var rng = (float)__lookup.GetValueOrDefault("Cost", 0.0f);
            var modguid = __lookup.ContainsKey("ModGUID") ? ModGuid.FromGuid(__lookup["ModGUID"] as string) : ModGuid.BaseGame;
            var pbgid = new BlueprintUID((ulong)__lookup.GetValueOrDefault("PBGID", 0ul), modguid);
            return new(__lookup.GetValueOrDefault("Name", string.Empty) as string, pbgid, dmg, rng);
        }

        public override void Write(Utf8JsonWriter writer, WeaponBlueprint value, JsonSerializerOptions options) => writer.WriteStringValue(value.Name);

    }


}
