using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Globalization;

using Battlegrounds.Util;
using Battlegrounds.Compiler.Source;
using Battlegrounds.Functional;
using Battlegrounds.Game.Match;
using Battlegrounds.Logging;
using Battlegrounds.Compiler.Essence;
using Battlegrounds.Compiler.Locale;

namespace Battlegrounds.Compiler.Wincondition.CoH2;

/// <summary>
/// Wincondition compiler for compiling a Company of Heroes 2 wincondition into a valid .sga file
/// </summary>
public sealed class CoH2WinconditionCompiler : IWinconditionCompiler {

    private static readonly Logger logger = Logger.CreateLogger();

    private readonly ILocaleCompiler localeCompiler;
    private readonly string workDirectory;

    /// <summary>
    /// Create a new <see cref="CoH2WinconditionCompiler"/> instance.
    /// </summary>
    /// <param name="workDirectory">The work directory to output temporary files to.</param>
    /// <param name="localeCompiler">The locale compiler to use when compiling locales.</param>
    public CoH2WinconditionCompiler(string workDirectory, ILocaleCompiler localeCompiler) {
        this.workDirectory = !workDirectory.EndsWith("\\", false, CultureInfo.InvariantCulture) ? workDirectory + "\\" : workDirectory;
        this.localeCompiler = localeCompiler;
    }

    /// <inheritdoc/>
    public string GetArchivePath() {
        string dirpath = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\my games\\Company of Heroes 2\\mods\\gamemode\\subscriptions";
        if (!Directory.Exists(dirpath)) {
            Directory.CreateDirectory(dirpath);
        }
        return dirpath + "\\coh2_battlegrounds_wincondition.sga";
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

        // Create the workspace
        CreateWorkspace();

        // The archive definition to use when compiling
        TxtBuilder archiveDef = new();

        // Scar/Data TOC section
        archiveDef.AppendLine($"Archive name=\"{session.Gamemode.Guid}\" blocksize=\"262144\"");
        archiveDef.AppendLine("TOCStart name=\"data\" alias=\"data\" path=\"\" relativeroot=\"data\"");
        archiveDef.AppendLine("\tFileSettingsStart defverification=\"sha1_blocks\" defcompression=\"stream_compress\"");
        archiveDef.AppendLine("\t\tOverride wildcard=\".*\\.(rgt|ttf)$\" minsize=\"-1\" maxsize=\"-1\" vt=\"none\" ct=\"store\"");
        archiveDef.AppendLine("\t\tOverride wildcard=\".*\\.(abp)$\" minsize=\"-1\" maxsize=\"-1\" vt=\"none\" ct=\"buffer_compress\"");
        archiveDef.AppendLine("\t\tOverride wildcard=\".*\\.(dds|mua|muax|rpb|sua|tga)$\" minsize=\"-1\" maxsize=\"-1\" vt=\"none\" ct=\"\"");
        archiveDef.AppendLine("\t\tOverride wildcard=\".*\\.(gfx)$\" minsize=\"-1\" maxsize=\"-1\" vt=\"sha1_blocks\" ct=\"\"");
        archiveDef.AppendLine("\t\tOverride wildcard=\".*\\.(ai|events|info|lua|options|scar|scenref|squadai)$\" minsize=\"-1\" maxsize=\"-1\" vt=\"sha1_blocks\" ct=\"buffer_compress\"");
        archiveDef.AppendLine("\t\tOverride wildcard=\".*\\.(scenariomarker|sgb|ter|sme|exclude|path)$\" minsize=\"-1\" maxsize=\"-1\" vt=\"crc_blocks\" ct=\"\"");
        archiveDef.AppendLine("\t\tOverride wildcard=\".*\\.(smf)$\" minsize=\"-1\" maxsize=\"-1\" vt=\"none\" ct=\"store\"");
        archiveDef.AppendLine("\t\tOverride wildcard=\".*\\.(bsc)$\" minsize=\"-1\" maxsize=\"-1\" vt=\"none\" ct=\"buffer_compress\"");
        archiveDef.AppendLine("\tFileSettingsEnd");

        // Add and compile win file(s)
        foreach (var file in winFiles) {
            if (!AddFile(archiveDef, "data\\game\\winconditions\\", file)) {
                return false;
            }
        }

        // Add the session file
        AddLocalFile(archiveDef, sessionFile, "data\\scar\\winconditions\\auxiliary_scripts\\");

        // Add and *compile* scar files
        foreach (var file in scarFiles) {
            if (!AddFile(archiveDef, "data\\scar\\winconditions\\", file)) {
                return false;
            }
        }

        // Add the graphic files
        AddGraphics(archiveDef, uiFiles);

        // Info TOC section
        archiveDef.AppendLine("TOCEnd");
        archiveDef.AppendLine("TOCStart name=\"info\" alias=\"info\" path=\"\" relativeroot=\"info\"");
        archiveDef.AppendLine("\tFileSettingsStart defverification=\"crc_blocks\" defcompression=\"stream_compress\"");
        archiveDef.AppendLine("\t\tOverride wildcard=\".*\\.(lua)$\" minsize=\"-1\" maxsize=\"-1\" vt=\"crc_blocks\" ct=\"buffer_compress\"");
        archiveDef.AppendLine("\tFileSettingsEnd");

        // Add and compile info file
        if (!AddInfoFiles(archiveDef, infoFile, modiconFile)) {
            return false;
        }

        // Locale TOC section
        archiveDef.AppendLine("TOCEnd");
        archiveDef.AppendLine("TOCStart name=\"locale\" alias=\"locale\" path=\"\" relativeroot=\"locale\"");
        archiveDef.AppendLine("\tFileSettingsStart defverification=\"crc_blocks\" defcompression=\"stream_compress\"");
        archiveDef.AppendLine("\tFileSettingsEnd");

        // Add and compile locale files
        foreach (var file in localeFiles) {

            // Grab path and log
            string abspath = Path.GetFullPath(workDirectory + file.Path.Replace("/", "\\"));
            logger.Info($"Adding locale file [ABS] <{abspath}>");

            // Translate the locale file
            localeCompiler.TranslateLocale(file.Contents, abspath, session.Names.ToArray());

            // Append win condition file to locale section.
            archiveDef.AppendLine($"\t{abspath}");

        }

        // Add end TOC
        archiveDef.AppendLine("TOCEnd");

        // Generate the path
        string archiveDefTxtPath = workDirectory + "ArchiveDefinition.txt";

        // Save the archive definition
        archiveDef.Save(archiveDefTxtPath);

        // The output archive
        string outputArchive = GetArchivePath();

        // Call the archive
        Archiver archiver = new Archiver(); // TODO: Enable dependency injection
        if (!archiver.Archive(archiveDefTxtPath, workDirectory, outputArchive)) {
            return false;
        }

#if RELEASE
        // Delete the temp_build directory
        Directory.Delete(workdir, true);
#endif

        // Return true
        return true;

    }

