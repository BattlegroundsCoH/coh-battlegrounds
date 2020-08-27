using Battlegrounds.Game.Database;
using Battlegrounds.Online.Services;

namespace BattlegroundsApp.Utilities {

    public delegate void LobbyStringCallback(string value);

    public static class ManagedLobbyExtension {

        public static void GetSelectedMap(this ManagedLobby lobby, bool format, LobbyStringCallback callback) 
            => lobby.GetLobbyInformation("selected_map", (a, _) => callback((format) ? ScenarioList.FromFilename(a).Name : a));

        public static void GetSelectedGamemode(this ManagedLobby lobby, LobbyStringCallback callback)
            => lobby.GetLobbyInformation("selected_wc", (a, _) => callback(a));

        public static void GetSelectedGamemodeOption(this ManagedLobby lobby, LobbyStringCallback callback)
            => lobby.GetLobbyInformation("selected_wcs", (a, _) => callback(a));

    }

}
