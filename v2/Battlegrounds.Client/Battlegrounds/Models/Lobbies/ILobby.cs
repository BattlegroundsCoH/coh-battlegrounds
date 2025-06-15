using Battlegrounds.Models.Companies;
using Battlegrounds.Models.Playing;
using Battlegrounds.Models.Replays;

namespace Battlegrounds.Models.Lobbies;

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

    Task SendMessage(string channel, string msg);

    Task SetCompany(Team team, int slotId, string id);
    
    Task<bool> SetMap(Map map);
    
    Task SetSetting(LobbySetting newSetting);
    
    Task SetSlotAIDifficulty(Team team, int slotIndex, AIDifficulty difficulty);
    
    Task ToggleSlotLock(Team team, int slotIndex);
    
    Task<UploadGamemodeResult> UploadGamemode(string gamemodeLocation);

}
