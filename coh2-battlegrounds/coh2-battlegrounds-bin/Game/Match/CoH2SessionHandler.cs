using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using Battlegrounds.Compiler;
using Battlegrounds.Compiler.Source;
using Battlegrounds.Compiler.Wincondition;
using Battlegrounds.Functional;
using Battlegrounds.Game.DataSource.Playback;
using Battlegrounds.Logging;

namespace Battlegrounds.Game.Match;

/// <summary>
/// Static utility class for working with <see cref="Session"/> data.
/// </summary>
public class CoH2SessionHandler : ISessionHandler {

    /// <summary>
    /// Path to the Company of Heroes 2 warnings log
    /// </summary>
    public static readonly string LogFilePath = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\my games\\company of heroes 2\\warnings.log";

    private static readonly Logger logger = Logger.CreateLogger();

    private readonly WinconditionSourceProviderFactory winconditionSourceFactory;
    private readonly WinconditionCompilerFactory winconditionCompilerFactory;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="winconditionSourceFactory"></param>
    /// <param name="winconditionCompilerFactory"></param>
    public CoH2SessionHandler(WinconditionSourceProviderFactory winconditionSourceFactory, WinconditionCompilerFactory winconditionCompilerFactory) {
        this.winconditionSourceFactory = winconditionSourceFactory;
        this.winconditionCompilerFactory = winconditionCompilerFactory;
    }

    /// <inheritdoc/>
    public bool CompileSession(ISessionCompiler compiler, ISession session) {

        // Get the scar file
        string sessionScarFile = BattlegroundsContext.GetRelativePath(BattlegroundsPaths.SESSION_FOLDER, "session.scar");

        // Conatiner for additional files to include
        List<WinconditionSourceFile> includeFiles = new();

        // Try the following
        try {

            // Log compiler
            logger.Info($"Compiling \"{sessionScarFile}\" into scar file using '{compiler.GetType().Name}'");

            // Write contents to session.scar
            File.WriteAllText(sessionScarFile, compiler.CompileSession(session));

            // If supply system exists
            if (session.Settings.GetCastValueOrDefault("sypply_system", false) is true) {

                // Get the supply file
                string sessionSupplyScarFile = BattlegroundsContext.GetRelativePath(BattlegroundsPaths.SESSION_FOLDER, "session_supply.scar");

                // Log compiler
                logger.Info($"Compiling \"{sessionSupplyScarFile}\" into a scar file using '{compiler.GetType().Name}'");

#if DEBUG
                // Write contents to session_supply.scar
                File.WriteAllText(sessionSupplyScarFile, compiler.CompileSupplyData(session));
#endif

                // Add supply file to include files
                includeFiles.Add(new("auxiliary_scripts\\session_supply.scar", Encoding.UTF8.GetBytes(compiler.CompileSupplyData(session))));

            }

            // Loop over additional include files
            for (int i = 0; i < session.Gamemode.IncludeFiles.Length; i++) {

                // Grab path and parse it
                var pathInfo = session.Gamemode.IncludeFiles[i].Split(';');
                if (pathInfo.Length is 2) {

                    // Pick real path
                    var realPath = BattlegroundsContext.GetRelativeVirtualPath(pathInfo[0], ".scar");

                    // Verify file exists
                    if (!File.Exists(realPath)) {
                        logger.Warning($"Invalid include file. File not found: {realPath}");
                    }

                    // Read contents
                    var contents = File.ReadAllBytes(realPath);

                    // Register
                    includeFiles.Add(new(pathInfo[1], contents));

                } else {

                    // Log
                    logger.Warning("Invalid include file. Expected include file of the form '<RealPath>;<ScarPath>'");

                }

            }

        } catch (Exception e) {

            // Log
            logger.Exception(e);

            // Any error ==> return false
            return false;

        }

        // Get the gamemode source code finder
        var sourceFinder = winconditionSourceFactory.GetSource(GameCase.CompanyOfHeroes2);

        // log the source finder type
        logger.Info($"Using [{sourceFinder}] as wincondition code source");

        // Get the compiler
        var winconditionCompiler = winconditionCompilerFactory.GetWinconditionCompiler(GameCase.CompanyOfHeroes2);

        // Return the result of the win condition compilation
        return winconditionCompiler.CompileToSga(sessionScarFile, session, sourceFinder, includeFiles.ToArray());

    }

    /// <inheritdoc/>
    public bool GotFatalScarError() {

        if (!File.Exists(LogFilePath)) {
            return false;
        }

        try {
            return File.ReadAllText(LogFilePath).Contains("GameObj::OnFatalScarError:");
        } catch (IOException iox) {
            logger.Error($"Error reading warnings.log when checking for scar error: {iox.Message}");
            return false; // ASSUME false - may change to true if this branch is hit more in that case.
        }

    }

    /// <inheritdoc/>
    public bool WasAnyMatchPlayed() {

        if (File.Exists(LogFilePath)) {
            return true;
        }

        return false;

    }

    /// <inheritdoc/>
    public async Task<bool> GotBugsplat() {

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
    /// <returns>Returns <see langword="true"/> if there's a file at <see cref="PlaybackLoader.LATEST_COH2_REPLAY_FILE"/>. Otherwise <see langword="false"/>.</returns>
    public bool HasPlayback() => File.Exists(PlaybackLoader.LATEST_COH2_REPLAY_FILE);

    /// <inheritdoc/>
    public GameProcess GetNewGameProcess() => new CoH2Process();

}
