using Battlegrounds.Core.Lobbies;

namespace Battlegrounds.Core.Test.Lobbies;

public static class LobbyFixture {

    public static readonly Guid LobbyId = new Guid("d93ebd78-2067-41eb-945f-d319652a4fc3");

    public static readonly MockLobby.MockPlayer LobbyDefaultHost = new MockLobby.MockPlayer(76561198003529969L, "Test Player");

    public static MockLobby CreateLobby() => CreateLobby(_ => { });

    public static MockLobby CreateLobby(Action<MockLobby.Builder> action) {
        MockLobby.Builder builder = new MockLobby.Builder();
        action(builder);
        MockLobby lobby = new MockLobby(LobbyId, "Test Lobby", builder.Game, LobbyDefaultHost) {
            MatchCompanies = builder.MatchCompanies,
            Scenario = builder.Scenario,
            Settings = builder.Settings,
            Team1 = builder.Team1.Build(),
            Team2 = builder.Team2.Build(),
        };
        return lobby;
    }

}
