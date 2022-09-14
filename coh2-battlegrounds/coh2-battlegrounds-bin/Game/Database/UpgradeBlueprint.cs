using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

using Battlegrounds.ErrorHandling.CommonExceptions;
using Battlegrounds.Functional;
using Battlegrounds.Game.Database.Extensions;
using Battlegrounds.Game.Database.Management;
using Battlegrounds.Modding;

namespace Battlegrounds.Game.Database;

/// <summary>
/// Representation of a <see cref="Blueprint"/> with upgrade specific values. Inherits from <see cref="Blueprint"/>. This class cannot be inheritted.
/// </summary>
[JsonConverter(typeof(UpgradeBlueprintConverter))]
public class UpgradeBlueprint : Blueprint, IUIBlueprint {

    public enum OwnerType {
        None,
        Self,
        Player,
        EntityInSquad
    }

    /// <summary>
    /// The unique PropertyBagGroupdID assigned to this blueprint.
    /// </summary>
    public override BlueprintUID PBGID { get; }

    public override BlueprintType BlueprintType => BlueprintType.UBP;

    public override string Name { get; }

    /// <summary>
    /// Get the <see cref="OwnerType"/> for the upgrade.
    /// </summary>
    public OwnerType OwnershipType { get; }

    /// <summary>
    /// Get the UI extension of the upgrade
    /// </summary>
    public UIExtension UI { get; }

    /// <summary>
    /// Get the cost extension of the ability.
    /// </summary>
    public CostExtension Cost { get; }

    /// <summary>
    /// The names of the granted <see cref="SlotItemBlueprint"/> by the <see cref="UpgradeBlueprint"/>.
    /// </summary>
    public HashSet<string> SlotItems { get; set; }

    /// <summary>
    /// Get the requirements for getting the upgrade.
    /// </summary>
    public RequirementExtension[] Requirements { get; }

    /// <summary>
    /// New <see cref="UpgradeBlueprint"/> instance. This should only ever be used by the database loader!
    /// </summary>
    public UpgradeBlueprint(string name, BlueprintUID pbgid, OwnerType ownertype, UIExtension ui, CostExtension cost, RequirementExtension[] requirements, string[] slotItems) : base() {
        this.Name = name;
        this.PBGID = pbgid;
        this.OwnershipType = ownertype;
        this.UI = ui;
        this.Cost = cost;
        this.SlotItems = new(slotItems);
        this.Requirements = requirements;
    }

}

public class UpgradeBlueprintConverter : JsonConverter<UpgradeBlueprint> {

    private static bool ReadFromString(ref Utf8JsonReader reader, out UpgradeBlueprint? ubp) {
        ubp = null;
        if (reader.TokenType is JsonTokenType.String) {
            ubp = BlueprintManager.FromBlueprintName<UpgradeBlueprint>(reader.GetString() ?? string.Empty);
            return ubp is not null;
        }
        return false;
    }

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
            __lookup.GetCastValueOrDefault("SlotItems", Array.Empty<string>()));
    }

    public override void Write(Utf8JsonWriter writer, UpgradeBlueprint value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.PBGID.Mod != ModGuid.BaseGame ? $"{value.PBGID.Mod.GUID}:{value.Name}" : value.Name);

}
