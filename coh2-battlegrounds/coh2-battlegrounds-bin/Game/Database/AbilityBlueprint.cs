using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

using Battlegrounds.ErrorHandling.CommonExceptions;
using Battlegrounds.Functional;
using Battlegrounds.Game.Database.Extensions;
using Battlegrounds.Game.Database.Management;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Modding;

namespace Battlegrounds.Game.Database;

/// <summary>
/// Enum representing the method in which an ability is activated.
/// </summary>
public enum AbilityActivation {

    /// <summary>
    /// The ability has no activation method
    /// </summary>
    none,

    /// <summary>
    /// The ability is always on.
    /// </summary>
    always_on,

    /// <summary>
    /// The ability requires a target.
    /// </summary>
    targeted,

    /// <summary>
    /// The ability is timed.
    /// </summary>
    timed,

    /// <summary>
    /// The ability is toggled.
    /// </summary>
    toggle

}

/// <summary>
/// Representation of a <see cref="Blueprint"/> with ability specific values. Inherits from <see cref="Blueprint"/>. This class cannot be inherited.
/// </summary>
[JsonConverter(typeof(AbilityBlueprintConverter))]
public sealed class AbilityBlueprint : Blueprint, IUIBlueprint {

    /// <summary>
    /// Invalid ability blueprint instance with no data associated with it.
    /// </summary>
    public static readonly AbilityBlueprint Invalid = new("abp_invalid", new(), null, new(), new(), Array.Empty<RequirementExtension>(), AbilityActivation.none);

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
    /// Get the cost extension of the ability.
    /// </summary>
    public CostExtension Cost { get; }

    /// <summary>
    /// Get the UI extension of the ability.
    /// </summary>
    public UIExtension UI { get; }

    /// <summary>
    /// Get the faction associated with the ability.
    /// </summary>
    public Faction? Faction { get; }

    /// <summary>
    /// Get the requirements required for using this ability.
    /// </summary>
    public RequirementExtension[] Requirements { get; }

    /// <summary>
    /// Get the activiation method of the ability.
    /// </summary>
    public AbilityActivation Activation { get; }

    /// <summary>
    /// 
    /// </summary>
    public bool HasFacingPhase { get; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="blueprintUID"></param>
    /// <param name="faction"></param>
    /// <param name="cost"></param>
    /// <param name="ui"></param>
    public AbilityBlueprint(string name, BlueprintUID blueprintUID, Faction? faction, CostExtension cost, UIExtension ui,
        RequirementExtension[] requirements, AbilityActivation abilityActivation) {
        this.Cost = cost;
        this.UI = ui;
        this.Faction = faction;
        this.Name = name;
        this.PBGID = blueprintUID;
        this.Requirements = requirements;
        this.Activation = abilityActivation;
    }

}

/// <summary>
/// Converter for converting a string into an <see cref="AbilityBlueprint"/>.
/// </summary>
public sealed class AbilityBlueprintConverter : JsonConverter<AbilityBlueprint> {

    private static bool ReadFromString(ref Utf8JsonReader reader, out AbilityBlueprint? abp) {
        abp = null;
        if (reader.TokenType is JsonTokenType.String) {
            abp = BlueprintManager.FromBlueprintName<AbilityBlueprint>(reader.GetString() ?? string.Empty);
            return abp is not null;
        }
        return false;
    }

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
        var fac = __lookup.GetCastValueOrDefault("Army", "NULL") is "NULL" ? null : Faction.FromName(__lookup.GetCastValueOrDefault("Army", "NULL"));
        var modguid = __lookup.ContainsKey("ModGUID") ? ModGuid.FromGuid((string)__lookup["ModGUID"]) : ModGuid.BaseGame;
        BlueprintUID pbgid = new(__lookup.GetCastValueOrDefault("PBGID", 0ul), modguid);
        return new(__lookup.GetCastValueOrDefault("Name", string.Empty), pbgid, fac,
            __lookup.GetCastValueOrDefault("Cost", new CostExtension()),
            __lookup.GetCastValueOrDefault("Display", new UIExtension()),
            __lookup.GetCastValueOrDefault("Requirements", Array.Empty<RequirementExtension>()),
            __lookup.GetCastValueOrDefault("Activation", AbilityActivation.none));
    }

    public override void Write(Utf8JsonWriter writer, AbilityBlueprint value, JsonSerializerOptions options) 
        => writer.WriteStringValue(value.PBGID.Mod != ModGuid.BaseGame ? $"{value.PBGID.Mod.GUID}:{value.Name}" : value.Name);

}
