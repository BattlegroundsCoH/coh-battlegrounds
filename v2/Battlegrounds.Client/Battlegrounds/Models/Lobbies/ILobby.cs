using Battlegrounds.Models.Companies;
using Battlegrounds.Models.Playing;
using Battlegrounds.Models.Replays;

namespace Battlegrounds.Models.Lobbies;

/// <summary>
/// Represents a multiplayer game lobby for managing participants, teams, game settings, and session state.
/// </summary>
/// <remarks>The ILobby interface provides methods and properties for interacting with a game lobby, including
/// managing participants, configuring teams and settings, launching games, and handling communication between players.
/// It is designed to support both multiplayer and singleplayer scenarios, with certain operations being no-ops in
/// singleplayer mode. Implementations are responsible for enforcing lobby rules, synchronizing state, and coordinating
/// game session lifecycle events.</remarks>
public interface ILobby {

    string Name { get; }

    bool IsHost { get; }
    
    bool IsActive { get; }

    Game Game { get; }

    ISet<Participant> Participants { get; }

    Dictionary<string, Company> Companies { get; }

    Team Team1 { get; }

    Team Team2 { get; }

    IList<LobbySetting> Settings { get; }

    Map Map { get; }

    string? GetLocalPlayerId();

    (Team? team, int slotId) GetLocalPlayerSlot();

    ValueTask<LobbyEvent?> GetNextEvent();
    
    Task<LaunchGameResult> LaunchGame();
    
    Task RemoveAI(Team team, int slotIndex);
    
    ValueTask<bool> ReportMatchResult(ReplayAnalysisResult matchResult);

    Task SendMessage(ChatChannel channel, string msg);

    Task SetCompany(Team team, int slotId, string id);
    
    Task<bool> SetMap(Map map);
    
    Task SetSetting(LobbySetting newSetting);
    
    Task SetSlotAIDifficulty(Team team, int slotIndex, AIDifficulty difficulty);
    
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

}
