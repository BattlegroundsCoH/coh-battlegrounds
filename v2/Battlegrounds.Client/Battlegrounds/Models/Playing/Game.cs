using System.Diagnostics.CodeAnalysis;

namespace Battlegrounds.Models.Playing;

/// <summary>
/// Represents an abstract base class for a game, providing core properties and methods for identifying and interacting
/// with a game instance.
/// </summary>
/// <remarks>This class defines the essential contract for game-related information and operations, such as
/// retrieving game identifiers, executable paths, and faction details. Derived classes must implement all abstract
/// members to provide game-specific behavior.</remarks>
public abstract class Game {

    /// <summary>
    /// Gets the unique identifier for the instance.
    /// </summary>
    public abstract string Id { get; }
    
    /// <summary>
    /// Gets the Steam application ID for the game.
    /// </summary>
    public abstract int SteamAppId { get; }

    /// <summary>
    /// Gets the display name of the game associated with the current instance.
    /// </summary>
    public abstract string GameName { get; }

    /// <summary>
    /// Gets the full file system path to the application's executable file.
    /// </summary>
    public abstract string AppExecutableFullPath { get; }

    /// <summary>
    /// Gets the file name or path of the archiver executable used to perform archive operations.
    /// </summary>
    public abstract string ArchiverExecutable { get; }

    /// <summary>
    /// Gets the identifiers of the factions associated with this entity.
    /// </summary>
    public abstract string[] FactionIds { get; }

    /// <summary>
    /// Retrieves the alliance information associated with the specified faction identifier.
    /// </summary>
    /// <param name="factionId">The unique identifier of the faction whose alliance information is to be retrieved. Cannot be null or empty.</param>
    /// <returns>A FactionAlliance object representing the alliance details of the specified faction. Returns null if the faction
    /// does not belong to any alliance.</returns>
    public abstract FactionAlliance GetFactionAlliance(string factionId);

    /// <summary>
    /// Retrieves the display name of the faction associated with the specified faction identifier.
    /// </summary>
    /// <param name="factionId">The unique identifier of the faction whose display name is to be retrieved. Cannot be null or empty.</param>
    /// <returns>The display name of the faction corresponding to the specified identifier.</returns>
    public abstract string GetFactionName(string factionId);

    /// <summary>
    /// Attempts to retrieve the display name of a faction based on its unique identifier.
    /// </summary>
    /// <param name="factionId">The unique identifier of the faction whose name is to be retrieved. Cannot be null.</param>
    /// <param name="factionName">When this method returns, contains the display name of the faction if found; otherwise, null. This parameter is
    /// passed uninitialized.</param>
    /// <returns>true if the faction name was found and assigned to factionName; otherwise, false.</returns>
    public abstract bool TryGetFactionName(string factionId, [NotNullWhen(true)] out string? factionName);

    /// <summary>
    /// Retrieves the blueprint identifier for the crew squad associated with the specified faction.
    /// </summary>
    /// <param name="factionId">The unique identifier of the faction for which to retrieve the crew squad blueprint. Cannot be null or empty.</param>
    /// <returns>A string containing the blueprint identifier for the faction's crew squad. Returns null if the faction does not
    /// have an associated crew squad blueprint.</returns>
    public abstract string GetFactionCrewSquadBlueprint(string factionId);

}
