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
/// Representation of a <see cref="Blueprint"/> with critical blueprint specific values. Inherits from <see cref="Blueprint"/>. This class cannot be inherited.
/// </summary>
[JsonConverter(typeof(CriticalBlueprintConverter))]
public sealed class CriticalBlueprint : Blueprint {

    /// <summary>
    /// The unique PropertyBagGroupdID assigned to this blueprint.
    /// </summary>
    public override BlueprintUID PBGID { get; }

    /// <summary>
    /// 
    /// </summary>
    public override BlueprintType BlueprintType => BlueprintType.CBP;

    /// <summary>
    /// 
    /// </summary>
    public override string Name { get; }

    /// <summary>
    /// 
    /// </summary>
    public UIExtension Display { get; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="name"></param>
    /// <param name="ui"></param>
    public CriticalBlueprint(string name, BlueprintUID uid, UIExtension ui) {
        this.PBGID = uid;
        this.Name = name;
        this.Display = ui;
    }

}

/// <summary>
/// Converter for converting a string into an <see cref="CriticalBlueprint"/>.
/// </summary>
public sealed class CriticalBlueprintConverter : JsonConverter<CriticalBlueprint> {

    public override CriticalBlueprint Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        Dictionary<string, object> __lookup = new();
        while (reader.Read() && reader.TokenType is not JsonTokenType.EndObject) {
            string prop = reader.ReadProperty() ?? throw new ObjectPropertyNotFoundException();
            __lookup[prop] = prop switch {
                "Display" => UIExtension.FromJson(ref reader),
                "PBGID" => reader.GetUInt64(),
                "Name" => reader.GetString() ?? throw new ObjectPropertyNotFoundException("Name"),
                "ModGUID" => reader.GetString() ?? throw new ObjectPropertyNotFoundException("ModGUID") ,
                _ => throw new NotImplementedException(prop)
            };
        }
        var ui = __lookup.GetCastValueOrDefault("Display", new UIExtension());
        var modguid = __lookup.ContainsKey("ModGUID") ? ModGuid.FromGuid((string)__lookup["ModGUID"]) : ModGuid.BaseGame;
        var pbgid = new BlueprintUID(__lookup.GetCastValueOrDefault("PBGID", 0ul), modguid);
        return new(__lookup.GetCastValueOrDefault("Name", string.Empty), pbgid, ui);
    }

    public override void Write(Utf8JsonWriter writer, CriticalBlueprint value, JsonSerializerOptions options) => writer.WriteStringValue(value.Name);

}

