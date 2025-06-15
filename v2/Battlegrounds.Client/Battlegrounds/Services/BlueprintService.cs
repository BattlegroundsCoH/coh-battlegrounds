using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;

using Battlegrounds.Models.Blueprints;
using Battlegrounds.Models.Playing;
using Battlegrounds.Parsers;

using Microsoft.Extensions.Logging;

using Serilog;

namespace Battlegrounds.Services;

/// <summary>
/// Provides functionality for managing and retrieving blueprints associated with different games.
/// </summary>
/// <remarks>The <see cref="BlueprintService"/> class is responsible for loading, organizing, and accessing
/// blueprints for various games. Blueprints are categorized by game and type, and can be retrieved using specific
/// methods based on game identifiers and blueprint types. This service supports asynchronous loading of blueprints from
/// external files and ensures thread-safe access to blueprint repositories.  Use this service to retrieve blueprints by
/// ID, check for their existence, or access collections of blueprints for a specific game. The service also provides
/// properties to check the loading state of blueprints.</remarks>
/// <param name="localeService"></param>
/// <param name="logger"></param>
public sealed class BlueprintService(IGameLocaleService localeService, ILogger<BlueprintService> logger) : IBlueprintService {

    private class BlueprintRepository {

        private readonly Dictionary<string, Dictionary<string, Blueprint>> _blueprints = [];

        public Dictionary<string, Dictionary<string, Blueprint>> Blueprints {
            get => _blueprints;
            init => _blueprints = value;
        }

        public T GetBlueprint<T>(string blueprintId) where T : Blueprint {
            string blueprintType = typeof(T).Name;
            if (_blueprints.TryGetValue(blueprintType, out var blueprints)) {
                if (blueprints.TryGetValue(blueprintId, out var blueprint)) {
                    return (T)blueprint;
                }
            }
            throw new KeyNotFoundException($"Blueprint of type {blueprintType} with ID {blueprintId} not found.");
        }

        public bool TryGetBlueprint<T>(string blueprintId, [NotNullWhen(true)] out T? blueprint) where T : Blueprint {
            string blueprintType = typeof(T).Name;
            if (_blueprints.TryGetValue(blueprintType, out var blueprints)) {
                if (blueprints.TryGetValue(blueprintId, out var foundBlueprint)) {
                    blueprint = (T)foundBlueprint;
                    return true;
                }
            }
            blueprint = null;
            return false;
        }

    }

    private readonly ILogger<BlueprintService> _logger = logger;
    private readonly IGameLocaleService _localeService = localeService;
    private readonly Dictionary<string, BlueprintRepository> _gameBlueprintRepositories = [];

    private bool _isLoaded = false;
    private bool _isLoading = false;

    public bool IsLoaded => _isLoaded;

    public bool IsLoading => _isLoading;

    public T2 GetBlueprint<T1, T2>(string blueprintId)
        where T1 : Game
        where T2 : Blueprint {
        return GetBlueprintRepository<T1>().GetBlueprint<T2>(blueprintId);
    }

    public T GetBlueprint<T>(string gameId, string blueprintId) where T : Blueprint {
        if (_gameBlueprintRepositories.TryGetValue(gameId, out var repository)) {
            return repository.GetBlueprint<T>(blueprintId);
        }
        throw new KeyNotFoundException($"Blueprint repository for game {gameId} not found.");
    }

    public bool TryGetBlueprint<T>(string gameId, string blueprintId, [NotNullWhen(true)] out T? blueprint) where T : Blueprint {
        if (_gameBlueprintRepositories.TryGetValue(gameId, out var repository)) {
            if (repository.TryGetBlueprint<T>(blueprintId, out var foundBlueprint)) {
                blueprint = foundBlueprint;
                return true;
            }
        }
        blueprint = null;
        return false;
    }

    public bool TryGetBlueprint<T1, T2>(string blueprintId, [NotNullWhen(true)] out T2? blueprint)
        where T1 : Game
        where T2 : Blueprint {
        if (_gameBlueprintRepositories.TryGetValue(GetGameId<T1>(), out var repository)) {
            return repository.TryGetBlueprint(blueprintId, out blueprint);
        }
        blueprint = null;
        return false;
    }

    private BlueprintRepository GetBlueprintRepository<T1>()
        where T1 : Game {
        var gameId = GetGameId<T1>();
        if (!_gameBlueprintRepositories.TryGetValue(gameId, out var repository)) {
            throw new KeyNotFoundException($"Blueprint repository for game {gameId} not found.");
        }
        return repository;
    }

