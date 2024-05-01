using System.Collections.Frozen;

using Microsoft.Extensions.Logging;

using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;

using Battlegrounds.Core.Games.Blueprints;

namespace Battlegrounds.Core.Services.Standard;

public sealed class BlueprintService(ILogger<BlueprintService> logger) : IBlueprintService {
    
    private readonly ILogger<BlueprintService> _logger = logger;
    private readonly Dictionary<string, GameBlueprintContainer> _gameBlueprints = [];

    private sealed class GameBlueprintContainer {

        public IDictionary<string, SquadBlueprint> Squads { get; set; } = new Dictionary<string, SquadBlueprint>();

        public IDictionary<PropertyBagGroupId, IBlueprint> PropertyBagGroups { get; set; } = new Dictionary<PropertyBagGroupId, IBlueprint>();

        public GameBlueprintContainer FromContainer(BlueprintsContainer container) {
            this.Squads = (container.Squads ?? []).ToDictionary(x => x.Key, x => x.Value.SetReferenceId(x.Key).Build()).ToFrozenDictionary();
            this.PropertyBagGroups = this.Squads.ToDictionary(x => x.Value.Pbgid, x => (IBlueprint)x.Value).ToFrozenDictionary();
            return this;
        }
    }

    private sealed class BlueprintsContainer {
        public string TargetGame { get; set; } = string.Empty;
        public Dictionary<string, SquadBlueprint.Builder> Squads { get; set; } = [];
    }

    public async Task<bool> LoadBlueprintsFromStream(Stream? stream) {

        var deserialiser = new DeserializerBuilder()
            .WithNamingConvention(HyphenatedNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();

        if (stream is null) {
            return false;
        }

        try {
            using var reader = new StreamReader(stream);

            var result = await Task.Run(() => deserialiser.Deserialize<BlueprintsContainer>(reader));
            if (!string.IsNullOrEmpty(result.TargetGame)) {
                _gameBlueprints[result.TargetGame] = (_gameBlueprints.TryGetValue(result.TargetGame, out GameBlueprintContainer? gc) ? gc : new GameBlueprintContainer()).FromContainer(result);
            }

        } catch (Exception ex) {
            _logger.LogError(ex, "Failed loading blueprints from stream");
            return false;
        }

        return true;

    }

}
