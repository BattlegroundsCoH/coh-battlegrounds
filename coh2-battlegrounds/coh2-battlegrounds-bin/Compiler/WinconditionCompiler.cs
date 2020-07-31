using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Diagnostics;

using Battlegrounds.Online;
using Battlegrounds.Util;

namespace Battlegrounds.Compiler {
    
    /// <summary>
    /// Helper class for compiling a win condition in a JIT-style.
    /// </summary>
    public static class WinconditionCompiler {

        // URLs to the most up-to-date scar files (Will need a separate branch for this later)
        private static string[] ScarFiles = new string[] {
            "https://raw.githubusercontent.com/JustCodiex/coh2-battlegrounds/scar-release-branch/coh2-battlegrounds-mod/wincondition_mod/auxiliary_scripts/shared_util.scar",
            "https://raw.githubusercontent.com/JustCodiex/coh2-battlegrounds/scar-release-branch/coh2-battlegrounds-mod/wincondition_mod/auxiliary_scripts/shared_units.scar",
            "https://raw.githubusercontent.com/JustCodiex/coh2-battlegrounds/scar-release-branch/coh2-battlegrounds-mod/wincondition_mod/auxiliary_scripts/shared_handler.scar",
            "https://raw.githubusercontent.com/JustCodiex/coh2-battlegrounds/scar-release-branch/coh2-battlegrounds-mod/wincondition_mod/auxiliary_scripts/shared_sessionloader.scar",
            "https://raw.githubusercontent.com/JustCodiex/coh2-battlegrounds/scar-release-branch/coh2-battlegrounds-mod/wincondition_mod/auxiliary_scripts/shared_ai.scar",
            "https://raw.githubusercontent.com/JustCodiex/coh2-battlegrounds/scar-release-branch/coh2-battlegrounds-mod/wincondition_mod/auxiliary_scripts/client_companyui.scar",
            "https://raw.githubusercontent.com/JustCodiex/coh2-battlegrounds/scar-release-branch/coh2-battlegrounds-mod/wincondition_mod/auxiliary_scripts/client_overrideui.scar",
            "https://raw.githubusercontent.com/JustCodiex/coh2-battlegrounds/scar-release-branch/coh2-battlegrounds-mod/wincondition_mod/auxiliary_scripts/api_ui.scar",
            "https://raw.githubusercontent.com/JustCodiex/coh2-battlegrounds/scar-release-branch/coh2-battlegrounds-mod/wincondition_mod/ui_api/button.scar",
            "https://raw.githubusercontent.com/JustCodiex/coh2-battlegrounds/scar-release-branch/coh2-battlegrounds-mod/wincondition_mod/ui_api/class.scar",
            "https://raw.githubusercontent.com/JustCodiex/coh2-battlegrounds/scar-release-branch/coh2-battlegrounds-mod/wincondition_mod/ui_api/color.scar",
            "https://raw.githubusercontent.com/JustCodiex/coh2-battlegrounds/scar-release-branch/coh2-battlegrounds-mod/wincondition_mod/ui_api/control.scar",
            "https://raw.githubusercontent.com/JustCodiex/coh2-battlegrounds/scar-release-branch/coh2-battlegrounds-mod/wincondition_mod/ui_api/icon.scar",
            "https://raw.githubusercontent.com/JustCodiex/coh2-battlegrounds/scar-release-branch/coh2-battlegrounds-mod/wincondition_mod/ui_api/label.scar",
            "https://raw.githubusercontent.com/JustCodiex/coh2-battlegrounds/scar-release-branch/coh2-battlegrounds-mod/wincondition_mod/ui_api/panel.scar",
            "https://raw.githubusercontent.com/JustCodiex/coh2-battlegrounds/scar-release-branch/coh2-battlegrounds-mod/wincondition_mod/ui_api/rootpanel.scar",
            "https://raw.githubusercontent.com/JustCodiex/coh2-battlegrounds/scar-release-branch/coh2-battlegrounds-mod/wincondition_mod/ui_api/statusindicator.scar",
            "https://raw.githubusercontent.com/JustCodiex/coh2-battlegrounds/scar-release-branch/coh2-battlegrounds-mod/wincondition_mod/coh2_battlegrounds.scar",
        };

        // URLs to the most up-to-date win files (Will need a separate branch for this later)
        private static string[] WinFiles = new string[] {
            "https://raw.githubusercontent.com/JustCodiex/coh2-battlegrounds/scar-release-branch/coh2-battlegrounds-mod/wincondition_mod/coh2_battlegrounds.win",
        };

        // URLs to the most up-to-date info file (Will need a separate branch for this later)
        private static string InfoFile = "https://raw.githubusercontent.com/JustCodiex/coh2-battlegrounds/scar-release-branch/coh2-battlegrounds-mod/wincondition_mod/coh2_battlegrounds_wincondition%20Intermediate%20Cache/Intermediate%20Files/info/6a0a13b89555402ca75b85dc30f5cb04.info";

        // URLs to the most up-to-date locale files (Will need a separate branch for this later)
        private static string[] LocaleFiles = new string[] {
            "https://raw.githubusercontent.com/JustCodiex/coh2-battlegrounds/scar-release-branch/coh2-battlegrounds-mod/wincondition_mod/locale/english/english.ucs",
        };

