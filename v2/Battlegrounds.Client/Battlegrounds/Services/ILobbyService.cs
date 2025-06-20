using Battlegrounds.Models.Lobbies;
using Battlegrounds.Models.Playing;

namespace Battlegrounds.Services;

public interface ILobbyService {
    
    bool HasActiveLobby { get; }

    ILobby? ActiveLobby { get; }

    Task<ILobby> CreateLobbyAsync(string name, string? password, bool multiplayer, Game game);
    
    Task LeaveLobbyAsync(ILobby lobby);

    Task<bool> IsServerAvailableAsync();

    Task<IEnumerable<BrowserLobby>> GetLobbiesAsync();

}
