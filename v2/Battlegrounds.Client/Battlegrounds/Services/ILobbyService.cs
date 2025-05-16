using Battlegrounds.Models.Lobbies;
using Battlegrounds.Models.Playing;

namespace Battlegrounds.Services;

public interface ILobbyService {
    
    Task<ILobby> CreateLobbyAsync(string name, string? password, bool multiplayer, Game game);

}
