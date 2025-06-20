﻿using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

using Battlegrounds.Errors.Common;
using Battlegrounds.Functional;
using Battlegrounds.Game.Blueprints.Extensions.RequirementTypes;
using Battlegrounds.Game.Gameplay;

namespace Battlegrounds.Game.Blueprints.Extensions;

public enum RequirementReason {
    None,
    Usage,
    UsageAndDisplay,
    Display
}

[JsonConverter(typeof(RequirementExtensionReader))]
public abstract class RequirementExtension {

    public string UIName { get; }

    public RequirementReason Reason { get; }

    public abstract bool IsTrue(Squad squad);

    public RequirementExtension(string ui, RequirementReason reason) {
        UIName = ui;
        Reason = reason;
    }

    public class NoRequirement : RequirementExtension {
        public NoRequirement(string ui, RequirementReason reason) : base(ui, reason) { }
        public override bool IsTrue(Squad squad) => true;
    }

}

public class RequirementExtensionReader : JsonConverter<RequirementExtension> {

    public override void Write(Utf8JsonWriter writer, RequirementExtension value, JsonSerializerOptions options) => throw new NotSupportedException("Cannot serialise requirements.");

    public override RequirementExtension Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {

        Dictionary<string, object> __lookup = new();
        while (reader.Read() && reader.TokenType is not JsonTokenType.EndObject) {
            string prop = reader.ReadProperty() ?? throw new ObjectPropertyNotFoundException();
            __lookup[prop] = prop switch {
                "RequirementType" or "UIText" => reader.GetString() ?? string.Empty,
                "RequirementReason" => GetRequirementReason(reader.GetString() ?? string.Empty),
                "RequirementProperties" => ReadKeyValue(ref reader),
                _ => throw new NotImplementedException(prop)
            };
        }

        return CreateRequirement(__lookup);

    }

    public static RequirementReason GetRequirementReason(string obj)
        => obj switch {
            "display" => RequirementReason.Display,
            "usage" => RequirementReason.Usage,
            "usage_and_display" => RequirementReason.UsageAndDisplay,
            _ => RequirementReason.None
        };

    public static RequirementExtension CreateRequirement(Dictionary<string, object> lookup) => CreateRequirement(lookup.GetCastValueOrDefault("RequirementType", "requires_none"),
            lookup.GetCastValueOrDefault("UIText", string.Empty),
            lookup.GetCastValueOrDefault("RequirementReason", RequirementReason.None),
            lookup.GetCastValueOrDefault("RequirementProperties", new Dictionary<string, object>()));

    public static RequirementExtension CreateRequirement(string type, string ui, RequirementReason reason, Dictionary<string, object> properties)
        => type switch {
            "required_all_in_list" => new RequireAllInList(ui, reason, properties),
            "required_not" => new RequireNot(ui, reason, properties),
            "required_squad_upgrade" => new RequireSquadUpgrade(ui, reason, properties),
            "required_in_state" => new RequireInState(ui, reason, properties),
            "required_interactive_stage" => new RequireInteractivityState(ui, reason, properties),
            "required_slot_item" => new RequireSlotItem(ui, reason, properties),
            "required_in_cover" => new RequireInCover(ui, reason, properties),
            "required_in_territory" => new RequireInTerritory(ui, reason, properties),
            _ => new RequirementExtension.NoRequirement(ui, reason)
        };

    private static Dictionary<string, object?> ReadKeyValue(ref Utf8JsonReader reader) {
        Dictionary<string, object?> __lookup = new();
        while (reader.Read() && reader.TokenType is not JsonTokenType.EndObject) {
            string prop = reader.ReadProperty() ?? throw new ObjectPropertyNotFoundException();
            __lookup[prop] = prop is "RequirementReason" ? GetRequirementReason(GetObject(ref reader) is string s ? s : string.Empty) : GetObject(ref reader);
        }
        return __lookup;
    }

    private static object? GetObject(ref Utf8JsonReader reader)
        => reader.TokenType switch {
            JsonTokenType.False or JsonTokenType.True => reader.GetBoolean(),
            JsonTokenType.String => reader.GetString() ?? string.Empty,
            JsonTokenType.Null => null,
            JsonTokenType.Number => reader.GetSingle(),
            JsonTokenType.StartArray => ReadArray(ref reader),
            JsonTokenType.StartObject => ReadKeyValue(ref reader),
            _ => throw new NotImplementedException()
        };

    private static List<object> ReadArray(ref Utf8JsonReader reader) {
        List<object> list = new();
        while (reader.Read() && reader.TokenType is not JsonTokenType.EndArray) {
            if (GetObject(ref reader) is object o) {
                list.Add(o);
            }
        }
        return list;
    }

}
