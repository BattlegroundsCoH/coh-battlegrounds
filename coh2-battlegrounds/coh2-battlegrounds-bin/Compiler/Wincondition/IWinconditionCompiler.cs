using Battlegrounds.Compiler.Source;
using Battlegrounds.Game.Match;

namespace Battlegrounds.Compiler.Wincondition;

/// <summary>
/// Interface for a wincondition compiler that handles the compilation of wincondition sources and session info into a .sga archive file.
/// </summary>
public interface IWinconditionCompiler {

    /// <summary>
    /// Get the path to where the gamemode archive is saved.
    /// </summary>
    /// <returns>The absolute path to the wincondition archive file.</returns>
    string GetArchivePath();

    /// <summary>
    /// Compile a session into a sga archive file.
    /// </summary>
    /// <param name="sessionFile">The session file to include</param>
    /// <param name="session">The session to compile.</param>
    /// <param name="source">The wincondition source file locator.</param>
    /// <param name="includeFiles">Additional files to include in the gamemode.</param>
    /// <returns>True if the archive file was created sucessfully. False if any error occured.</returns>
    bool CompileToSga(string sessionFile, ISession session, IWinconditionSourceProvider source, params WinconditionSourceFile[] includeFiles);

}
