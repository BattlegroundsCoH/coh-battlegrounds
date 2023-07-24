using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Battlegrounds.Game;
using Battlegrounds.Networking.LobbySystem.Factory;
using Battlegrounds.Networking.Server;
using Battlegrounds.Steam;

namespace Battlegrounds.Testing.Core.Networking.LobbySystem;

public class OnlineLobbyHandleTest : TestWithServer {

    private const string LOBBY_NAME = "Test Lobby";
    private const string? LOBBY_PASSWORD = null;

    private readonly ILobbyFactory factory1;
    private readonly ILobbyFactory factory2;

    private readonly SteamUser SteamUser1 = SteamUser.CreateTempUser(0, "Alfredo");
    private readonly SteamUser SteamUser2 = SteamUser.CreateTempUser(1, "Alfredos Friend");

    public OnlineLobbyHandleTest() {
        factory1 = new OnlineLobbyFactory(serverAPI, endpoint, SteamUser1);
        factory2 = new OnlineLobbyFactory(serverAPI, endpoint, SteamUser2);
    }

    [Test]
    public void CanHostLobby() {

        var lobby = HostTestLobby(LOBBY_NAME, LOBBY_PASSWORD, GameCase.CompanyOfHeroes3, "bg", factory1);
        Assert.That(lobby, Is.Not.Null);

        lobby.CloseHandle();

    }

}
