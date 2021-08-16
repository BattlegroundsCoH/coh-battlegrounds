using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

using Battlegrounds.Compiler;
using Battlegrounds.Compiler.Source;
using Battlegrounds.Game.Match.Data;
using Battlegrounds.Networking.Server;

namespace Battlegrounds.Game.Match {

    /// <summary>
    /// Static utility class for working with <see cref="Session"/> data.
    /// </summary>
    public static class SessionUtility {

        /// <summary>
        /// Fully compile a <see cref="Session"/> into a .sga file using a concrete <see cref="ISessionCompiler"/> instance.
        /// </summary>
        /// <param name="compiler">The <see cref="ISessionCompiler"/> compiler to use when compiling session data.</param>
        /// <param name="session">The <see cref="Session"/> instance to compile</param>
        /// <returns>Will return <see langword="true"/> if <see cref="Session"/> was compiled into a .sga file. Otherwise <see langword="false"/>.</returns>
        public static bool CompileSession(ISessionCompiler compiler, Session session, ServerAPI serverAPI) {

            // Get the scar file
            string sessionScarFile = BattlegroundsInstance.GetRelativePath(BattlegroundsPaths.SESSION_FOLDER, "session.scar");

            // Try the following
            try {

                // Log compiler
                Trace.WriteLine($"Compiling \"{sessionScarFile}\" into a .sga archive using a '{compiler}' compiler", "SessionCompiler");

                // Compile the session
                string luaSessionOutput = compiler.CompileSession(session);

                // Write contents to session.scar
                File.WriteAllText(sessionScarFile, luaSessionOutput);

            } catch {

                // Any error ==> return false
                return false;

            }

            // Get the gamemode source code finder
            var sourceFinder = WinconditionSourceFinder.GetSource(session.Gamemode, serverAPI);

            // log the source finder type
            Trace.WriteLine($"Using {sourceFinder} as wincondition code source", "SessionCompiler");

            // Return the result of the win condition compilation
            return WinconditionCompiler.CompileToSga(BattlegroundsInstance.GetRelativePath(BattlegroundsPaths.BUILD_FOLDER, string.Empty), sessionScarFile, session.Gamemode, sourceFinder);

        }

        /// <summary>
        /// Check the latest log if any fatal scar error occured.
        /// </summary>
        /// <returns><see langword="true"/> if a fatal scar error was detected; Otherwise <see langword="false"/>.</returns>
        public static bool GotFatalScarError() {

            string logPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\my games\\company of heroes 2\\warnings.log";

            if (File.Exists(logPath)) {
                if (File.ReadAllText(logPath).Contains("GameObj::OnFatalScarError:")) {
                    return true;
                }
            }

            return false;

        }

        /// <summary>
        /// Check if a bugsplat occured.
        /// </summary>
        /// <returns><see langword="true"/> if a bug splat was detected; Otherwise <see langword="false"/>.</returns>
        public static async Task<bool> GotBugsplat() {

            int count = 0;

            while (count < 15) {
                Process[] processes = Process.GetProcessesByName("BsSndRpt");
                if (processes.Length >= 1) {
                    return true;
                }
                await Task.Delay(100);
                count++;
            }

            return false;

        }

        /// <summary>
        /// Determine if there is a playback at the expected location
        /// </summary>
        /// <returns>Returns <see langword="true"/> if there's a file at <see cref="ReplayMatchData.LATEST_REPLAY_FILE"/>. Otherwise <see langword="false"/>.</returns>
        public static bool HasPlayback()
            => File.Exists(ReplayMatchData.LATEST_REPLAY_FILE);


    }

}