        /// <summary>
        /// Compile a session into a sga archive file.
        /// </summary>
        /// <param name="workdir">The temporary work directory.</param>
        /// <param name="sessionFile">The session file to include</param>
        /// <returns>True of the archive file was created sucessfully. False if any error occured.</returns>
        public static bool CompileToSga(string workdir, string sessionFile) {

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

            // Get the scar files
            string[] winfiles = WinFiles;

            // Add and compile scar files
            foreach (string file in winfiles) {
                if (!AddURLFile(archiveDef, "data\\game\\winconditions\\", workdir, file)) {
                    return false;
                }
            }

            // Add the company file
            AddLocalFile(archiveDef, sessionFile, "data\\scar\\winconditions\\auxiliary_scripts\\", workdir);

            // Get the scar files
            string[] scarfiles = ScarFiles;

            // Add and compile scar files
            foreach (string file in scarfiles) {
                if (!AddURLFile(archiveDef, "data\\scar\\winconditions\\", workdir, file)) {
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
            if (!AddInfoFile(archiveDef, workdir, InfoFile)) {
                return false;
            }

            // Add the info preview file
            AddLocalFile(archiveDef, BattlegroundsInstance.GetRelativePath(BattlegroundsPaths.MOD_ART_FOLDER, "coh2_battlegrounds_wincondition_preview.dds"), "info\\", workdir);

            // Locale TOC section
            archiveDef.AppendLine("TOCEnd");
            archiveDef.AppendLine("TOCStart name=\"locale\" alias=\"locale\" path=\"\" relativeroot=\"locale\"");
            archiveDef.AppendLine("\tFileSettingsStart defverification=\"crc_blocks\" defcompression=\"stream_compress\"");
            archiveDef.AppendLine("\tFileSettingsEnd");

            // Get all locale files
            string[] localefiles = LocaleFiles;

            // Add and compile info files
            foreach (string file in localefiles) {
                if (!AddURLFile(archiveDef, "", workdir, file, true)) {
                    return false;
                }
            }

            // Add end TOC
            archiveDef.AppendLine("TOCEnd");

            // Generate the path
            string archiveDefTxtPath = workdir + "ArchiveDefinition.txt";

            // Save the archive definition
            archiveDef.Save(archiveDefTxtPath);

            string outputArchive = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\my games\\Company of Heroes 2\\mods\\gamemode\\coh2_battlegrounds_wincondition.sga";

            // Call the archive
            if (!InvokeArchiver(archiveDefTxtPath, workdir, outputArchive)) {
                return false;
            }

#if RELEASE
            // Delete the temp_build directory
            Directory.Delete(workdir, true);
#endif

            // Return true
            return true;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="archdef"></param>
        /// <param name="relativepath"></param>
        /// <param name="output"></param>
        /// <returns></returns>
        public static bool InvokeArchiver(string archdef, string relativepath, string output) {

            string cmdarg = $" -c \"{archdef}\" -a \"{output}\" -v -r \"{relativepath}\\\"";

            Process archiveProcess = new Process {
                StartInfo = new ProcessStartInfo() {
                    FileName = Pathfinder.GetOrFindCoHPath() + "Archive.exe",
                    Arguments = cmdarg,
                    RedirectStandardOutput = true,
                    RedirectStandardInput = false,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                },
                EnableRaisingEvents = true,
            };

            archiveProcess.OutputDataReceived += ArchiveProcess_OutputDataReceived;

            try {

                if (!archiveProcess.Start()) {
                    archiveProcess.Dispose();
                    return false;
                } else {
                    archiveProcess.BeginOutputReadLine();
                }

                Thread.Sleep(1000);

                do {
                    Thread.Sleep(100);
                } while (!archiveProcess.HasExited);

                if (archiveProcess.ExitCode != 0) {
                    int eCode = archiveProcess.ExitCode;
                    Trace.WriteLine($"Archiver has finished with error code = {eCode}");
                    archiveProcess.Dispose();
                    return false;
                }

            } catch {
                archiveProcess.Dispose();
                return false;
            }

            archiveProcess.Dispose();

            return true;

        }

        private static void ArchiveProcess_OutputDataReceived(object sender, DataReceivedEventArgs e) {
            if (e.Data != null && e.Data != string.Empty && e.Data != " ")
                Console.WriteLine($"{e.Data}");
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

        private static string path_cut = "https://raw.githubusercontent.com/JustCodiex/coh2-battlegrounds/master/coh2-battlegrounds-mod/wincondition_mod/";

        private static bool AddURLFile(TxtBuilder builder, string rpath, string workdir, string file, bool useBytes = false) {

            string fileContent;

            if (useBytes) {
                fileContent = Encoding.UTF8.GetString(SourceDownloader.DownloadSourceFile(file));
            } else {
                fileContent = SourceDownloader.DownloadSourceCode(file);
            }

            if (fileContent == string.Empty) {
                return false;
            }

            string relpath = rpath + file.Substring(path_cut.Length);
            string abspath = Path.GetFullPath(workdir + relpath.Replace("/", "\\"));

            builder.AppendLine($"\t{abspath}");

            File.WriteAllText(abspath, fileContent);

            return true;

        }

        private static bool AddInfoFile(TxtBuilder builder, string workdir, string file) {

            string fileContent = SourceDownloader.DownloadSourceCode(file);
            if (fileContent == string.Empty) {
                return false;
            }

            string relpath = file.Substring("https://raw.githubusercontent.com/JustCodiex/coh2-battlegrounds/master/coh2-battlegrounds-mod/wincondition_mod/coh2_battlegrounds_wincondition%20Intermediate%20Cache/Intermediate%20Files".Length);
            string abspath = Path.GetFullPath(workdir + relpath.Replace("/", "\\"));

            builder.AppendLine($"\t{abspath}");

            File.WriteAllText(abspath, fileContent);

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
            File.Copy(Path.GetFullPath("6a0a13b89555402ca75b85dc30f5cb04.gfx"), gfxabspath);
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

    }

}
