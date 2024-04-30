using Battlegrounds.Core.Games;
using Battlegrounds.Core.Games.Scenarios;

using Microsoft.Extensions.Logging;

using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;

namespace Battlegrounds.Core.Services.Standard;

public class ScenarioService(ILogger<ScenarioService> logger) : IScenarioService {

    private readonly ILogger<ScenarioService> _logger = logger;
    private readonly Dictionary<string, IList<IScenario>> _scenarios = [];

    private class ScenarioList {
        public record Scenario(string Name, string Description, int PlayerCount) { public Scenario() : this("", "", 0) { } }
        public string TargetGame { get; set; } = "coh";
        public Dictionary<string, Scenario> Scenarios { get; set; } = [];
    }

    public IScenario? GetScenario(IGame game, string scenarioFilename) {
        if (_scenarios.TryGetValue(game.Name, out var scenarios)) {
            return scenarios.FirstOrDefault(x => x.FileName == scenarioFilename);
        }
        return null;
    }

    public IList<IScenario> GetScenarios(IGame game) => _scenarios.TryGetValue(game.Name, out var scenarios) ? scenarios : [];

    public async Task<bool> LoadFromStreamAsync(Stream? stream) {

        var deserialiser = new DeserializerBuilder()
            .WithNamingConvention(HyphenatedNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();

        if (stream is null) {
            return false;
        }

        try {
            using var reader = new StreamReader(stream);

            var result = await Task.Run(() => deserialiser.Deserialize<ScenarioList>(reader));
            if (!string.IsNullOrEmpty(result.TargetGame)) {
                _scenarios[result.TargetGame] = result.Scenarios.Select(x => new BaseScenario { 
                    Name = x.Value.Name,
                    FileName = x.Key,
                    Description = x.Value.Description, 
                    PlayerCount = x.Value.PlayerCount
                }).ToArray();
            }

        } catch (Exception ex) {
            _logger.LogError(ex, "Failed loading scenarios from stream");
            return false;
        }

        return true;

    }

}