    private async Task<List<T>> LoadAndLogBlueprints<T>(string path, Func<Stream, Task<List<T>>> loader) where T : Blueprint {
        if (!File.Exists(path)) {
            throw new FileNotFoundException($"Blueprint file not found: {path}");
        }
        var stopwatch = Stopwatch.StartNew();
        List<T> blueprints;
        try {
            using var fs = File.OpenRead(path);
            blueprints = await loader(fs);
        } catch (Exception ex) {
            _logger.LogError(ex, "Failed to load blueprints from {Path}.", path);
            throw new InvalidOperationException($"Failed to load blueprints from {path}.", ex);
        }
        stopwatch.Stop();
        _logger.LogInformation("Loaded {Count} blueprints from {Path} in {ElapsedMilliseconds} ms.", blueprints.Count, path, stopwatch.ElapsedMilliseconds);
        return blueprints;
    }

    /// <summary>
    /// Loads blueprints for the game and initializes the blueprint repositories.
    /// </summary>
    /// <remarks>This method loads blueprints from predefined YAML files, parses them, and organizes them into
    /// repositories for use within the application. If the blueprints are already loaded, the method logs a message and
    /// exits.  The method performs asynchronous operations to load and parse blueprints, and waits for all tasks to
    /// complete before updating the repositories. If an error occurs during the loading process, the exception is
    /// logged and rethrown.</remarks>
    public async void LoadBlueprints() {

        if (_isLoaded) {
            _logger.LogInformation("Blueprints are already loaded.");
            return;
        }

        if (_isLoading) {
            _logger.LogInformation("Blueprints are currently loading. Please wait until the loading is complete.");
            return;
        }

        _isLoading = true;

        try {

            // Create a new blueprint parser for CoH3
            var coh3BpParser = new BlueprintParser<CoH3>(_localeService, LoggerFactory.Create(x => x.AddSerilog()).CreateLogger<BlueprintParser<CoH3>>());

            // Load blueprints for CoH3 from YAML files
            var coh3SbpTask = LoadAndLogBlueprints("Assets/Factions/CoH3/sbps.yaml", coh3BpParser.ParseSquadBlueprints);
            var coh3UpgTask = LoadAndLogBlueprints("Assets/Factions/CoH3/upgs.yaml", coh3BpParser.ParseUpgradeBlueprints);

            // Wait for all tasks to complete before proceeding
            await Task.WhenAll(coh3SbpTask, coh3UpgTask);

            // Add blueprints for CoH3
            _gameBlueprintRepositories.Add(CoH3.GameId, new BlueprintRepository() {
                Blueprints = new Dictionary<string, Dictionary<string, Blueprint>>() {
                    { nameof(SquadBlueprint), (await coh3SbpTask).ToDictionary(k => k.Id, v => v as Blueprint) },
                    { nameof(UpgradeBlueprint), (await coh3UpgTask).ToDictionary(k => k.Id, v => v as Blueprint)},
                    { nameof(SlotItemBlueprint), [] } // No slot items for CoH3, but we need it here to avoid NPEs later
                }
            });

            _isLoaded = true;

        } catch (Exception e) {
            _logger.LogError(e, "An error occurred while loading blueprints.");
            throw;
        } finally {
            _isLoading = false;
        }
    }

    public ICollection<Blueprint> GetBlueprintsForGame(string gameId) {
        if (_gameBlueprintRepositories.TryGetValue(gameId, out var repository)) {
            return [.. repository.Blueprints.Values.SelectMany(bp => bp.Values)];
        }
        throw new KeyNotFoundException($"Blueprint repository for game {gameId} not found.");
    }

    public ICollection<Blueprint> GetBlueprintsForGame<T>() where T : Game => GetBlueprintsForGame(GetGameId<T>());

    public ICollection<T2> GetBlueprintsForGame<T1, T2>()
        where T1 : Game
        where T2 : Blueprint => GetBlueprintsForGame<T2>(GetGameId<T1>());

    public ICollection<T> GetBlueprintsForGame<T>(string gameId) where T : Blueprint 
        => [.. GetBlueprintsForGame(gameId).OfType<T>()];

    private static string GetGameId<T>() where T : Game {
        if (typeof(T) == typeof(CoH3)) {
            return CoH3.GameId;
        }
        throw new NotSupportedException($"Game type {typeof(T).Name} is not supported.");
    }

}
