using Battlegrounds.Models.Companies;
using Battlegrounds.Models.Playing;
using Battlegrounds.Models.Replays;

namespace Battlegrounds.Models.Lobbies;

public sealed class MultiplayerLobby : ILobby {

    public string Name => throw new NotImplementedException();

    public bool IsHost => throw new NotImplementedException();

    public bool IsActive => throw new NotImplementedException();

    public ISet<Participant> Participants => throw new NotImplementedException();

    public Team Team1 => throw new NotImplementedException();

    public Team Team2 => throw new NotImplementedException();

    public Game Game => throw new NotImplementedException();

    public Dictionary<string, Company> Companies => throw new NotImplementedException();

    public IList<LobbySetting> Settings => throw new NotImplementedException();

    public Map Map => throw new NotImplementedException();

    public string? GetLocalPlayerId() {
        throw new NotImplementedException();
    }

    public (Team? team, int slotId) GetLocalPlayerSlot() {
        throw new NotImplementedException();
    }

    public ValueTask<LobbyEvent?> GetNextEvent() {
        throw new NotImplementedException();
    }

    public Task<LaunchGameResult> LaunchGame() {
        throw new NotImplementedException();
    }

    public Task RemoveAI(Team team, int slotIndex) {
        throw new NotImplementedException();
    }

    public Task ReportMatchResult(ReplayAnalysisResult matchResult) {
        throw new NotImplementedException();
    }

    public Task SendMessage(string channel, string msg) {
        throw new NotImplementedException();
    }

    public Task SetCompany(Team team, int slotId, string id) {
        throw new NotImplementedException();
    }

    public Task<bool> SetMap(Map map) {
        throw new NotImplementedException();
    }

    public Task SetSetting(LobbySetting newSetting) {
        throw new NotImplementedException();
    }

    public Task SetSlotAIDifficulty(Team team, int slotIndex, string difficulty) {
        throw new NotImplementedException();
    }

    public Task ToggleSlotLock(Team team, int slotIndex) {
        throw new NotImplementedException();
    }

    public Task<UploadGamemodeResult> UploadGamemode(string gamemodeLocation) {
        throw new NotImplementedException();
    }

}
