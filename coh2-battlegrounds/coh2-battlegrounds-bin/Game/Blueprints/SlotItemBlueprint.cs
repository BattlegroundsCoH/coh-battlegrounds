using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

using Battlegrounds.Errors.Common;
using Battlegrounds.Functional;
using Battlegrounds.Game.Blueprints.Extensions;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Meta.Annotations;
using Battlegrounds.Modding;

namespace Battlegrounds.Game.Blueprints;

/// <summary>
/// 
/// </summary>
[JsonConverter(typeof(SlotItemBlueprintConverter))]
[GameSpecific(GameCase.CompanyOfHeroes2)]
public sealed class SlotItemBlueprint : Blueprint, IUIBlueprint {

    /// <summary>
    /// The unique PropertyBagGroupdID assigned to this blueprint.
    /// </summary>
    public override BlueprintUID PBGID { get; }

    public override BlueprintType BlueprintType => BlueprintType.IBP;

    public override string Name { get; }

    public UIExtension UI { get; }

    public int SlotWeight { get; }

    public string Weapon { get; }

    public Faction? Army { get; }

    public SlotItemBlueprint(string name, BlueprintUID pbgid, UIExtension ui, Faction? faction, int weight, string wpn) {
        Name = name;
        PBGID = pbgid;
        UI = ui;
        SlotWeight = weight;
        Weapon = wpn;
        Army = faction;
        Game = GameCase.CompanyOfHeroes2;
    }

}

/// <summary>
/// 
/// </summary>
public sealed class SlotItemBlueprintConverter : JsonConverter<SlotItemBlueprint> {

    /// <inheritdoc/>
    public override SlotItemBlueprint Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        Dictionary<string, object> __lookup = new();
        while (reader.Read() && reader.TokenType is not JsonTokenType.EndObject) {
            string prop = reader.ReadProperty() ?? throw new ObjectPropertyNotFoundException();
            __lookup[prop] = prop switch {
                "Display" => UIExtension.FromJson(ref reader),
                "PBGID" => reader.GetUInt64(),
                "Name" => reader.GetString() ?? throw new ObjectPropertyNotFoundException("Name"),
                "ModGUID" => reader.GetString() ?? throw new ObjectPropertyNotFoundException("ModGUID"),
                "SlotSize" => reader.GetInt32(),
                "WPB" => reader.GetString() ?? string.Empty,
                "Army" => reader.GetString() ?? string.Empty,
                _ => throw new NotImplementedException(prop)
            };
        }
        var ui = __lookup.GetCastValueOrDefault("Display", new UIExtension());
        var weight = __lookup.GetCastValueOrDefault("SlotSize", 0);
        string wpn = __lookup.GetCastValueOrDefault("WPB", string.Empty);
        var fac = __lookup.GetCastValueOrDefault("Army", "NULL") is "NULL" ? null : Faction.FromName(__lookup.GetCastValueOrDefault("Army", "NULL"));
        var modguid = __lookup.ContainsKey("ModGUID") ? ModGuid.FromGuid((string)__lookup["ModGUID"]) : ModGuid.BaseGame;
        var pbgid = new BlueprintUID(__lookup.GetCastValueOrDefault("PBGID", 0ul), modguid);
        return new(__lookup.GetCastValueOrDefault("Name", string.Empty), pbgid, ui, fac, weight, wpn) { Game = GameCase.CompanyOfHeroes2 };
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, SlotItemBlueprint value, JsonSerializerOptions options) => writer.WriteStringValue(value.Name);

}