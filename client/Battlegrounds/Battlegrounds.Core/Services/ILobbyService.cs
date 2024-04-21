using Battlegrounds.Core.Lobbies;
using Battlegrounds.Core.Users;

namespace Battlegrounds.Core.Services;

public interface ILobbyService {

    record struct LobbyPlayerDTO(string DisplayName, string Faction);
    record struct LobbyTeamDTO(string Name, string Alliance, LobbyPlayerDTO[] Players);
    record struct LobbyDTO(Guid Id, string Name, LobbyTeamDTO[] Teams, bool IsPasswordProtected, string GameId);

    Task<LobbyDTO[]> GetLobbiesAsync();

    Task<ILobby> HostLobby(UserContext userContext, string name, string game, string password);

    Task<ILobby> JoinLobby(UserContext userContext, Guid guid, string password);

}
