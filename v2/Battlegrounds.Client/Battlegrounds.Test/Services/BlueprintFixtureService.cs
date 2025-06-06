using System.Diagnostics.CodeAnalysis;

using Battlegrounds.Models.Blueprints;
using Battlegrounds.Models.Playing;
using Battlegrounds.Services;
using Battlegrounds.Test.Models.Blueprints;

namespace Battlegrounds.Test.Services;

public sealed class BlueprintFixtureService : IBlueprintService { // Test fixture for IBlueprintService

    public static readonly SquadBlueprint[] COH3_SQUADS = [
        SquadBlueprintFixture.SBP_TOMMY_UK,
        SquadBlueprintFixture.SBP_HALFTRACK_M3_UK,
        SquadBlueprintFixture.SBP_MATILDA_UK,
        SquadBlueprintFixture.SBP_CRUSADER_UK,
        SquadBlueprintFixture.SBP_PANZERGRENADIER_AK,
        SquadBlueprintFixture.SBP_HALFTRACK_250_AK,
        SquadBlueprintFixture.SBP_PANZER_III_AK
    ];

    public static readonly UpgradeBlueprint[] COH3_UPGRADES = [
        UpgradeBlueprintFixture.UPG_LMG_BREN,
        UpgradeBlueprintFixture.UPG_LMG_PANZERGRENADIER_AK
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
        } else if (typeof(T) == typeof(UpgradeBlueprint)) {
            if (gameId == CoH3.GameId && COH3_UPGRADES.FirstOrDefault(u => u.Id == blueprintId) is T bp) {
                return bp;
            }
            throw new KeyNotFoundException($"Upgrade blueprint with ID '{blueprintId}' not found for game '{gameId}'.");
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

    public ICollection<Blueprint> GetBlueprintsForGame(string gameId) => gameId switch {
        CoH3.GameId => [.. COH3_SQUADS, .. COH3_UPGRADES],
        _ => throw new KeyNotFoundException($"Blueprint repository for game {gameId} not found.")
    };

    public ICollection<Blueprint> GetBlueprintsForGame<T>() where T : Game => GetBlueprintsForGame(GetGameId<T>());

    public ICollection<T2> GetBlueprintsForGame<T1, T2>()
        where T1 : Game
        where T2 : Blueprint => GetBlueprintsForGame<T2>(GetGameId<T1>());

    public ICollection<T> GetBlueprintsForGame<T>(string gameId) where T : Blueprint
        => [.. GetBlueprintsForGame(gameId).OfType<T>()];

    private static string GetGameId<T>() where T : Game {
        return typeof(T).Name;
    }


}
