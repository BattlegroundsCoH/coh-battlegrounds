using Battlegrounds.Models;

namespace Battlegrounds.Services;

public sealed class LobbyService : ILobbyService {
    
    public async Task<ILobby> CreateLobbyAsync(string name, string? password, bool multiplayer) {
        if (multiplayer) {
            return await CreateMultiplayerLobbyAsync(name, password);
        }
        return await CreateSingleplayerLobbyAsync(name);
    }

    private Task<ILobby> CreateSingleplayerLobbyAsync(string name) {
        throw new NotImplementedException();
    }

    private Task<MultiplayerLobby> CreateMultiplayerLobbyAsync(string name, string? password) {
        return Task.FromResult(new MultiplayerLobby());
    }

}
