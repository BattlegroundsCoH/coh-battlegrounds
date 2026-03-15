using Battlegrounds.Models.Companies;
using Battlegrounds.Models.Playing;
using Battlegrounds.Models.Replays;

namespace Battlegrounds.Models.Lobbies;

public enum EndMatchReason {
    Unknown,
    ScarError,
    GameCancelled,
    MatchEndedInSuccess,
}

/// <summary>
/// Represents a multiplayer game lobby for managing participants, teams, game settings, and session state.
/// </summary>
/// <remarks>The ILobby interface provides methods and properties for interacting with a game lobby, including
/// managing participants, configuring teams and settings, launching games, and handling communication between players.
/// It is designed to support both multiplayer and singleplayer scenarios, with certain operations being no-ops in
/// singleplayer mode. Implementations are responsible for enforcing lobby rules, synchronizing state, and coordinating
/// game session lifecycle events.</remarks>
public interface ILobby {

    /// <summary>
    /// Gets the name associated with the current lobby.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets a value indicating whether the current user is the host of the session.
    /// </summary>
    bool IsHost { get; }
    
    /// <summary>
    /// Gets a value indicating whether the current instance is active.
    /// </summary>
    bool IsActive { get; }

    /// <summary>
    /// Gets a value indicating whether the user is marked as ready in the lobby. 
    /// This indicates that the user has completed their setup and is prepared to start the match.
    /// </summary>
    bool IsReady { get; }

    /// <summary>
    /// Gets the current game instance associated with this context.
    /// </summary>
    Game Game { get; }

    /// <summary>
    /// Gets the collection of participants associated with the current instance.
    /// </summary>
    ISet<Participant> Participants { get; }

    /// <summary>
    /// Gets the collection of companies indexed by their unique identifiers.
    /// </summary>
    Dictionary<string, Company> Companies { get; }

    /// <summary>
    /// Gets the first team participating in the match.
    /// </summary>
    /// <remarks>
    /// This is usually the Allies team.
    /// </remarks>
    Team Team1 { get; }

    /// <summary>
    /// Gets the second team participating in the match.
    /// </summary>
    /// <remarks>
    /// This is usually the Axis team.
    /// </remarks>
    Team Team2 { get; }

    /// <summary>
    /// Gets the collection of settings associated with the lobby.
    /// </summary>
    /// <remarks>The returned list contains all current settings for the lobby. Modifications to the list or
    /// its elements may affect the lobby's configuration, depending on the implementation.</remarks>
    IList<LobbySetting> Settings { get; }

    /// <summary>
    /// Gets the map associated with the current context.
    /// </summary>
    Map Map { get; }

    /// <summary>
    /// Begins the asynchronous process of starting a match. Notifies the server to initiate the match start sequence, which may involve validating lobby state, synchronizing participants.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task completes when the match has been successfully
    /// started.</returns>
    Task BeginMatch();

    /// <summary>
    /// Ends the current match and performs any necessary cleanup operations asynchronously.
    /// </summary>
    /// <param name="reason">The reason for ending the match. This parameter provides context for why the match is being ended, such as a game cancellation or an error condition.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task EndMatch(EndMatchReason reason);

    /// <summary>
    /// Retrieves the unique identifier of the local player, if available.
    /// </summary>
    /// <returns>A string containing the local player's unique identifier, or null if the local player is not available or not
    /// signed in.</returns>
    string? GetLocalPlayerId();

    /// <summary>
    /// Gets the team and slot identifier for the local player, if available.
    /// </summary>
    /// <remarks>Use this method to determine the local player's current team and slot assignment in
    /// multiplayer scenarios. If the local player is not assigned to any team, the returned team value will be
    /// null.</remarks>
    /// <returns>A tuple containing the local player's team and slot identifier. The team is null if the local player is not
    /// assigned to a team. The slot identifier is an integer representing the player's position within the team.</returns>
    (Team? team, int slotId) GetLocalPlayerSlot();

