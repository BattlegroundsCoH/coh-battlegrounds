using Battlegrounds.Models;

namespace Battlegrounds.Services;

public interface ILobbyService {
    
    Task<ILobby> CreateLobbyAsync(string name, string? password, bool multiplayer);

}
