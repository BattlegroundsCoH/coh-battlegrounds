using Battlegrounds.Models.Lobbies;

namespace Battlegrounds.Services;

public interface ILobbyBrowserService {

    Task<bool> IsServerAvailableAsync();

    Task<IEnumerable<BrowserLobby>> GetLobbiesAsync();

}
