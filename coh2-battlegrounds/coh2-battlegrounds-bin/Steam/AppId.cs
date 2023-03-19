namespace Battlegrounds.Steam;

/// <summary>
/// Specifies the type of application ID.
/// </summary>
public enum AppIdType {

    /// <summary>
    /// The type of the application ID is unspecified.
    /// </summary>
    Unspecified,

    /// <summary>
    /// The application ID represents a game.
    /// </summary>
    Game,

    /// <summary>
    /// The application ID represents a DLC (Downloadable Content) of a game.
    /// </summary>
    DLC

}

/// <summary>
/// Represents an application ID, consisting of an identifier, a type, and an optional parent identifier.
/// </summary>
/// <param name="Identifier">The identifier of the application.</param>
/// <param name="AppType">The type of the application.</param>
/// <param name="ParentIdentifier">The identifier of the parent application (if any).</param>
public record struct AppId(uint Identifier, AppIdType AppType, uint? ParentIdentifier) {

    /// <summary>
    /// Implicitly converts the AppId to its identifier.
    /// </summary>
    /// <param name="id">The AppId to convert.</param>
    public static implicit operator uint(AppId id) => id.Identifier;

    /// <summary>
    /// Creates a new AppId with the specified identifier and type of "Game".
    /// </summary>
    /// <param name="identifier">The identifier of the game.</param>
    /// <returns>An AppId object representing the game.</returns>
    public static AppId Game(uint identifier) => new(identifier, AppIdType.Game, null);

    /// <summary>
    /// Creates a new AppId with the specified identifier, type of "DLC", and parent identifier.
    /// </summary>
    /// <param name="identifier">The identifier of the DLC.</param>
    /// <param name="parentIdentifier">The identifier of the parent game.</param>
    /// <returns>An AppId object representing the DLC.</returns>
    public static AppId DLC(uint identifier, uint parentIdentifier) => new AppId(identifier, AppIdType.DLC, parentIdentifier);

}
