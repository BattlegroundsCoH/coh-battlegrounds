using System.Diagnostics.CodeAnalysis;

using Battlegrounds.Models.Blueprints;
using Battlegrounds.Models.Playing;

namespace Battlegrounds.Services;

public interface IBlueprintService {
    
    bool IsLoaded { get; }

    T2 GetBlueprint<T1, T2>(string blueprintId) 
        where T1 : Game 
        where T2 : Blueprint;

    T GetBlueprint<T>(string gameId, string blueprintId) where T : Blueprint;

    bool TryGetBlueprint<T>(string gameId, string blueprintId, [NotNullWhen(true)] out T? blueprint) where T : Blueprint;

    bool TryGetBlueprint<T1, T2>(string blueprintId, [NotNullWhen(true)] out T2? blueprint) 
        where T1 : Game 
        where T2 : Blueprint;

    void LoadBlueprints();
    
    ICollection<Blueprint> GetBlueprintsForGame(string gameId);

    ICollection<Blueprint> GetBlueprintsForGame<T>() where T : Game;

    ICollection<T2> GetBlueprintsForGame<T1, T2>() where T1 : Game where T2 : Blueprint;

    ICollection<T> GetBlueprintsForGame<T>(string gameId) where T : Blueprint;

}
