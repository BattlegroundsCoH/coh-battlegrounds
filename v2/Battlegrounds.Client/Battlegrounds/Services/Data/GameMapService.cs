using System.Diagnostics.CodeAnalysis;
using System.IO;

using Battlegrounds.Models;
using Battlegrounds.Models.Playing;
using Battlegrounds.Parsers;

using Microsoft.Extensions.Logging;

namespace Battlegrounds.Services.Data;

public sealed class GameMapService(ScenarioParser<CoH3> scenarioParser, Configuration configuration, ILogger<GameMapService> logger) : IGameMapService {

    private readonly ScenarioParser<CoH3> _scenarioParser = scenarioParser;
    private readonly Configuration _configuration = configuration;
    private readonly ILogger<GameMapService> _logger = logger;

    private readonly Dictionary<string, Dictionary<string, Scenario>> _mapsByGame = new() {
        { CoH3.GameId, new Dictionary<string, Scenario>() }
    };
    
    private bool _isLoaded = false;
    private bool _isLoading = false;

    public async Task LoadMapsAsync() {

        if (_isLoaded) return;

        if (_isLoading) return;

        _isLoading = true;

        _logger.LogInformation("Loading maps for games...");

        try {

            using var coh3scenariosFile = File.OpenRead("Assets/Scenarios/coh3/scenarios.yaml");
            var scenarios = await _scenarioParser.ParseScenarios(coh3scenariosFile);
            _mapsByGame[CoH3.GameId] = scenarios.ToDictionary(x => x.ScenarioName, x => x);

        } catch (Exception ex) { 
            _logger.LogError(ex, "Failed to load maps.");
        }

        _logger.LogInformation("Finished loading maps for games.");

        _isLoaded = true;
        _isLoading = false;

    }

    public Task<Scenario> GetLatestMapAsync(string gameId) {
        if (!_mapsByGame.TryGetValue(gameId, out Dictionary<string, Scenario>? value) || value.Count == 0) {
            throw new InvalidOperationException($"No maps found for game with ID '{gameId}'.");
        }

        if (value.TryGetValue(_configuration.LobbySetups[gameId].ScenarioId, out Scenario? scenario)) {
            return Task.FromResult(scenario);
        }

        var latestMap = value.Values.OrderByDescending(m => m.ScenarioName).FirstOrDefault()
            ?? throw new InvalidOperationException($"No maps found for game with ID '{gameId}'.");
        return Task.FromResult(latestMap);
    }

    public Scenario GetMapByScenarioName(Game game, string newValue) => _mapsByGame[game.Id][newValue];

    public Task<List<Scenario>> GetMapsForGame(string gameId) => Task.FromResult(_mapsByGame[gameId].Values.ToList());

    public Task<List<Scenario>> GetMapsForGame<T>() where T : Game => GetMapsForGame(typeof(T).Name);

    public bool TryGetMapByScenarioName(Game game, string mapId, [NotNullWhen(true)] out Scenario? map) {
        return _mapsByGame[game.Id].TryGetValue(mapId, out map);
    }

}
