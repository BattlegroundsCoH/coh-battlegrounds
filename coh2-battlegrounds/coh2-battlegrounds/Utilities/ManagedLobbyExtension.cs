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

        public static async Task<(int, string, string, int)> GetPlayerdata(this ManagedLobby lobby, int playerIndex) {
            if (int.TryParse(await lobby.GetUserInformation(playerIndex, "tid"), out int teamIndex)) {
                string faction = await lobby.GetUserInformation(playerIndex, "fac");
                string companyName = await lobby.GetUserInformation(playerIndex, "com");
                if (int.TryParse(await lobby.GetUserInformation(playerIndex, "dif"), out int aiDiff)) {
                    return (teamIndex, faction, companyName, aiDiff);
                } else {
                    return (teamIndex, faction, companyName, -1);
                }
            } else {
                return (-1, "<None>", "<None>", -1);
            }
        }

    }

}
