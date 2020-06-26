using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Diagnostics;
using Battlegrounds.Online;
using Battlegrounds.Util;

namespace Battlegrounds.Compiler {
    
    /// <summary>
    /// 
    /// </summary>
    public static class WinconditionCompiler {

        // URLs to the most up-to-date files (Will need a separate branch for this later)
        private static string[] DownloadFiles = new string[] {
            
            // Scar files
            "https://raw.githubusercontent.com/JustCodiex/coh2-battlegrounds/master/coh2-battlegrounds-mod/wincondition_mod/auxiliary_scripts/shared_util.scar",
            "https://raw.githubusercontent.com/JustCodiex/coh2-battlegrounds/master/coh2-battlegrounds-mod/wincondition_mod/auxiliary_scripts/shared_units.scar",
            "https://raw.githubusercontent.com/JustCodiex/coh2-battlegrounds/master/coh2-battlegrounds-mod/wincondition_mod/auxiliary_scripts/shared_handler.scar",
            "https://raw.githubusercontent.com/JustCodiex/coh2-battlegrounds/master/coh2-battlegrounds-mod/wincondition_mod/auxiliary_scripts/shared_companyloader.scar",
            "https://raw.githubusercontent.com/JustCodiex/coh2-battlegrounds/master/coh2-battlegrounds-mod/wincondition_mod/auxiliary_scripts/shared_companyloader.scar",
            "https://raw.githubusercontent.com/JustCodiex/coh2-battlegrounds/master/coh2-battlegrounds-mod/wincondition_mod/auxiliary_scripts/client_companyui.scar",
            "https://raw.githubusercontent.com/JustCodiex/coh2-battlegrounds/master/coh2-battlegrounds-mod/wincondition_mod/coh2_battlegrounds_annihilate.scar",
            "https://raw.githubusercontent.com/JustCodiex/coh2-battlegrounds/master/coh2-battlegrounds-mod/wincondition_mod/coh2_battlegrounds_vp.scar",

            // Win files
            "https://raw.githubusercontent.com/JustCodiex/coh2-battlegrounds/master/coh2-battlegrounds-mod/wincondition_mod/coh2_battlegrounds_annihilate.win",
            "https://raw.githubusercontent.com/JustCodiex/coh2-battlegrounds/master/coh2-battlegrounds-mod/wincondition_mod/coh2_battlegrounds_vp.win",

            // Info files
            "https://raw.githubusercontent.com/JustCodiex/coh2-battlegrounds/master/coh2-battlegrounds-mod/wincondition_mod/coh2_battlegrounds_wincondition%20Intermediate%20Cache/Intermediate%20Files/info/6a0a13b89555402ca75b85dc30f5cb04.info",

            // Locale file
            "https://raw.githubusercontent.com/JustCodiex/coh2-battlegrounds/master/coh2-battlegrounds-mod/wincondition_mod/locale/english/english.ucs",
        
        };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="workdir"></param>
        /// <param name="companyfile"></param>
        /// <returns></returns>
        public static bool CompileToSga(string workdir, string companyfile) {

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
            string[] winfiles = DownloadFiles[8..10];

            // Add and compile scar files
            foreach (string file in winfiles) {
                if (!AddURLFile(archiveDef, "data\\game\\winconditions\\", workdir, file)) {
                    return false;
                }
            }

            // Add the company file
            AddLocalFile(archiveDef, companyfile, "data\\scar\\winconditions\\auxiliary_scripts\\", workdir);

            // Get the scar files
            string[] scarfiles = DownloadFiles[0..8];

            // Add and compile scar files
            foreach (string file in scarfiles) {
                if (!AddURLFile(archiveDef, "data\\scar\\winconditions\\", workdir, file)) {
                    return false;
                }
            }

            // Info TOC section
            archiveDef.AppendLine("TOCEnd");
            archiveDef.AppendLine("TOCStart name=\"info\" alias=\"info\" path=\"\" relativeroot=\"info\"");
            archiveDef.AppendLine("\tFileSettingsStart defverification=\"crc_blocks\" defcompression=\"stream_compress\"");
            archiveDef.AppendLine("\t\tOverride wildcard=\".*\\.(lua)$\" minsize=\"-1\" maxsize=\"-1\" vt=\"crc_blocks\" ct=\"buffer_compress\"");
            archiveDef.AppendLine("\tFileSettingsEnd");

            // Add and compile info file
            if (!AddInfoFile(archiveDef, workdir, DownloadFiles[10])) {
                return false;
            }

            // Add the info preview file
            AddLocalFile(archiveDef, "coh2_battlegrounds_wincondition_preview.dds", "info\\", workdir);

            // Locale TOC section
            archiveDef.AppendLine("TOCEnd");
            archiveDef.AppendLine("TOCStart name=\"locale\" alias=\"locale\" path=\"\" relativeroot=\"locale\"");
            archiveDef.AppendLine("\tFileSettingsStart defverification=\"crc_blocks\" defcompression=\"stream_compress\"");
            archiveDef.AppendLine("\tFileSettingsEnd");

            // Get all locale files
            string[] localefiles = DownloadFiles[11..^0];

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

            } catch (Exception e) {
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


            // Get path to copy company file to
            string companyCopyFile = Path.GetFullPath($"{workdir}{relpath}{ Path.GetFileName(localfile)}");

            // Add the company scarfile
            builder.AppendLine($"\t{companyCopyFile}");

            // Copy scar file
            File.Copy(Path.GetFullPath(localfile), companyCopyFile);


        }

    }

}
