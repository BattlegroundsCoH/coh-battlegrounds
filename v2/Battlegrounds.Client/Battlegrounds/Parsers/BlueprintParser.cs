using System.IO;

using Battlegrounds.Models;
using Battlegrounds.Models.Blueprints;
using Battlegrounds.Models.Blueprints.Extensions;
using Battlegrounds.Models.Playing;
using Battlegrounds.Serializers;
using Battlegrounds.Services;

using Microsoft.Extensions.Logging;

using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Battlegrounds.Parsers;

/// <summary>
/// Provides functionality for parsing blueprint data from YAML streams into strongly-typed objects.
/// </summary>
/// <remarks>This class is designed to parse blueprint data, such as squad blueprints, from YAML-formatted input
/// streams. It uses a combination of YAML deserialization and dictionary-based processing to handle complex structures,
/// including extensions and localized strings.</remarks>
/// <typeparam name="G">The type of game context associated with the blueprints. Must derive from <see cref="Game"/>.</typeparam>
public sealed class BlueprintParser<G> where G : Game {

    // Prefer nice yaml format over performance, so we deserialize from yaml and use dictionaries to deserialize the blueprints
    // That also solves the issue of blueprint ids being used as keys in the yaml file, which is not supported by the yaml deserializer
    // As well as extensions being stored as dictionaries in the yaml file, which is not supported by the yaml deserializer
    // Maybe write a generated deserializer in the future, but this is good enough for now

    private class SquadBlueprints {
        public Dictionary<string, Dictionary<string, object>> Squads { get; set; } = [];
    }

    private class UpgradeBlueprints {
        public Dictionary<string, Dictionary<string, object>> Upgrades { get; set; } = [];
    }

    private readonly ILogger<BlueprintParser<G>> _logger;
    private readonly IGameLocaleService _localeService;
    private readonly IDeserializer _deserializer;
    private readonly DictionaryDeserializer _dictionaryDeserializer;

    public BlueprintParser(IGameLocaleService localeService, ILogger<BlueprintParser<G>> logger) {
        _logger = logger;
        _localeService = localeService ?? throw new ArgumentNullException(nameof(localeService));
        _deserializer = new DeserializerBuilder()
            .WithNamingConvention(HyphenatedNamingConvention.Instance)
            .IgnoreFields()
            .IgnoreUnmatchedProperties()
            .Build();
        _dictionaryDeserializer = new();

        // Register a type converter for strings to use the locale service
        _dictionaryDeserializer.RegisterTypeConverter(ConvertLocStr);
    }

    private LocaleString ConvertLocStr(string str) {
        if (str.StartsWith('$')) {
            str = str[1..]; // Remove the leading dollar sign
        }
        return uint.TryParse(str, out uint key) ? _localeService.FromGame<G>(key) : LocaleString.TempString(str);
    }

    public async Task<List<SquadBlueprint>> ParseSquadBlueprints(Stream source) {
        ArgumentNullException.ThrowIfNull(source, nameof(source));

        if (!source.CanRead) {
            throw new ArgumentException("The provided stream is not readable.", nameof(source));
        }

        using StreamReader reader = new(source, leaveOpen: true);

        SquadBlueprints squadBlueprints = await Task.Run(() => _deserializer.Deserialize<SquadBlueprints>(reader));
        List<SquadBlueprint> blueprints = new(squadBlueprints?.Squads.Count ?? 0);
        foreach (var (bpid, bp) in squadBlueprints?.Squads ?? []) {
            bp[nameof(Blueprint.Id)] = bpid; // Ensure the id is set in the dictionary
            blueprints.Add(DeserializeFromDictionary<SquadBlueprint>(bp.Select(x => new KeyValuePair<string, object>(HyphenatedNamingConvention.Instance.Reverse(x.Key), x.Value)).ToDictionary()));
        }

        return blueprints;

    }

    public async Task<List<UpgradeBlueprint>> ParseUpgradeBlueprints(Stream source) {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        if (!source.CanRead) {
            throw new ArgumentException("The provided stream is not readable.", nameof(source));
        }
        
        using StreamReader reader = new(source, leaveOpen: true);

        UpgradeBlueprints upgradeBlueprints = await Task.Run(() => _deserializer.Deserialize<UpgradeBlueprints>(reader));
        List<UpgradeBlueprint> blueprints = new(upgradeBlueprints?.Upgrades.Count ?? 0);
        foreach (var (bpid, bp) in upgradeBlueprints?.Upgrades ?? []) {
            bp[nameof(Blueprint.Id)] = bpid; // Ensure the id is set in the dictionary
            blueprints.Add(DeserializeFromDictionary<UpgradeBlueprint>(bp.Select(x => new KeyValuePair<string, object>(HyphenatedNamingConvention.Instance.Reverse(x.Key), x.Value)).ToDictionary()));
        }
        
        return blueprints;

    }

    private static readonly Dictionary<string, Type> _extensionTypes = new() {
        { nameof(UIExtension), typeof(UIExtension) },
        { "ui", typeof(UIExtension) },
        { nameof(HoldExtension), typeof(HoldExtension) },
        { "hold", typeof(HoldExtension) },
        { nameof(CostExtension), typeof(CostExtension) },
        { "cost", typeof(CostExtension) },
        { nameof(UpgradesExtension), typeof(UpgradesExtension) },
        { "upgrades", typeof(UpgradesExtension) },
        { nameof(VeterancyExtension), typeof(VeterancyExtension) },
        { "veterancy", typeof(VeterancyExtension) },
        { nameof(LoadoutExtension), typeof(LoadoutExtension) },
        { "loadout", typeof(LoadoutExtension) },
        { nameof(TypesExtension), typeof(TypesExtension) },
        { "types", typeof(TypesExtension) },
    };

    private T DeserializeFromDictionary<T>(Dictionary<string, object> data) where T : Blueprint, new() {
        T v = _dictionaryDeserializer.DeserializeFromDictionary<T>(data);
        if (data.TryGetValue("Extensions", out var extensionsObj) && extensionsObj is Dictionary<object, object> extensions) {
            foreach (var (extName, extData) in extensions) {
                if (!_extensionTypes.TryGetValue(extName.ToString()!, out var extType)) {
                    _logger.LogWarning("Unknown extension type: {ExtensionName}, skipping deserialization.", extName);
                    continue;
                }
                if (extData is Dictionary<object, object> extObjData) {
                    Dictionary<string, object> extDataDictionary = extObjData.Select(x => new KeyValuePair<string, object>(HyphenatedNamingConvention.Instance.Reverse(x.Key.ToString()!), x.Value)).ToDictionary();
                    var value = _dictionaryDeserializer.DeserializeFromDictionary(extType, extDataDictionary);
                    v.AddExtension((BlueprintExtension)value);
                } else {
                    _logger.LogWarning("Extension data for {ExtensionName} is not a dictionary, skipping deserialization.", extName);
                    continue;
                }
            }
        }
        v.Freeze(); // Freeze the blueprint after deserialization to prevent further modifications
        return v;
    }

}
