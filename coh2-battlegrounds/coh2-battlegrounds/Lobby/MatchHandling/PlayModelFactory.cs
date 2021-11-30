using Battlegrounds.Networking.LobbySystem;

using BattlegroundsApp.Lobby.MVVM.Models;

namespace BattlegroundsApp.Lobby.MatchHandling;

internal static class PlayModelFactory {

    public static IPlayModel GetModel(LobbyHandler handler, LobbyChatSpectatorModel lobbyChat) {

        // If only one human -> single model
        if (handler.Lobby.Humans == 1) {
            return new SingleplayerModel(handler, lobbyChat);
        } else {
            return new OnlineModel(handler, lobbyChat);
        }

    }

}
