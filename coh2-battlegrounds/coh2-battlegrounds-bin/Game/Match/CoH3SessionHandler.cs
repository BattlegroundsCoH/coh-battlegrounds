using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using Battlegrounds.Compiler;
using Battlegrounds.Compiler.Source;
using Battlegrounds.Compiler.Wincondition;
using Battlegrounds.Functional;
using Battlegrounds.Game.DataSource.Playback;
using Battlegrounds.Logging;

namespace Battlegrounds.Game.Match;

/// <summary>
/// 
/// </summary>
public class CoH3SessionHandler : ISessionHandler {

    /// <summary>
    /// Path to the Company of Heroes 3 warnings log
    /// </summary>
    public static readonly string LogFilePath = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\my games\\company of heroes 3\\warnings.log";

    private static readonly Logger logger = Logger.CreateLogger();

    private readonly WinconditionSourceProviderFactory winconditionSourceFactory;
    private readonly WinconditionCompilerFactory winconditionCompilerFactory;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="winconditionSourceFactory"></param>
    /// <param name="winconditionCompilerFactory"></param>
    public CoH3SessionHandler(WinconditionSourceProviderFactory winconditionSourceFactory, WinconditionCompilerFactory winconditionCompilerFactory) {
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
                logger.Debug("Skipping supply system for CoH3 until implemented");
            }

            // Loop over additional include files
            for (int i = 0; i < session.Gamemode.IncludeFiles.Length; i++) { // TODO: Make common helper method somewhere to abstract this (Make sure to also use in CoH2SessionHandler)

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
        var sourceFinder = winconditionSourceFactory.GetSource(GameCase.CompanyOfHeroes3);

        // log the source finder type
        logger.Info($"Using [{sourceFinder}] as wincondition code source");

        // Get the compiler
        var winconditionCompiler = winconditionCompilerFactory.GetWinconditionCompiler(GameCase.CompanyOfHeroes3);

        // Return the result of the win condition compilation
        return winconditionCompiler.CompileToSga(sessionScarFile, session, sourceFinder, includeFiles.ToArray());

    }

    /// <inheritdoc/>
    public GameProcess GetNewGameProcess() => new CoH3Process();

    /// <inheritdoc/>
    public async Task<bool> GotBugsplat() => await Task.FromResult(false);

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

    /// <summary>
    /// Determine if there is a playback at the expected location
    /// </summary>
    /// <returns>Returns <see langword="true"/> if there's a file at <see cref="PlaybackLoader.LATEST_COH3_REPLAY_FILE"/>. Otherwise <see langword="false"/>.</returns>
    public bool HasPlayback() => File.Exists(PlaybackLoader.LATEST_COH3_REPLAY_FILE);

    /// <inheritdoc/>
    public bool WasAnyMatchPlayed() {

        if (File.Exists(LogFilePath)) {
            return true;
        }

        return false;

    }

}
