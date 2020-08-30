using System.Threading.Tasks;

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

        public static async Task<(int, string, string)> GetPlayerdata(this ManagedLobby lobby, int playerIndex) {
            if (int.TryParse(await lobby.GetLobbyInformation($"tid{playerIndex}"), out int teamIndex)) {
                string faction = await lobby.GetLobbyInformation($"fac{playerIndex}");
                string companyName = await lobby.GetLobbyInformation($"com{playerIndex}");
                return (teamIndex, faction, companyName);
            } else {
                return (-1, "<None>", "<None>");
            }
        }

    }

}
