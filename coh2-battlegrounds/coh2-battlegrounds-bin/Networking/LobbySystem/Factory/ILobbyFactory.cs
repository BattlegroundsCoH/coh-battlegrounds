using Battlegrounds.Functional;
using Battlegrounds.Game;
using Battlegrounds.Networking.Server;

namespace Battlegrounds.Networking.LobbySystem.Factory;

public interface ILobbyFactory {

    Result<ILobbyHandle?> HostLobby(string name, string? password, GameCase game, string mod);

    Result<ILobbyHandle?> JoinLobby(ServerLobby lobbyData, string password);

}
