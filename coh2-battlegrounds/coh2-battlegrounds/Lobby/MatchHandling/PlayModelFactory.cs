using Battlegrounds.Modding;
using Battlegrounds.Networking.LobbySystem;

namespace BattlegroundsApp.Lobby.MatchHandling;

internal static class PlayModelFactory {

    public static IPlayModel GetModel(LobbyHandler handler) {

        // If only one human -> single model
        if (handler.Lobby.Humans == 1) {
            return new SingleplayerModel(handler);
        } else {
            return new OnlineModel(handler);
        }

    }

}
