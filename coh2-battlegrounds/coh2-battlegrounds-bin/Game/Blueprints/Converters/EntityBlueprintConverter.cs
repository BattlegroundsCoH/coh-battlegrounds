using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

using Battlegrounds.Errors.Common;
using Battlegrounds.Functional;
using Battlegrounds.Game.Blueprints.Extensions;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Modding;

namespace Battlegrounds.Game.Blueprints.Converters;

/// <summary>
/// 
/// </summary>
public class EntityBlueprintConverter : JsonConverter<EntityBlueprint> {

    private readonly ModGuid modGuid;
    private readonly GameCase game;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="modGuid"></param>
    /// <param name="game"></param>
    public EntityBlueprintConverter(ModGuid modGuid, GameCase game) {
        this.modGuid = modGuid;
        this.game = game;
    }

    /// <inheritdoc/>
    public override EntityBlueprint Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        Dictionary<string, object> __lookup = new();
        while (reader.Read() && reader.TokenType is not JsonTokenType.EndObject) {
            string prop = reader.ReadProperty() ?? throw new ObjectPropertyNotFoundException();
            __lookup[prop] = prop switch {
                "Cost" => CostExtension.FromJson(ref reader),
                "Display" => UIExtension.FromJson(ref reader),
                "Army" => reader.GetString() ?? string.Empty,
                "PBGID" => reader.GetUInt64(),
                "Name" => reader.GetString() ?? string.Empty,
                "ModGUID" => reader.GetString() ?? string.Empty,
                "Health" => reader.GetSingle(),
                "Abilities" => reader.GetStringArray(),
                "Hardpoints" => reader.GetStringArray(),
                "Drivers" => DriverExtension.FromJson(ref reader),
                "UpgradeCapacity" => reader.GetInt32(),
                "Upgrades" => reader.GetStringArray(),
                "AppliedUpgrades" => reader.GetStringArray(),
                "Types" => reader.GetStringArray(),
                "IsInventoryItem" => reader.GetBoolean(),
                "InventoryRequiredCapacity" => reader.GetInt32(),
                "InventoryDropOnDeathChance" => reader.GetSingle(),
                "ParentFilepath" => reader.GetString() ?? string.Empty,
                _ => throw new NotImplementedException(prop)
            };
        }
        Faction? fac = Faction.TryGetFromName(__lookup.GetCastValueOrDefault("Army", "NULL"), game);
        BlueprintUID pbgid = new BlueprintUID(__lookup.GetCastValueOrDefault("PBGID", 0ul), modGuid);
        return new(__lookup.GetCastValueOrDefault("Name", string.Empty),
            pbgid,
            fac,
            __lookup.GetCastValueOrDefault("Cost", new CostExtension()),
             __lookup.GetCastValueOrDefault("Display", new UIExtension()),
             __lookup.GetCastValueOrDefault("Drivers", new DriverExtension(Array.Empty<DriverExtension.Entry>())),
             __lookup.GetCastValueOrDefault("Abilities", Array.Empty<string>()),
             __lookup.GetCastValueOrDefault("Upgrades", Array.Empty<string>()),
             __lookup.GetCastValueOrDefault("AppliedUpgrades", Array.Empty<string>()),
             __lookup.GetCastValueOrDefault("UpgradeCapacity", 0),
             __lookup.GetCastValueOrDefault("Hardpoints", Array.Empty<string>()),
              (float)__lookup.GetCastValueOrDefault("Health", 0.0f)) { 
            Game = game,
            Types = __lookup.GetCastValueOrDefault("Types", Array.Empty<string>()),
            IsInventoryItem = __lookup.GetCastValueOrDefault("IsInventoryItem", false),
            InventoryRequiredCapacity = __lookup.GetCastValueOrDefault("InventoryRequiredCapacity", -1),
            InventoryDropOnDeathChance = __lookup.GetCastValueOrDefault("InventoryDropOnDeathChance", 0.0f)
        };
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, EntityBlueprint value, JsonSerializerOptions options) => writer.WriteStringValue(value.Name);

}
