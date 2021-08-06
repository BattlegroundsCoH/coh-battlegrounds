using System.Diagnostics;
using System.Text;
using System.Threading;

namespace Battlegrounds.Game {

    /// <summary>
    /// Static class for launching Company of Heroes 2 with the proper set of arguments.
    /// </summary>
    public static class CoH2Launcher {

        /// <summary>
        /// Const ID for marking process as having run to completion
        /// </summary>
        public const int PROCESS_OK = 0;

        /// <summary>
        /// Const ID for marking process not found
        /// </summary>
        public const int PROCESS_NOT_FOUND = 1;

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

        /// <summary>
        /// Watch the RelicCoH2.exe process
        /// </summary>
        /// <returns>Integer value representing the result of the operation (0 = OK, 1 = Not found)</returns>
        public static int WatchProcess() {

            // The attempts counter
            int attemps = 0;

            // The processes
            Process[] processes = System.Array.Empty<Process>();

            // While we havent found it and there are still attempts to make
            while (attemps < 1000) {

                // Try and find it
                processes = Process.GetProcessesByName("RelicCoH2");

                // If found - break
                if (processes.Length > 0) {
                    break;
                }

                // Wait 5ms
                Thread.Sleep(5);

                // Increase attempts so it's not an endless loop
                attemps++;

            }

            // None found?
            if (processes.Length == 0) {
                return PROCESS_NOT_FOUND;
            }

            // Get the process
            Process coh2Process = processes[0];

            // Wait for exit
            coh2Process.WaitForExit();

            // Wait 0.5s
            Thread.Sleep(500);

            return PROCESS_OK;

        }

    }

}
