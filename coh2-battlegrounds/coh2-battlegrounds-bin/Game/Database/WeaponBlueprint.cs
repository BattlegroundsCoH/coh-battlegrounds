using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

using Battlegrounds.Functional;
using Battlegrounds.Game.Database.Management;
using Battlegrounds.Modding;

namespace Battlegrounds.Game.Database {

    public enum WeaponCategory {
        Undefined,
        Ballistic,
        Explosive,
        Flamethrower,
        SmallArms,
    }

    public enum WeaponSmallArmsType {
        Invalid,
        HeavyMachineGun,
        LightMachineGun,
        SubMachineGun,
        Pistol,
        Rifle
    }

    public enum WeaponBallisticType {
        Invalid,
        AntiTankGun,
        TankGun,
        InfantryAntiTankGun,
    }

    public enum WeaponExplosiveType {
        Invalid,
        Grenade,
        Artillery,
        Mine,
        Mortar
    }

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
        public int MagazineSize { get; }

        /// <summary>
        /// 
        /// </summary>
        public float FireRate { get; }

        /// <summary>
        /// 
        /// </summary>
        public WeaponCategory Category { get; }

        /// <summary>
        /// 
        /// </summary>
        public WeaponSmallArmsType SmallArmsType { get; }

        /// <summary>
        /// 
        /// </summary>
        public WeaponBallisticType BallisticType { get; }

        /// <summary>
        /// 
        /// </summary>
        public WeaponExplosiveType ExplosiveType { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="pbgid"></param>
        /// <param name="dmg"></param>
        /// <param name="range"></param>
        public WeaponBlueprint(string name, BlueprintUID pbgid, WeaponCategory category, Enum specificType, float dmg, float range, int msize, float frate) {
            this.Name = name;
            this.PBGID = pbgid;
            this.MaxDamage = dmg;
            this.MaxRange = range;
            this.FireRate = frate;
            this.MagazineSize = msize;
            this.Category = category;
            switch (category) {
                case WeaponCategory.Ballistic:
                    this.BallisticType = (WeaponBallisticType)specificType;
                    break;
                case WeaponCategory.Explosive:
                    this.ExplosiveType = (WeaponExplosiveType)specificType;
                    break;
                case WeaponCategory.SmallArms:
                    this.SmallArmsType = (WeaponSmallArmsType)specificType;
                    break;
            }
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
                    "MagazineSize" => reader.GetInt32(),
                    "FireRate" => reader.GetSingle(),
                    "Category" => reader.GetString() switch {
                        "ballistic" => WeaponCategory.Ballistic,
                        "explosive" => WeaponCategory.Explosive,
                        "flame" => WeaponCategory.Flamethrower,
                        "smallarms" => WeaponCategory.SmallArms,
                        _ => WeaponCategory.Undefined
                    },
                    "SmallArmsType" => reader.GetString() switch {
                        "heavymachinegun" => WeaponSmallArmsType.HeavyMachineGun,
                        "lightmachinegun" => WeaponSmallArmsType.LightMachineGun,
                        "submachinegun" => WeaponSmallArmsType.SubMachineGun,
                        "pistol" => WeaponSmallArmsType.Pistol,
                        "rifle" => WeaponSmallArmsType.Rifle,
                        _ => WeaponSmallArmsType.Invalid
                    },
                    "BalisticType" => reader.GetString() switch {
                        "antitankgun" => WeaponBallisticType.AntiTankGun,
                        "tankgun" => WeaponBallisticType.TankGun,
                        "infantryatgun" => WeaponBallisticType.InfantryAntiTankGun,
                        _ => WeaponBallisticType.Invalid
                    },
                    "ExplosiveType" => reader.GetString() switch {
                        "grenade" => WeaponExplosiveType.Grenade,
                        "artillery" => WeaponExplosiveType.Artillery,
                        "mine" => WeaponExplosiveType.Mine,
                        "mortar" => WeaponExplosiveType.Mortar,
                        _ => WeaponExplosiveType.Invalid
                    },
                    _ => throw new NotImplementedException(prop)
                };
            }
            var cat = __lookup.GetValueOrDefault("Category", WeaponCategory.Undefined);
            var modguid = __lookup.ContainsKey("ModGUID") ? ModGuid.FromGuid(__lookup["ModGUID"] as string) : ModGuid.BaseGame;
            var pbgid = new BlueprintUID(__lookup.GetValueOrDefault("PBGID", 0ul), modguid);
            return new(
                __lookup.GetValueOrDefault("Name", string.Empty), 
                pbgid,
                cat,
                cat switch {
                    WeaponCategory.Ballistic => __lookup.GetValueOrDefault("BalisticType", WeaponBallisticType.Invalid),
                    WeaponCategory.SmallArms => __lookup.GetValueOrDefault("SmallArmsType", WeaponBallisticType.Invalid),
                    WeaponCategory.Explosive => __lookup.GetValueOrDefault("ExplosiveType", WeaponExplosiveType.Invalid),
                    _ => null
                },
                __lookup.GetValueOrDefault("Damage", 0.0f),
                __lookup.GetValueOrDefault("Range", 0.0f),
                __lookup.GetValueOrDefault("MagazineSize", 0),
                __lookup.GetValueOrDefault("FireRate", 0.0f));
        }

        public override void Write(Utf8JsonWriter writer, WeaponBlueprint value, JsonSerializerOptions options) => writer.WriteStringValue(value.Name);

    }

}