    /// <summary>
    /// Retrieves the next available lobby event, if any, in an asynchronous operation.
    /// </summary>
    /// <remarks>
    /// This method allows you to asynchronously retrieve the next event that occurs in the lobby. It is useful for
    /// handling lobby events in a non-blocking manner. Any visual updates or state changes that need to occur in response to lobby events should be performed after awaiting this method. 
    /// If no events are available, the result will be null.
    /// </remarks>
    /// <returns>A value task that represents the asynchronous operation. The result contains the next available <see
    /// cref="LobbyEvent"/>, or <see langword="null"/> if no event is available.</returns>
    ValueTask<LobbyEvent?> GetNextEvent();

    /// <summary>
    /// Retrieves the participant associated with the specified identifier.
    /// </summary>
    /// <param name="participantId">The unique identifier of the participant to retrieve. Cannot be null or empty.</param>
    /// <returns>A <see cref="Participant"/> object representing the participant if found; otherwise, <see langword="null"/>.</returns>
    Participant? GetParticipant(string participantId);
    
    /// <summary>
    /// Gets the number of real (non-bot) players currently present.
    /// </summary>
    /// <returns>The total count of real players. Returns 0 if no real players are present.</returns>
    int GetRealPlayersCount();
    
    /// <summary>
    /// Launches the game process asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains the result of the game launch.</returns>
    Task<LaunchGameResult> LaunchGame();

    /// <summary>
    /// Publishes a system message asynchronously to all connected clients or subscribers.
    /// </summary>
    /// <param name="message">The content of the system message to be published. Cannot be null or empty.</param>
    /// <returns>A ValueTask that represents the asynchronous publish operation.</returns>
    ValueTask PublishSystemMessage(string message);

    /// <summary>
    /// Removes the AI-controlled participant from the specified slot in the given team.
    /// </summary>
    /// <param name="team">The team from which the AI participant will be removed. Cannot be null.</param>
    /// <param name="slotIndex">The zero-based index of the slot from which to remove the AI participant. Must be within the valid range of slot
    /// indices for the team.</param>
    /// <returns>A task that represents the asynchronous remove operation.</returns>
    Task RemoveAI(Team team, int slotIndex);
    
    /// <summary>
    /// Reports the result of a replay match analysis asynchronously.
    /// </summary>
    /// <param name="matchResult">The analysis result of the replay match to be reported. Cannot be null.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the match result
    /// was reported successfully; otherwise, <see langword="false"/>.</returns>
    ValueTask<bool> ReportMatchResult(ReplayAnalysisResult matchResult);

    /// <summary>
    /// Sends a message to the specified chat channel asynchronously.
    /// </summary>
    /// <param name="channel">The chat channel to which the message will be sent. Cannot be null.</param>
    /// <param name="msg">The message text to send. Cannot be null or empty.</param>
    /// <returns>A task that represents the asynchronous send operation.</returns>
    Task SendMessage(ChatChannel channel, string msg);

    /// <summary>
    /// Assigns a company to the specified slot within the given team.
    /// </summary>
    /// <param name="team">The team to which the company will be assigned. Cannot be null.</param>
    /// <param name="slotId">The identifier of the slot within the team where the company will be set.</param>
    /// <param name="id">The unique identifier of the company to assign to the slot. Cannot be null or empty.</param>
    /// <param name="faction">The faction to assign to the slot. Cannot be null or empty.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task SetCompany(Team team, int slotId, string id, string faction);
    
    /// <summary>
    /// Asynchronously sets the current map to the specified value.
    /// </summary>
    /// <param name="map">The map to set as the current map. Cannot be null.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the map was set
    /// successfully; otherwise, <see langword="false"/>.</returns>
    Task<bool> SetMap(Map map);
    
