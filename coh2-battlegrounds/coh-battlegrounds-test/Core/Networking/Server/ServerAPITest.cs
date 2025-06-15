using Battlegrounds.Game;
using Battlegrounds.Networking.LobbySystem;
using Battlegrounds.Networking.LobbySystem.Factory;
using Battlegrounds.Steam;

namespace Battlegrounds.Testing.Core.Networking.Server;

public class ServerAPITest : TestWithServer {

    private const string LOBBY_NAME = "Test Lobby";
    private const string? LOBBY_PASSWORD = null;

    private readonly ILobbyFactory factory1;
    private readonly ILobbyFactory factory2;

    private readonly SteamUser SteamUser1 = SteamUser.CreateTempUser(0, "Alfredo");
    private readonly SteamUser SteamUser2 = SteamUser.CreateTempUser(1, "Alfredos Friend");

    private ILobbyHandle lobby;

    public ServerAPITest() {
        factory1 = new OnlineLobbyFactory(serverAPI, endpoint, SteamUser1);
        factory2 = new OnlineLobbyFactory(serverAPI, endpoint, SteamUser2);
    }

    [SetUp]
    public void SetUp() {
        lobby = HostTestLobby(LOBBY_NAME, LOBBY_PASSWORD, GameCase.CompanyOfHeroes3, "bg", factory1);
    }

    [TearDown] 
    public void TearDown() {
        lobby.CloseHandle();
    }

    [Test]
    public void CanGetLobbyList() {

        var lobbies = serverAPI.GetLobbies();

        Assert.Multiple(() => {

            Assert.That(lobbies, Has.Count.EqualTo(1));

            Assert.That(lobbies[0].Game, Is.EqualTo(GameCase.CompanyOfHeroes3.ToString()));

            Assert.That(lobbies[0].GetGame(), Is.EqualTo(GameCase.CompanyOfHeroes3));

        });

    }

}
