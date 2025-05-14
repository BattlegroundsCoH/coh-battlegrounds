using Battlegrounds.Models;
using Battlegrounds.Services;

namespace Battlegrounds.ViewModels;

public sealed class LobbyViewModel {

    private readonly ILobby _lobby;

    public LobbyViewModel(ILobby lobby, ILobbyService lobbyService) {
        _lobby = lobby;
    }

}
