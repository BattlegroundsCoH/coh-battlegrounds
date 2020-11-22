using System;
using System.IO;
using System.Text;
using System.Diagnostics;

using Battlegrounds.Online;
using Battlegrounds.Util;
using Battlegrounds.Compiler.Source;
using Battlegrounds.Modding;

namespace Battlegrounds.Compiler {
    
    /// <summary>
    /// Helper class for compiling a win condition in a JIT-style.
    /// </summary>
    public static class WinconditionCompiler {

        /// <summary>
        /// Compile a session into a sga archive file.
        /// </summary>
        /// <param name="workdir">The temporary work directory.</param>
        /// <param name="sessionFile">The session file to include</param>
        /// <returns>True of the archive file was created sucessfully. False if any error occured.</returns>
        public static bool CompileToSga(string workdir, string sessionFile, IWinconditionMod wincondition) {

            // Get the source
            IWinconditionSource winconSrc = WinconditionSourceFinder.GetSource(wincondition);

            // Verify is win condition source is valid
            if (winconSrc is null) {
                Trace.WriteLine("Failed to find a valid source", "Wincondition-Compiler");
                return false;
            }

            // Get the files
            WinconoditionSourceFile[] scarFiles = winconSrc.GetScarFiles();
            WinconoditionSourceFile[] winFiles = winconSrc.GetWinFiles();
            WinconoditionSourceFile[] localeFiles = winconSrc.GetLocaleFiles();
            WinconoditionSourceFile infoFile = winconSrc.GetInfoFile();

            // Fix potential missing '\'
            if (!workdir.EndsWith("\\")) {
                workdir += "\\";
            }

            // Create the workspace
            CreateWorkspace(workdir);

            // The archive definition to use when compiling
            TxtBuilder archiveDef = new TxtBuilder();
            
            // Scar/Data TOC section
            archiveDef.AppendLine("Archive name=\"6a0a13b89555402ca75b85dc30f5cb04\" blocksize=\"262144\"");
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
            foreach (WinconoditionSourceFile file in winFiles) {
                if (!AddFile(archiveDef, "data\\game\\winconditions\\", workdir, file)) {
                    return false;
                }
            }

            // Add the company file
            AddLocalFile(archiveDef, sessionFile, "data\\scar\\winconditions\\auxiliary_scripts\\", workdir);

            // Add and *compile* scar files
            foreach (WinconoditionSourceFile file in scarFiles) {
                if (!AddFile(archiveDef, "data\\scar\\winconditions\\", workdir, file)) {
                    return false;
                }
            }

            // Add the graphic files
            AddGraphics(archiveDef, workdir);

            // Info TOC section
            archiveDef.AppendLine("TOCEnd");
            archiveDef.AppendLine("TOCStart name=\"info\" alias=\"info\" path=\"\" relativeroot=\"info\"");
            archiveDef.AppendLine("\tFileSettingsStart defverification=\"crc_blocks\" defcompression=\"stream_compress\"");
            archiveDef.AppendLine("\t\tOverride wildcard=\".*\\.(lua)$\" minsize=\"-1\" maxsize=\"-1\" vt=\"crc_blocks\" ct=\"buffer_compress\"");
            archiveDef.AppendLine("\tFileSettingsEnd");

            // Add and compile info file
            if (!AddInfoFile(archiveDef, workdir, infoFile)) {
                return false;
            }

            // Add the info preview file
            AddLocalFile(archiveDef, BattlegroundsInstance.GetRelativePath(BattlegroundsPaths.MOD_ART_FOLDER, "..\\coh2_battlegrounds_wincondition_preview.dds"), "info\\", workdir);

            // Locale TOC section
            archiveDef.AppendLine("TOCEnd");
            archiveDef.AppendLine("TOCStart name=\"locale\" alias=\"locale\" path=\"\" relativeroot=\"locale\"");
            archiveDef.AppendLine("\tFileSettingsStart defverification=\"crc_blocks\" defcompression=\"stream_compress\"");
            archiveDef.AppendLine("\tFileSettingsEnd");

            // Add and compile locale files
            foreach (WinconoditionSourceFile file in localeFiles) {
                if (!AddFile(archiveDef, string.Empty, workdir, file, true, Encoding.Unicode)) {
                    return false;
                }
            }

            // Add end TOC
            archiveDef.AppendLine("TOCEnd");

            // Generate the path
            string archiveDefTxtPath = workdir + "ArchiveDefinition.txt";

            // Save the archive definition
            archiveDef.Save(archiveDefTxtPath);

            // The output archive
            string outputArchive = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\my games\\Company of Heroes 2\\mods\\gamemode\\coh2_battlegrounds_wincondition.sga";

            // Call the archive
            if (!Archiver.Archive(archiveDefTxtPath, workdir, outputArchive)) {
                return false;
            }

#if RELEASE
            // Delete the temp_build directory
            Directory.Delete(workdir, true);
#endif

            // Return true
            return true;

        }