    private void CreateWorkspace() {

        // Clear the directory we're going to work in
        if (Directory.Exists(workDirectory)) {
            Directory.Delete(workDirectory, true);
        }

        // Create directories
        Directory.CreateDirectory(workDirectory);
        Directory.CreateDirectory($"{workDirectory}data\\game\\winconditions");
        Directory.CreateDirectory($"{workDirectory}data\\scar\\winconditions\\auxiliary_scripts");
        Directory.CreateDirectory($"{workDirectory}data\\scar\\winconditions\\ui_api");
        Directory.CreateDirectory($"{workDirectory}data\\ui\\Assets\\Textures\\");
        Directory.CreateDirectory($"{workDirectory}data\\ui\\Bin\\");
        Directory.CreateDirectory($"{workDirectory}info");
        Directory.CreateDirectory($"{workDirectory}locale\\english");

    }

    private bool AddFile(TxtBuilder builder, string rpath, WinconditionSourceFile sourceFile, bool useBytes = false, Encoding? encoding = null) {

        string relpath = rpath + sourceFile.Path;
        string abspath = Path.GetFullPath(workDirectory + relpath.Replace("/", "\\"));

        if (sourceFile.Contents == null || sourceFile.Contents.Length == 0) {
            logger.Error($"Failed adding file [ABS] <{abspath}>");
            return false;
        }

        logger.Info($"Adding file [ABS] <{abspath}>");

        builder.AppendLine($"\t{abspath}");

        if (File.Exists(abspath)) {
            File.Delete(abspath);
        }

        if (useBytes) {
            File.WriteAllBytes(abspath, sourceFile.Contents);
        } else {
            encoding ??= Encoding.UTF8;
            File.WriteAllText(abspath, encoding.GetString(sourceFile.Contents));
        }

        return true;

    }

    private void AddLocalFile(TxtBuilder builder, string localfile, string relpath) {

        // Get path to copy file to
        string copyFile = Path.GetFullPath($"{workDirectory}{relpath}{Path.GetFileName(localfile)}");

        // Add the local file
        builder.AppendLine($"\t{copyFile}");

        // Copy file
        File.Copy(Path.GetFullPath(localfile), copyFile);

    }

    private void AddGraphics(TxtBuilder builder, WinconditionSourceFile[] uiFiles) {

        // Create paths
        string ddsPath = Path.GetFullPath($"{workDirectory}data\\ui\\Assets\\Textures\\");
        string gfxPath = Path.GetFullPath($"{workDirectory}data\\ui\\Bin\\");

        // Loop through gfx files and add them
        for (int i = 0; i < uiFiles.Length; i++) {
            if (uiFiles[i].Path.EndsWith(".dds")) {
                string ddspath = $"{ddsPath}{Path.GetFileName(uiFiles[i].Path)}";
                File.WriteAllBytes(ddspath, uiFiles[i].Contents);
                builder.AppendLine($"\t{ddspath}");
            } else if (uiFiles[i].Path.EndsWith(".gfx")) {
                string gfxpath = $"{gfxPath}{Path.GetFileName(uiFiles[i].Path)}";
                File.WriteAllBytes(gfxpath, uiFiles[i].Contents);
                builder.AppendLine($"\t{gfxpath}");
            } else {
                logger.Warning($"Skipping graphics file \"{uiFiles[i].Path}\"");
            }
        }

    }

    private bool AddInfoFiles(TxtBuilder builder, WinconditionSourceFile infoFile, WinconditionSourceFile iconFile) {

        if (!AddFile(builder, string.Empty, infoFile)) {
            return false;
        }

        return AddFile(builder, string.Empty, iconFile);

    }

}
