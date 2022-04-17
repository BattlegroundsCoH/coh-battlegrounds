using Battlegrounds.Networking.Communication.Connections;
using Battlegrounds.Networking.LobbySystem;

using BattlegroundsApp.Lobby.MVVM.Models;

namespace BattlegroundsApp.Lobby.MatchHandling;

internal static class PlayModelFactory {

    public static IPlayModel GetModel(LobbyAPI handle, LobbyChatSpectatorModel lobbyChat, UploadProgressCallbackHandler? callbackHandler) {

        // If only one human -> single model
        uint humans = handle.GetPlayerCount(humansOnly: true);
        if (humans == 1) {
            return new SingleplayerModel(handle, lobbyChat);
        } else {
            return new OnlineModel(handle, lobbyChat, callbackHandler ?? throw new System.Exception("Expected upload callback handler"));
        }

    }

}
