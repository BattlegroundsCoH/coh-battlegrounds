using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Text.Json;

using Battlegrounds.Errors.Common;
using Battlegrounds.Functional;
using Battlegrounds.Modding;

namespace Battlegrounds.Game.Blueprints.Converters;

/// <summary>
/// 
/// </summary>
public class WeaponBlueprintConverter : JsonConverter<WeaponBlueprint> {

    private readonly ModGuid modGuid;
    private readonly GameCase game;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="modGuid"></param>
    /// <param name="game"></param>
    public WeaponBlueprintConverter(ModGuid modGuid, GameCase game) {
        this.modGuid = modGuid;
        this.game = game;
    }

    /// <inheritdoc/>
    public override WeaponBlueprint Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        Dictionary<string, object> __lookup = new();
        while (reader.Read() && reader.TokenType is not JsonTokenType.EndObject) {
            string prop = reader.ReadProperty() ?? throw new ObjectPropertyNotFoundException();
            __lookup[prop] = prop switch {
                "PBGID" => reader.GetUInt64(),
                "Name" => reader.GetString() ?? throw new ObjectPropertyNotFoundException("Name"),
                "Damage" => reader.GetSingle(),
                "Range" => reader.GetSingle(),
                "ModGUID" => reader.GetString() ?? throw new ObjectPropertyNotFoundException("ModGUID"),
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
                "CallbackType" => GetCallbackType(reader.GetString() ?? string.Empty),
                _ => throw new NotImplementedException(prop)
            };
        }
        var cat = __lookup.GetCastValueOrDefault("Category", WeaponCategory.Undefined);
        var pbgid = new BlueprintUID(__lookup.GetCastValueOrDefault("PBGID", 0ul), modGuid);
        return new(
            __lookup.GetCastValueOrDefault("Name", string.Empty),
            pbgid,
            cat,
            cat switch {
                WeaponCategory.Ballistic => __lookup.GetCastValueOrDefault("BalisticType", WeaponBallisticType.Invalid),
                WeaponCategory.SmallArms => __lookup.GetCastValueOrDefault("SmallArmsType", WeaponSmallArmsType.Invalid),
                WeaponCategory.Explosive => __lookup.GetCastValueOrDefault("ExplosiveType", WeaponExplosiveType.Invalid),
                _ => WeaponExplosiveType.Invalid
            },
            __lookup.GetCastValueOrDefault("CallbackType", WeaponOnFireCallbackType.None),
            __lookup.GetCastValueOrDefault("Damage", 0.0f),
            __lookup.GetCastValueOrDefault("Range", 0.0f),
            __lookup.GetCastValueOrDefault("MagazineSize", 0),
            __lookup.GetCastValueOrDefault("FireRate", 0.0f)) { Game = game };
    }

    private static WeaponOnFireCallbackType GetCallbackType(string val) {
        if (Enum.TryParse(val, out WeaponOnFireCallbackType r)) {
            return r;
        } else {
            return WeaponOnFireCallbackType.None;
        }
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, WeaponBlueprint value, JsonSerializerOptions options) => writer.WriteStringValue(value.Name);

}
