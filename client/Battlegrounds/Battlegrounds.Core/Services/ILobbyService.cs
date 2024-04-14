using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Battlegrounds.Core.Services;

public interface ILobbyService {

    public record struct LobbyPlayerDTO(string DisplayName, string Faction);
    public record struct LobbyTeamDTO(string Name, string Alliance, LobbyPlayerDTO[] Players);
    public record struct LobbyDTO(Guid Id, string Name, LobbyTeamDTO[] Teams, bool IsPasswordProtected, string GameId);

    public Task<LobbyDTO[]> GetLobbiesAsync();

}
