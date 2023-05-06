using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Text.Json;

using Battlegrounds.Errors.Common;
using Battlegrounds.Functional;
using Battlegrounds.Game.Blueprints.Extensions;
using Battlegrounds.Modding;

namespace Battlegrounds.Game.Blueprints.Converters;

/// <summary>
/// 
/// </summary>
public class UpgradeBlueprintConverter : JsonConverter<UpgradeBlueprint> {

    private readonly ModGuid modGuid;
    private readonly GameCase game;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="modGuid"></param>
    /// <param name="game"></param>
    public UpgradeBlueprintConverter(ModGuid modGuid, GameCase game) {
        this.modGuid = modGuid;
        this.game = game;
    }

    private bool ReadFromString(ref Utf8JsonReader reader, out UpgradeBlueprint? ubp) {
        ubp = null;
        if (reader.TokenType is JsonTokenType.String) {
            ubp = BattlegroundsContext.ModManager.GetPackageFromGuid(modGuid, this.game)
                ?.GetDataSource()
                .GetBlueprints(game)
                .FromBlueprintName<UpgradeBlueprint>(reader.GetString() ?? string.Empty);
            return ubp is not null;
        }
        return false;
    }

    /// <inheritdoc/>
    public override UpgradeBlueprint? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {

        // Return ubp from blueprint manager if string variant.
        if (ReadFromString(ref reader, out UpgradeBlueprint? ubp)) {
            return ubp;
        }

        Dictionary<string, object> __lookup = new();
        while (reader.Read() && reader.TokenType is not JsonTokenType.EndObject) {
            string prop = reader.ReadProperty() ?? throw new ObjectPropertyNotFoundException();
            __lookup[prop] = prop switch {
                "Cost" => CostExtension.FromJson(ref reader),
                "Display" => UIExtension.FromJson(ref reader),
                "PBGID" => reader.GetUInt64(),
                "Name" => reader.GetString() ?? throw new ObjectPropertyNotFoundException("Name"),
                "ModGUID" => reader.GetString() ?? throw new ObjectPropertyNotFoundException("ModGUID"),
                "SlotItems" => reader.GetStringArray(),
                "OwnerType" => reader.GetString() switch {
                    "self" => UpgradeBlueprint.OwnerType.Self,
                    "player" => UpgradeBlueprint.OwnerType.Player,
                    "entity_in_squad" => UpgradeBlueprint.OwnerType.EntityInSquad,
                    _ => UpgradeBlueprint.OwnerType.None,
                },
                "Requirements" => JsonSerializer.Deserialize<RequirementExtension[]>(ref reader, options) ?? Array.Empty<RequirementExtension>(),
                "ParentFilepath" => reader.GetString() ?? string.Empty,
                _ => throw new NotImplementedException(prop)
            };
        }
        ModGuid modguid = __lookup.ContainsKey("ModGUID") ? ModGuid.FromGuid((string)__lookup["ModGUID"]) : ModGuid.BaseGame;
        BlueprintUID pbgid = new BlueprintUID(__lookup.GetCastValueOrDefault("PBGID", 0ul), modguid);
        return new(__lookup.GetCastValueOrDefault("Name", string.Empty), pbgid,
            __lookup.GetCastValueOrDefault("OwnerType", UpgradeBlueprint.OwnerType.None),
            __lookup.GetCastValueOrDefault("Display", new UIExtension()),
            __lookup.GetCastValueOrDefault("Cost", new CostExtension()),
            __lookup.GetCastValueOrDefault("Requirements", Array.Empty<RequirementExtension>()),
            __lookup.GetCastValueOrDefault("SlotItems", Array.Empty<string>())) { Game = game };
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, UpgradeBlueprint value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.PBGID.Mod != ModGuid.BaseGame ? $"{value.PBGID.Mod.GUID}:{value.Name}" : value.Name);

}
