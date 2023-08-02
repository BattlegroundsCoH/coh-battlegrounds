using System;
using System.IO;
using System.IO.Compression;
using System.Linq;

using Battlegrounds.Compiler.Source;
using Battlegrounds.Functional;
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
        this.workDirectory = workDirectory.EndsWith("\\") ? workDirectory : workDirectory + "\\";
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

        // Create work directory
        CreateWorkspace();

        // Create archive definition
        ArchiveDefinition archiveDefinition = new ArchiveDefinition() { 
            OutputFilepath = GetArchivePath(),
            TableOfContents = new[] {
                CompileDataTOC(source, session, includeFiles) // TOC: Data
            }
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

    private void CreateWorkspace() {

        // Clear the directory we're going to work in
        if (Directory.Exists(workDirectory)) {
            Directory.Delete(workDirectory, true);
        }

        // Create directories
        Directory.CreateDirectory(workDirectory);
        Directory.CreateDirectory($"{workDirectory}data\\scar\\winconditions\\bg_scripts\\ui");
        Directory.CreateDirectory($"{workDirectory}data\\info");
        Directory.CreateDirectory($"{workDirectory}data\\locale");

    }

    private ArchiveDefinition.ArchiveTOC CompileDataTOC(IWinconditionSourceProvider source, ISession session, WinconditionSourceFile[] includeFiles) {

        // Get the files
        var scarFiles = source.GetScarFiles().Union(includeFiles.Where(x => x.Path.EndsWith(".scar", StringComparison.InvariantCulture))).ToArray();
        var winFiles = source.GetWinFiles(); // .bin file here
        var localeFiles = source.GetLocaleFiles("");
        var uiFiles = source.GetUIFiles(session.Gamemode);
        var infoFile = source.GetInfoFile(session.Gamemode);
        var modiconFile = source.GetModGraphic();

        // Copy over scar files
        var scarArchiveEntry = scarFiles.Map(scarFile => {
            string filename = Path.Combine(workDirectory, "data", "scar", "winconditions", scarFile.Path);
            File.WriteAllBytes(filename, scarFile.Contents);
            return new ArchiveDefinition.ArchiveFile() { 
                FileName = filename, 
                RelativePath = "scar\\winconditions\\" + scarFile.Path, 
                Encryption = "None", 
                Storage = "StreamCompress", 
                Verification = "None" 
            };
        });

        // Copy over the bin files
        var binArchiveEntry = new ArchiveDefinition.ArchiveFile[] {
            MakeArchiveFile(winFiles[0], Path.Combine(workDirectory, "data", winFiles[0].Path)),
            MakeArchiveFile(infoFile, Path.Combine(workDirectory, "data", infoFile.Path))
        };

        // Return
        return new ArchiveDefinition.ArchiveTOC() {
            Alias = "Data",
            TocName = "Data",
            Files = scarArchiveEntry.Concat(binArchiveEntry)
        };

    }

    private ArchiveDefinition.ArchiveFile MakeArchiveFile(WinconditionSourceFile file, string filename) {
        File.WriteAllBytes(filename, file.Contents);
        return new ArchiveDefinition.ArchiveFile() {
            FileName = filename,
            RelativePath = file.Path,
            Encryption = "None",
            Storage = "StreamCompress",
            Verification = "None"
        };
    }

}
