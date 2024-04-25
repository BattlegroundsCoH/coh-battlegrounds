using Battlegrounds.Core.Games.Scenarios;

namespace Battlegrounds.Core.Lobbies;

public interface ILobby {

    Guid Guid { get; }

    string Name { get; }

    bool IsHost { get; }

    ILobbyTeam Team1 { get; }

    ILobbyTeam Team2 { get; }

    IDictionary<string, string> Settings { get; }

    ulong LocalPlayerId { get; }

    void SetUpdateCallback(Action callback);

    void SetChatCallback(Action<ILobbyChatMessage> callback);

    Task SetTeamNames(string team1, string team2);

    Task MoveToSlot(int team, int slot);

    Task SetDifficulty(int team, int slot, int aiDifficulty);

    Task SetCompany(int team, int slot, string company);

    Task SetLocked(int team, int slot, bool isLocked);

    Task SetSetting(string setting, string value);

    Task SetScenario(IScenario scenario);

    Task LaunchMatch();

    Task Leave();

}
