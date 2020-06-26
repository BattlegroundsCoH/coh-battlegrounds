using System.Diagnostics;
using System.Text;

namespace Battlegrounds.Game {
    
    /// <summary>
    /// Static class for launching Company of Heroes 2 with the proper set of arguments.
    /// </summary>
    public static class CoH2Launcher {

        /// <summary>
        /// The steam app ID for Company of Heroes 2.
        /// </summary>
        public const string GameAppID = "231430";

        /// <summary>
        /// Launch Company of Heroes 2 through Steam
        /// </summary>
        public static bool Launch() {

            StringBuilder commandline = new StringBuilder();
            commandline.Append($"-applaunch {GameAppID} ");

            ProcessStartInfo startInfo = new ProcessStartInfo {
                FileName = Pathfinder.GetOrFindSteamPath(),
                Arguments = commandline.ToString()
            };

            try {
                Process.Start(startInfo);
            } catch {
                return false;
            }

            return true;

        }

    }

}
