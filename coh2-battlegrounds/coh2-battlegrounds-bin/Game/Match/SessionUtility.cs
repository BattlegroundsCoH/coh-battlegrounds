using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using Battlegrounds.Compiler;
using Battlegrounds.Compiler.Source;
using Battlegrounds.Functional;
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
        /// <param name="session">The <see cref="ISession"/> instance to compile</param>
        /// <returns>Will return <see langword="true"/> if <see cref="ISession"/> was compiled into a .sga file. Otherwise <see langword="false"/>.</returns>
        public static bool CompileSession(ISessionCompiler compiler, ISession session) {

            // Get the scar file
            string sessionScarFile = BattlegroundsInstance.GetRelativePath(BattlegroundsPaths.SESSION_FOLDER, "session.scar");

            // Conatiner for additional files to include
            List<WinconditionSourceFile> includeFiles = new();

            // Try the following
            try {

                // Log compiler
                Trace.WriteLine($"Compiling \"{sessionScarFile}\" into scar file useing '{compiler.GetType().Name}'", nameof(SessionUtility));

                // Write contents to session.scar
                File.WriteAllText(sessionScarFile, compiler.CompileSession(session));

                // If supply system exists
                if (session.Settings.GetCastValueOrDefault("sypply_system", false) is true) {

                    // Get the supply file
                    string sessionSupplyScarFile = BattlegroundsInstance.GetRelativePath(BattlegroundsPaths.SESSION_FOLDER, "session_supply.scar");

                    // Log compiler
                    Trace.WriteLine($"Compiling \"{sessionSupplyScarFile}\" into a scar file using '{compiler.GetType().Name}'", nameof(SessionUtility));

#if DEBUG
                    // Write contents to session_supply.scar
                    File.WriteAllText(sessionSupplyScarFile, compiler.CompileSupplyData(session));
#endif

                    // Add supply file to include files
                    includeFiles.Add(new("auxiliary_scripts\\session_supply.scar", Encoding.UTF8.GetBytes(compiler.CompileSupplyData(session))));

                }

            } catch {

                // Any error ==> return false
                return false;

            }

            // Get the gamemode source code finder
            var sourceFinder = WinconditionSourceFinder.GetSource(session.Gamemode);

            // log the source finder type
            Trace.WriteLine($"Using {sourceFinder} as wincondition code source", "SessionCompiler");

            // Get the input directory
            var buildFolder = BattlegroundsInstance.GetRelativePath(BattlegroundsPaths.BUILD_FOLDER, string.Empty);

            // Get the locale compiler
            var locCompiler = new LocaleCompiler();

            // Return the result of the win condition compilation
            return WinconditionCompiler.CompileToSga(buildFolder, sessionScarFile, session.Gamemode, sourceFinder, locCompiler, includeFiles.ToArray());

        }

        public static readonly string LogFilePath = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\my games\\company of heroes 2\\warnings.log";

        /// <summary>
        /// Check the latest log if any fatal scar error occured.
        /// </summary>
        /// <returns><see langword="true"/> if a fatal scar error was detected; Otherwise <see langword="false"/>.</returns>
        public static bool GotFatalScarError() {

            if (File.Exists(LogFilePath)) {
                if (File.ReadAllText(LogFilePath).Contains("GameObj::OnFatalScarError:")) {
                    return true;
                }
            }

            return false;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static bool WasAnyMatchPlayed() {

            if (File.Exists(LogFilePath)) {
                if (File.ReadAllText(LogFilePath).Contains("GameObj::OnFatalScarError:")) {
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
