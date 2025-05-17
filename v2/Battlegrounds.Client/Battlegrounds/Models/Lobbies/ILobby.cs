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

    (Team? team, int slotId) GetLocalPlayerSlot();

    ValueTask<LobbyEvent?> GetNextEvent();
    
    Task<LaunchGameResult> LaunchGame();
    
    Task ReportMatchResult(ReplayAnalysisResult matchResult);
    
    Task SetCompany(Team team, int slotId, string id);

    Task<UploadGamemodeResult> UploadGamemode(string gamemodeLocation);

}