        private static void CreateWorkspace(string workdir) {

            // Clear the directory we're going to work in
            if (Directory.Exists(workdir)) {
                Directory.Delete(workdir, true);
            }

            // Create directories
            Directory.CreateDirectory(workdir);
            Directory.CreateDirectory($"{workdir}data\\");
            Directory.CreateDirectory($"{workdir}data\\game");
            Directory.CreateDirectory($"{workdir}data\\game\\winconditions");
            Directory.CreateDirectory($"{workdir}data\\scar");
            Directory.CreateDirectory($"{workdir}data\\scar\\winconditions");
            Directory.CreateDirectory($"{workdir}data\\scar\\winconditions\\auxiliary_scripts");
            Directory.CreateDirectory($"{workdir}data\\scar\\winconditions\\ui_api");
            Directory.CreateDirectory($"{workdir}data\\ui\\");
            Directory.CreateDirectory($"{workdir}data\\ui\\Assets\\");
            Directory.CreateDirectory($"{workdir}data\\ui\\Assets\\Textures\\");
            Directory.CreateDirectory($"{workdir}data\\ui\\Bin\\");
            Directory.CreateDirectory($"{workdir}info");
            Directory.CreateDirectory($"{workdir}locale");
            Directory.CreateDirectory($"{workdir}locale\\english");

        }

        private static bool AddFile(TxtBuilder builder, string rpath, string workdir, WinconoditionSourceFile sourceFile, bool useBytes = false, Encoding encoding = null) {

            if (sourceFile.contents == null || sourceFile.contents.Length == 0) {
                return false;
            }

            string relpath = rpath + sourceFile.path;
            string abspath = Path.GetFullPath(workdir + relpath.Replace("/", "\\"));

            Trace.WriteLine($"Adding file [ABS] <{abspath}>", "Wincondition-Compiler");
            Trace.WriteLine($"\t[REL] <{relpath}>", "Wincondition-Compiler");
            Trace.WriteLine($"\t[RPA] <{rpath}>", "Wincondition-Compiler");
            Trace.WriteLine($"\t[SFP] <{sourceFile.path}>", "Wincondition-Compiler");

            builder.AppendLine($"\t{abspath}");

            if (File.Exists(abspath)) {
                File.Delete(abspath);
            }

            if (useBytes) {
                File.WriteAllBytes(abspath, sourceFile.contents);
            } else {
                File.WriteAllText(abspath, encoding.GetString(sourceFile.contents));
            }

            return true;

        }

        private static string path_cut = FixDebugString("https://raw.githubusercontent.com/JustCodiex/coh2-battlegrounds/scar-release-branch/coh2-battlegrounds-mod/wincondition_mod/");

        [Obsolete]
        private static bool AddURLFile(TxtBuilder builder, string rpath, string workdir, string file, bool useBytes = false, Encoding encoding = null, byte[] prepend = null) {

            byte[] binaryContent;

            if (encoding == null) {
                encoding = Encoding.UTF8;
            }

            file = FixDebugString(file);

            if (useBytes) {
                binaryContent = SourceDownloader.DownloadSourceFile(file, encoding);
            } else {
                binaryContent = encoding.GetBytes(SourceDownloader.DownloadSourceCode(file));
            }

            if (binaryContent == null || binaryContent.Length == 0) {
                return false;
            }

            string relpath = rpath + file.Substring(path_cut.Length);
            string abspath = Path.GetFullPath(workdir + relpath.Replace("/", "\\"));

            builder.AppendLine($"\t{abspath}");

            if (File.Exists(abspath)) {
                File.Delete(abspath);
            }

            using (BinaryWriter writer = new BinaryWriter(File.OpenWrite(abspath), encoding)) {
                if (prepend != null) {
                    writer.Write(prepend);
                }
                writer.Write(binaryContent);
            }

            return true;

        }

        private static bool AddInfoFile(TxtBuilder builder, string workdir, WinconoditionSourceFile file) {

            string relpath = file.path;
            string abspath = Path.GetFullPath(workdir + relpath.Replace("/", "\\"));

            builder.AppendLine($"\t{abspath}");

            File.WriteAllBytes(abspath, file.contents);

            return true;

        }

        private static void AddLocalFile(TxtBuilder builder, string localfile, string relpath, string workdir) {

            // Get path to copy file to
            string copyFile = Path.GetFullPath($"{workdir}{relpath}{ Path.GetFileName(localfile)}");

            // Add the local file
            builder.AppendLine($"\t{copyFile}");

            // Copy file
            File.Copy(Path.GetFullPath(localfile), copyFile);

        }

        private static void AddGraphics(TxtBuilder builder, string workdir) {

            // Add the gfx file
            string gfxabspath = Path.GetFullPath($"{workdir}data\\ui\\Bin\\6a0a13b89555402ca75b85dc30f5cb04.gfx");
            File.Copy(Path.GetFullPath(BattlegroundsInstance.GetRelativePath(BattlegroundsPaths.MOD_ART_FOLDER, "6a0a13b89555402ca75b85dc30f5cb04.gfx")), gfxabspath);
            builder.AppendLine($"\t{gfxabspath}");

            // Get DDS files
            string[] ddsFiles = Directory.GetFiles(BattlegroundsInstance.GetRelativePath(BattlegroundsPaths.MOD_ART_FOLDER, string.Empty), "*.dds");

            // Add DDS files
            for (int i = 0; i < ddsFiles.Length; i++) {
                string ddspath = Path.GetFullPath($"{workdir}data\\ui\\Assets\\Textures\\{Path.GetFileName(ddsFiles[i])}");
                File.Copy(Path.GetFullPath(ddsFiles[i]), ddspath);
                builder.AppendLine($"\t{ddspath}");
            }

        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0022:Use expression body for methods", Justification = "Compile-mode dependant")]
        private static string FixDebugString(string input) {
#if DEBUG
            return input.Replace("scar-release-branch", "scar-dev-branch");
#else
            return input;
#endif
        }

    }

}
