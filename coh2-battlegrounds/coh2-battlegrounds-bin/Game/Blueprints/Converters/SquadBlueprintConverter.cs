using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Text.Json;

using Battlegrounds.Errors.Common;
using Battlegrounds.Functional;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Modding;
using Battlegrounds.Game.Blueprints.Extensions;

namespace Battlegrounds.Game.Blueprints.Converters;

/// <summary>
/// 
/// </summary>
public sealed class SquadBlueprintConverter : JsonConverter<SquadBlueprint> {

    private readonly ModGuid modGuid;
    private readonly GameCase game;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="modGuid"></param>
    /// <param name="game"></param>
    public SquadBlueprintConverter(ModGuid modGuid, GameCase game) {
        this.modGuid = modGuid;
        this.game = game;
    }

    /// <inheritdoc/>
    public override SquadBlueprint Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        Dictionary<string, object> __lookup = new();
        while (reader.Read() && reader.TokenType is not JsonTokenType.EndObject) {
            string prop = reader.ReadProperty() ?? throw new ObjectPropertyNotFoundException();
            __lookup[prop] = prop switch {
                "SquadCost" => CostExtension.FromJson(ref reader),
                "Display" => UIExtension.FromJson(ref reader),
                "Entities" => LoadoutExtension.FromJson(ref reader),
                "Veterancy" => VeterancyExtension.FromJson(ref reader),
                "PBGID" => reader.GetUInt64(),
                "Name" => reader.GetString() ?? string.Empty,
                "Army" => reader.GetString() ?? string.Empty,
                "ModGUID" => reader.GetString() ?? string.Empty,
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
                "ParentFilepath" => reader.GetString() ?? string.Empty,
                _ => throw new NotImplementedException(prop)
            };
        }
        Faction? fac = Faction.TryGetFromName(__lookup.GetCastValueOrDefault("Army", "NULL"), game);
        BlueprintUID pbgid = new BlueprintUID(__lookup.GetCastValueOrDefault("PBGID", 0ul), modGuid);
        return new(__lookup.GetCastValueOrDefault("Name", string.Empty),
            pbgid,
            fac,
            __lookup.GetCastValueOrDefault("Display", new UIExtension()),
            __lookup.GetCastValueOrDefault("SquadCost", new CostExtension()),
            __lookup.GetCastValueOrDefault("Entities", new LoadoutExtension(Array.Empty<LoadoutExtension.Entry>())),
            __lookup.GetCastValueOrDefault("Veterancy", new VeterancyExtension(Array.Empty<VeterancyExtension.Rank>())),
            __lookup.GetCastValueOrDefault("Types", Array.Empty<string>()),
            __lookup.GetCastValueOrDefault("Abilities", Array.Empty<string>()),
            __lookup.GetCastValueOrDefault("Upgrades", Array.Empty<string>()),
            __lookup.GetCastValueOrDefault("AppliedUpgrades", Array.Empty<string>()),
            __lookup.GetCastValueOrDefault("UpgradeCapacity", 0),
            __lookup.GetCastValueOrDefault("SlotPickupCapacity", 0),
            __lookup.GetCastValueOrDefault("CanPickupItems", false),
            __lookup.GetCastValueOrDefault("IsSyncWeapon", false),
            __lookup.GetCastValueOrDefault("HasCrew", false),
            __lookup.GetCastValueOrDefault("FemaleChance", 0.0f)) { Game = game };
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, SquadBlueprint value, JsonSerializerOptions options) => writer.WriteStringValue(value.Name);

}
