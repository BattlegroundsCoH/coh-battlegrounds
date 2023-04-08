using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Text.Json;

using Battlegrounds.ErrorHandling.CommonExceptions;
using Battlegrounds.Functional;
using Battlegrounds.Game.Blueprints.Extensions;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Modding;

namespace Battlegrounds.Game.Blueprints.Converters;

/// <summary>
/// Converter for converting a string into an <see cref="AbilityBlueprint"/>.
/// </summary>
public sealed class AbilityBlueprintConverter : JsonConverter<AbilityBlueprint> {

    private readonly ModGuid modGuid;
    private readonly GameCase game;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="modGuid"></param>
    /// <param name="game"></param>
    public AbilityBlueprintConverter(ModGuid modGuid, GameCase game) {
        this.modGuid = modGuid;
        this.game = game;
    }

    private bool ReadFromString(ref Utf8JsonReader reader, out AbilityBlueprint? abp) {
        abp = null;
        if (reader.TokenType is JsonTokenType.String) {
            abp = BattlegroundsContext.ModManager.GetPackageFromGuid(modGuid)
                ?.GetDataSource()
                .GetBlueprints(game)
                .FromBlueprintName<AbilityBlueprint>(reader.GetString() ?? string.Empty);
            return abp is not null;
        }
        return false;
    }

    /// <inheritdoc/>
    public override AbilityBlueprint? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {

        // Return abp from blueprint manager if string variant.
        if (ReadFromString(ref reader, out AbilityBlueprint? abp)) {
            return abp;
        }

        Dictionary<string, object> __lookup = new();
        while (reader.Read() && reader.TokenType is not JsonTokenType.EndObject) {
            string prop = reader.ReadProperty() ?? throw new ObjectPropertyNotFoundException();
            __lookup[prop] = prop switch {
                "Cost" => CostExtension.FromJson(ref reader),
                "Display" => UIExtension.FromJson(ref reader),
                "Army" => reader.GetString() ?? string.Empty,
                "PBGID" => reader.GetUInt64(),
                "Name" => reader.GetString() ?? throw new ObjectPropertyNotFoundException("Name"),
                "ModGUID" => reader.GetString() ?? throw new ObjectPropertyNotFoundException("ModGUID"),
                "Requirements" => JsonSerializer.Deserialize<RequirementExtension[]>(ref reader, options) ?? Array.Empty<RequirementExtension>(),
                "Activation" => reader.GetString() switch {
                    "none" or "" => AbilityActivation.none,
                    "always_on" => AbilityActivation.always_on,
                    "timed" => AbilityActivation.timed,
                    "toggle" => AbilityActivation.toggle,
                    "targeted" => AbilityActivation.targeted,
                    _ => throw new FormatException()
                },
                "HasFacingPhase" => reader.GetBoolean(),
                _ => throw new NotImplementedException(prop)
            };
        }
        var fac = __lookup.GetCastValueOrDefault("Army", "NULL") is "NULL" ? null : Faction.FromName(__lookup.GetCastValueOrDefault("Army", "NULL"), game);
        BlueprintUID pbgid = new(__lookup.GetCastValueOrDefault("PBGID", 0ul), modGuid);
        return new(__lookup.GetCastValueOrDefault("Name", string.Empty), pbgid, fac,
            __lookup.GetCastValueOrDefault("Cost", new CostExtension()),
            __lookup.GetCastValueOrDefault("Display", new UIExtension()),
            __lookup.GetCastValueOrDefault("Requirements", Array.Empty<RequirementExtension>()),
            __lookup.GetCastValueOrDefault("Activation", AbilityActivation.none)) { Game = game };
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, AbilityBlueprint value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.PBGID.Mod != ModGuid.BaseGame ? $"{value.PBGID.Mod.GUID}:{value.Name}" : value.Name);

}
