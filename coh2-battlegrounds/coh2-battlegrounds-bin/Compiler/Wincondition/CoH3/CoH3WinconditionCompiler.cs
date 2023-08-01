using System;
using System.IO;
using System.Linq;

using Battlegrounds.Compiler.Source;
using Battlegrounds.Game.Match;
using Battlegrounds.Logging;

namespace Battlegrounds.Compiler.Wincondition.CoH3;

/// <summary>
/// 
/// </summary>
public sealed class CoH3WinconditionCompiler : IWinconditionCompiler {

    private static readonly Logger logger = Logger.CreateLogger();

    private readonly string workDirectory;
    private readonly LocaleCompiler localeCompiler;
    private readonly IArchiver archiver;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="workDirectory"></param>
    /// <param name="localeCompiler"></param>
    /// <param name="archiver"></param>
    public CoH3WinconditionCompiler(string workDirectory, LocaleCompiler localeCompiler, IArchiver archiver) {
        this.workDirectory = workDirectory;
        this.localeCompiler = localeCompiler;
        this.archiver = archiver;
    }

    /// <inheritdoc/>
    public bool CompileToSga(string sessionFile, ISession session, IWinconditionSourceProvider source, params WinconditionSourceFile[] includeFiles) {

        // Verify is win condition source is valid
        if (source is null) {
            logger.Error("Failed to find a valid source");
            return false;
        }

        // Get the files
        var scarFiles = source.GetScarFiles().Union(includeFiles.Where(x => x.Path.EndsWith(".scar", StringComparison.InvariantCulture)));
        var winFiles = source.GetWinFiles();
        var localeFiles = source.GetLocaleFiles("");
        var uiFiles = source.GetUIFiles(session.Gamemode);
        var infoFile = source.GetInfoFile(session.Gamemode);
        var modiconFile = source.GetModGraphic();

        // Create archive definition
        ArchiveDefinition archiveDefinition = new ArchiveDefinition() { 
            OutputFilepath = GetArchivePath()
        };

        // Generate the path
        string archiveDefTxtPath = workDirectory + "ArchiveDefinition.json";

        // Save
        if (!archiveDefinition.ToFile(archiveDefTxtPath)) {
            return false;
        }

        // Invoke archiver
        bool success = archiver.Archive(archiveDefTxtPath);
        if (!success) {
            return false;
        }

#if RELEASE
        // Delete the temp_build directory
        Directory.Delete(workdir, true);
#endif

        return true;
    }

    /// <inheritdoc/>
    public string GetArchivePath() {
        string dirpath = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\my games\\Company of Heroes 3\\mods\\extension\\subscriptions\\battlegrounds";
        if (!Directory.Exists(dirpath)) {
            Directory.CreateDirectory(dirpath);
        }
        return dirpath + "\\coh3_battlegrounds_wincondition.sga";
    }

}