    /// <summary>
    /// Asynchronously updates the lobby configuration with the specified setting.
    /// </summary>
    /// <param name="newSetting">The new lobby setting to apply. Cannot be null.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task SetSetting(LobbySetting newSetting);
    
    /// <summary>
    /// Sets the AI difficulty level for a specific slot on the given team.
    /// </summary>
    /// <param name="team">The team containing the slot for which to set the AI difficulty.</param>
    /// <param name="slotIndex">The zero-based index of the slot within the team whose AI difficulty will be set.</param>
    /// <param name="difficulty">The AI difficulty level to assign to the specified slot.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task SetSlotAIDifficulty(Team team, int slotIndex, AIDifficulty difficulty);

    /// <summary>
    /// Sets the faction for the specified slot on the given team.
    /// </summary>
    /// <param name="team">The team whose slot faction is to be set.</param>
    /// <param name="slotIndex">The zero-based index of the slot to update. Must be within the valid range of slot indices for the team.</param>
    /// <param name="faction">The name of the faction to assign to the slot, or null to clear the faction assignment.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task SetSlotFaction(Team team, int slotIndex, string? faction);

    /// <summary>
    /// Toggles the lock state of a slot within the specified team asynchronously.
    /// </summary>
    /// <param name="team">The team containing the slot to be locked or unlocked. Cannot be null.</param>
    /// <param name="slotIndex">The zero-based index of the slot to toggle. Must be within the valid range of slots for the team.</param>
    /// <returns>A task that represents the asynchronous toggle operation.</returns>
    Task ToggleSlotLock(Team team, int slotIndex);

    /// <summary>
    /// Asynchronously uploads the gamemode file to the server and allows participants to download it.
    /// </summary>
    /// <remarks>
    /// This is a NO-OP if the lobby is singleplayer.
    /// </remarks>
    /// <param name="gamemodeLocation">The absolute path of the local gamemode file to upload.</param>
    /// <returns>The result of the gamemode upload operation.</returns>
    ValueTask<UploadGamemodeResult> UploadGamemode(string gamemodeLocation);

    /// <summary>
    /// Asynchronously waits until all players in the lobby have the required game mode set.
    /// </summary>
    /// <remarks>
    /// This is a NO-OP if the lobby is singleplayer.
    /// </remarks>
    /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if all players have
    /// the required game mode; otherwise, <see langword="false"/>.</returns>
    ValueTask<bool> WaitForAllPlayersHaveGamemode();

    /// <summary>
    /// Marks the current instance as ready or not ready for operation.
    /// </summary>
    /// <param name="isReady">A value indicating whether the instance should be marked as ready (<see langword="true"/>) or not ready (<see
    /// langword="false"/>).</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task MarkReady(bool isReady);

    /// <summary>
    /// Removes a player from the specified team at the given slot index.
    /// </summary>
    /// <param name="team">The team from which the player will be removed. Cannot be null.</param>
    /// <param name="slotIndex">The zero-based index of the player slot to remove. Must be within the valid range of player slots for the team.</param>
    /// <returns>A task that represents the asynchronous operation of removing the player.</returns>
    Task KickPlayer(Team team, int slotIndex);
    
    /// <summary>
    /// Asynchronously retrieves the results of the completed match, if available.
    /// </summary>
    /// <remarks>Callers should check for a <see langword="null"/> result to determine if the match has not
    /// yet finished. This method does not block waiting for the match to complete.</remarks>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="MatchOverData"/> object
    /// with the match results if the match has concluded; otherwise, <see langword="null"/>.</returns>
    Task<MatchOverData?> GetMatchResults();
    
    /// <summary>
    /// Moves the local player to the specified team to the given slot asynchronously.
    /// </summary>
    /// <param name="team">The team to be moved to. Cannot be null.</param>
    /// <param name="slotIndex">The zero-based index of the slot to move the team to. Must be within the valid range of available slots.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task MoveToSlot(Team team, int slotIndex);

}
