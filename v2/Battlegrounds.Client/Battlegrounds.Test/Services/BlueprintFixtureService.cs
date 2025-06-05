using System.Diagnostics.CodeAnalysis;

using Battlegrounds.Models.Blueprints;
using Battlegrounds.Models.Playing;
using Battlegrounds.Services;
using Battlegrounds.Test.Models.Blueprints;

namespace Battlegrounds.Test.Services;

public sealed class BlueprintFixtureService : IBlueprintService {

    public static readonly SquadBlueprint[] COH3_SQUADS = [
        SquadBlueprintFixture.SBP_TOMMY_UK,
    ];

    public bool IsLoaded => true;

    public T2 GetBlueprint<T1, T2>(string blueprintId)
        where T1 : Game
        where T2 : Blueprint {
        if (typeof(T1) == typeof(CoH3)) {
            return GetBlueprint<T2>(CoH3.GameId, blueprintId);
        }
        throw new NotImplementedException($"Blueprints for game {typeof(T1).Name} are not supported in this fixture service.");
    }

    public T GetBlueprint<T>(string gameId, string blueprintId) where T : Blueprint {
        if (typeof(T) == typeof(SquadBlueprint)) {
            if (gameId == CoH3.GameId && COH3_SQUADS.FirstOrDefault(s => s.Id == blueprintId) is T bp) {
                return bp;
            }
            throw new KeyNotFoundException($"Squad blueprint with ID '{blueprintId}' not found for game '{gameId}'.");
        }
        throw new NotImplementedException($"Blueprints of type {typeof(T).Name} are not supported in this fixture service.");
    }

    public void LoadBlueprints() { }

    public bool TryGetBlueprint<T>(string gameId, string blueprintId, [NotNullWhen(true)] out T? blueprint) where T : Blueprint {
        try {             
            var bp = GetBlueprint<T>(gameId, blueprintId);
            blueprint = bp;
            return true;
        } catch (Exception) {
            blueprint = default!;
            return false;
        }
    }

    public bool TryGetBlueprint<T1, T2>(string blueprintId, [NotNullWhen(true)] out T2? blueprint)
        where T1 : Game
        where T2 : Blueprint {
        try {
            var bp = GetBlueprint<T1, T2>(blueprintId);
            blueprint = bp;
            return true;
        } catch (Exception) {
            blueprint = default!;
            return false;
        }
    }

}
