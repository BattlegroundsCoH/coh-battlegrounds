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

    Task LoadBlueprints();
    
    ICollection<Blueprint> GetBlueprintsForGame(string gameId);

    ICollection<Blueprint> GetBlueprintsForGame<T>() where T : Game;

    ICollection<T2> GetBlueprintsForGame<T1, T2>() where T1 : Game where T2 : Blueprint;

    ICollection<T> GetBlueprintsForGame<T>(string gameId) where T : Blueprint;

    IBlueprintRepository GetBlueprintRepositoryForGame<T>() where T : Game;

}

/// <summary>
/// Represents a repository that provides access to blueprints by their unique identifiers.
/// </summary>
/// <remarks>Implementations of this interface are responsible for retrieving blueprint instances of a specified
/// type. The repository may support various storage mechanisms or caching strategies. Thread safety and performance
/// characteristics depend on the specific implementation.</remarks>
public interface IBlueprintRepository {
    
    /// <summary>
    /// Retrieves a blueprint of the specified type by its unique identifier.
    /// </summary>
    /// <typeparam name="T">The type of blueprint to retrieve. Must inherit from Blueprint.</typeparam>
    /// <param name="blueprintId">The unique identifier of the blueprint to retrieve. Cannot be null or empty.</param>
    /// <returns>An instance of type T representing the requested blueprint, or null if no matching blueprint is found.</returns>
    T GetBlueprint<T>(string blueprintId) where T : Blueprint;

}
