using Battlegrounds.Models.Lobbies;
using Battlegrounds.Models.Playing;

namespace Battlegrounds.Services;

public sealed class LobbyService : ILobbyService {
    
    public async Task<ILobby> CreateLobbyAsync(string name, string? password, bool multiplayer, Game game) {
        if (multiplayer) {
            return await CreateMultiplayerLobbyAsync(name, password, game);
        }
        return await CreateSingleplayerLobbyAsync(name, game);
    }

    private Task<ILobby> CreateSingleplayerLobbyAsync(string name, Game game) {
        return Task.FromResult(new SingleplayerLobby(name, game) as ILobby);
    }

    private Task<ILobby> CreateMultiplayerLobbyAsync(string name, string? password, Game game) {
        return Task.FromResult(new MultiplayerLobby() as ILobby);
    }

}
