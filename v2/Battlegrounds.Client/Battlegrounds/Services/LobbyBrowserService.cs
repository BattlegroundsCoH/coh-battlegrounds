using Battlegrounds.Models;

namespace Battlegrounds.Services;

public sealed class LobbyBrowserService() : ILobbyBrowserService {

    public Task<IEnumerable<BrowserLobby>> GetLobbiesAsync() => Task.FromResult(Enumerable.Empty<BrowserLobby>()); // TODO: Implement this method

    public Task<bool> IsServerAvailableAsync() => Task.Delay(1500).ContinueWith(x => true); // TODO: Implement this method

}
