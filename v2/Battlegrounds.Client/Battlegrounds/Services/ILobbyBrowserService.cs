using Battlegrounds.Models;

namespace Battlegrounds.Services;

public interface ILobbyBrowserService {

    Task<bool> IsServerAvailableAsync();

    Task<IEnumerable<BrowserLobby>> GetLobbiesAsync();

}
