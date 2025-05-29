using Battlegrounds.Models.Lobbies;
using Battlegrounds.Models.Playing;

namespace Battlegrounds.Services;

public sealed class LobbyService(IUserService userService, IGameMapService mapService) : ILobbyService {
    
    public async Task<ILobby> CreateLobbyAsync(string name, string? password, bool multiplayer, Game game) {
        if (multiplayer) {
            return await CreateMultiplayerLobbyAsync(name, password, game);
        }
        return await CreateSingleplayerLobbyAsync(name, game);
    }

    private async Task<ILobby> CreateSingleplayerLobbyAsync(string name, Game game) {
        var localUser = await userService.GetLocalUserAsync();
        var localUserParticipant = new Participant(0, localUser.UserId, localUser.UserDisplayName, false);
        var latestMap = await mapService.GetLatestMapAsync(game.Id);
        return new SingleplayerLobby(name, game, latestMap, localUserParticipant);
    }

    private Task<ILobby> CreateMultiplayerLobbyAsync(string name, string? password, Game game) {
        return Task.FromResult(new MultiplayerLobby() as ILobby);
    }

}
