using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using Battlegrounds;
using Battlegrounds.AI;
using Battlegrounds.AI.Lobby;
using Battlegrounds.Functional;
using Battlegrounds.Game.Database;
using Battlegrounds.Game.Database.Management;
using Battlegrounds.Game.DataCompany;
using Battlegrounds.Game.DataSource.Replay;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Game.Match;
using Battlegrounds.Game.Match.Data;
using Battlegrounds.Gfx;
using Battlegrounds.Modding;
using Battlegrounds.Modding.Content.Companies;
using Battlegrounds.Networking;
using Battlegrounds.Verification;

using Renci.SshNet;

namespace coh2_battlegrounds_console;

class Program {

    /// <summary>
    /// Represents the GfxVerify flag command.
    /// </summary>
    class GfxVerify : Command {
        
        public static readonly Argument<string> PATH = new Argument<string>("-f", "Specifies the file to verify integrity of.", string.Empty);

        public GfxVerify() : base("gfxverify", "Verifies the integrity of a gfx file.", PATH) { }

        public override void Execute(CommandArgumentList argumentList) {

            // Grab target path
            var target = argumentList.GetValue(PATH);

            try {

                // Try read
                GfxMap map = GfxMap.FromBinary(File.OpenRead(target));

                // Log details if read
                if (map is null) {
                    Console.WriteLine($"Failed to read {target}");
                    return;
                }

                // Do stuff
                Console.WriteLine("Successfully parsed gfx file:");
                Console.WriteLine("\tBinary Version: " + map.BinaryVersion);
                Console.WriteLine("\tResource count: " + map.Resources.Length);

            } catch (Exception ex) {
                Console.WriteLine(ex);
            }

        }

    }

    class GfxCompile : Command {
        
        public static readonly Argument<string> DIR = new Argument<string>("-d", "Specifies the directory to compile.", string.Empty);
        public static readonly Argument<string> OUT = new Argument<string>("-o", "Specifies the name of the file to output gfx map to.", "gfx.dat");
        public static readonly Argument<int> VER = new Argument<int>("-v", "Specifies the gfx version to target.", GfxMap.GfxBinaryVersion);
        public static readonly Argument<string> REG = new Argument<string>("-r", "Specifies regex pattern to select specific files in folder to compile.", string.Empty);

        public GfxCompile() : base("gfxdir", "Compiles directory gfx content into a gfx data file.", DIR, OUT, VER, REG) { }

        public override void Execute(CommandArgumentList argumentList) 
            => GfxFolderCompiler.Compile(argumentList.GetValue(DIR), argumentList.GetValue(OUT), version: argumentList.GetValue(VER));

    }

    class TestCompanies : Command {

        public static readonly Argument<bool> SOV = new Argument<bool>("-s", "Specifies the Soviet company should *NOT* be generated.", true);
        public static readonly Argument<bool> GER = new Argument<bool>("-g", "Specifies the German company should *NOT* be generated.", true);
        public static readonly Argument<bool> RAW = new Argument<bool>("-raw", "Specifies the output format should not be formatted.", true);

        public TestCompanies() : base("company-test", "Compiles a German and Soviet test company using the most up-to-date version.", SOV, GER, RAW) { }

        public override void Execute(CommandArgumentList argumentList) {

            LoadBGAndProceed();

            if (argumentList.GetValue(SOV))
                File.WriteAllText("26th_Rifle_Division.json", CompanySerializer.GetCompanyAsJson(CreateSovietCompany(), argumentList.GetValue(RAW)));

            if (argumentList.GetValue(GER))
                File.WriteAllText("69th_panzer.json", CompanySerializer.GetCompanyAsJson(CreateGermanCompany(), argumentList.GetValue(RAW)));

        }

    }

    class MapExtract : Command {

        public static readonly Argument<string> PATH = new Argument<string>("-o", "Specifies output directory.", "archive_maps");

        public static readonly Argument<bool> TEST = new Argument<bool>("-t", "Specifies if the testmap should be read instead", false);

        public MapExtract() : base("coh2-extract-maps", "Extracts all CoH2 maps.", PATH, TEST) { }

        public override void Execute(CommandArgumentList argumentList) {

            if (argumentList.GetValue(TEST)) {
                MapExtractor.ReadTestmap();
                return;
            }

            MapExtractor.Output = argumentList.GetValue(PATH);
            MapExtractor.Extract();

        }

    }

    class CampaignCompile : Command {

        public static readonly Argument<string> PATH = new Argument<string>("-c", "Specifies the directory to compile", string.Empty);
        public static readonly Argument<string> OUT = new Argument<string>("-o", "Specifies compiled campaign output file.", "campaign.camp");

        public CampaignCompile() : base("campaign", "Compiles a campaign directory..", PATH, OUT) { }

        public override void Execute(CommandArgumentList argumentList) {

            LoadBGAndProceed();
           // CampaignCompiler.Output = argumentList.GetValue(PATH);
            CampaignCompiler.Compile(argumentList.GetValue(OUT));

        }

    }

    class ReplayAnalysis : Command {

        public static readonly Argument<string> PATH = 
            new Argument<string>("-playback", "Specifies the playback file to analyse.", File.Exists("temp.rec") ? "temp.rec" : ReplayMatchData.LATEST_REPLAY_FILE);
        public static readonly Argument<bool> JSON = new Argument<bool>("-json", "Specifies the analysis should be saved to a json file.", false);

        public ReplayAnalysis() : base("replay", "Runs an analysis of the most recent replay file (or otherwise specified file).", PATH, JSON) { }

        public override void Execute(CommandArgumentList argumentList) {

            var target = argumentList.GetValue(PATH);
            var compile_json = argumentList.GetValue(JSON);

            LoadBGAndProceed();

            Console.WriteLine("Parsing latest replay file");
            ReplayFile replayFile = new ReplayFile(target);
            Console.WriteLine($"Load Replay: {replayFile.LoadReplay()}");
            Console.WriteLine($"Partial: {replayFile.IsPartial}");
            ReplayMatchData playback = new ReplayMatchData(new NullSession());
            playback.SetReplayFile(replayFile);
            if (!playback.ParseMatchData()) {
                Console.WriteLine("Failed to parse match data");
                return;
            }

            if (compile_json) {
                Console.WriteLine("Compiling to json playback");
                JsonPlayback events = new JsonPlayback(playback);
                File.WriteAllText("playback.json", events.ToJson());
                Console.WriteLine("Saved to replay analysis to playback.json");
            }

        }

    }

    class ServerCheck : Command {

        public ServerCheck() : base("server-check", "Will send a ping request to the server and time the response time.") { }

        public override void Execute(CommandArgumentList argumentList) {
            var watch = Stopwatch.StartNew();
            var response = NetworkInterface.RemoteReleaseEndpoint.IsConnectable();
            watch.Stop();
            Console.WriteLine($"Server response: {response} - ping: {watch.ElapsedMilliseconds}ms");
        }

    }

    class CreateInstaller : Command {

        const string msbuild_path = @"C:\Program Files\Microsoft Visual Studio\2022\Community\Msbuild\Current\Bin";

        public static readonly Argument<string> PLATFORM = new Argument<string>("-p", "Specifies the build platform", "x64");

        public CreateInstaller() : base("create-installer", "Creates a .msi file", PLATFORM) { }

        public override void Execute(CommandArgumentList argumentList) {

            string platform = argumentList.GetValue(PLATFORM);
            if (platform != "x64" && platform != "x86") {
                Console.WriteLine("Invalid platform");
                return;
            }

            string workdir = Path.GetFullPath("..\\..\\..\\..\\");

            // Create publish files 
            ProcessStartInfo createPublishFiles = new ProcessStartInfo() {
                UseShellExecute = false,
                FileName = Path.Combine(workdir, "coh2-battlegrounds-console\\bin\\Debug\\net6.0\\coh2-battlegrounds-console.exe"),
                Arguments = $"mki -p {platform} -dz",
                WorkingDirectory = Path.Combine(workdir, "coh2-battlegrounds-console\\bin\\Debug\\net6.0\\"),
            };

            Process? createPublishFilesProcess = Process.Start(createPublishFiles);
            if (createPublishFilesProcess is null) {
                Console.WriteLine($"Failed to create coh2-battlegrounds-console.exe mki -p {platform} -dz process");
                return;
            }

            createPublishFilesProcess!.WaitForExit();

            if (!CompileProject("coh2-battlegrounds-installer.wixproj", Path.Combine(workdir, "coh2-battlegrounds-installer"), platform)) {
                return;
            }

        }

        private bool CompileProject(string project, string projectDir, string platform) {

            // Firstly we trigger MSBuild.exe on our solution file
            ProcessStartInfo msbuild_bin = new ProcessStartInfo() {
                UseShellExecute = false,
                FileName = $"{msbuild_path}\\MSBuild.exe",
                Arguments = $"{project} -property:Configuration=Release -property:Platform={platform}",
                WorkingDirectory = projectDir,
            };

            // Trigger compile
            Process? binbuilder = Process.Start(msbuild_bin);
            if (binbuilder is null) {
                Console.WriteLine("Failed to create MS build process");
                return false;
            }

            // Grab io
            binbuilder.OutputDataReceived += (s, e) => {
                Console.WriteLine($"msbuild: {e.Data}");
            };
            binbuilder.ErrorDataReceived += (s, e) => {
                Console.WriteLine($"error: {e.Data}");
            };

            // Wait for exit
            binbuilder.WaitForExit();

            // Return OK
            return true;

        }

    }

    class ExecutableFirewall : Command {

        public ExecutableFirewall() : base("add-firewallexeption", "Adds FirewallExceptio to executable") { }

        public override void Execute(CommandArgumentList argumentList) {

            string workdir = Path.GetFullPath("..\\..\\..\\..\\");

            string oldHeader = "<Wix xmlns=\"http://schemas.microsoft.com/wix/2006/wi\">";
            string newHeader = "<Wix xmlns=\"http://schemas.microsoft.com/wix/2006/wi\" xmlns:fire=\"http://schemas.microsoft.com/wix/FirewallExtension\">";

            string oldComponent = "<File Id=\"filC3836236C73ECA8506FB24920C9B8D22\" KeyPath=\"yes\" Source=\"$(var.BasePath)\\coh2-battlegrounds.exe\" />";
            string newComponent = "<File Id=\"filC3836236C73ECA8506FB24920C9B8D22\" KeyPath=\"yes\" Source=\"$(var.BasePath)\\coh2-battlegrounds.exe\"><fire:FirewallException Id=\"ExeFirewall\" Name=\"!(loc.ProductNameFolder)\" Port=\"11000\" Protocol=\"tcp\" Profile=\"public\" Scope=\"any\"/></File>";

            string file = Path.Combine(workdir, "coh2-battlegrounds-installer\\ComponentsGen.wxs");

            StreamReader reader = new(file);
            string content = reader.ReadToEnd();
            reader.Close();

            content = content.Replace(oldHeader, newHeader);
            content = content.Replace(oldComponent, newComponent);

            StreamWriter writer = new(file);
            writer.Write(content);
            writer.Close();

        }

    }

    class Update : Command {
        
        public static readonly string ChecksumPath = Path.GetFullPath("..\\..\\..\\..\\coh2-battlegrounds\\checksum.txt");

        public static readonly Argument<string> TARGET = new Argument<string>("-target", "Specifies the update target [db, checksum]", "db");

        public static readonly Argument<string> MOD = new Argument<string>("-m", "Specifies the mod to update, applicable when target=db", "vcoh");

        public static readonly Argument<string> TOOL = 
            new Argument<string>("-t", "Specifies the full path to the tools folder", @"C:\Program Files (x86)\Steam\steamapps\common\Company of Heroes 2 Tools");

        public static readonly Argument<string> PLATFORM = new Argument<string>("-p", "Specifies build platform", "x64");

        public Update() : base("update", "Will update specified elements of the source build.", PLATFORM, TARGET, MOD, TOOL) { }

        private ConsoleColor m_col;

        public override void Execute(CommandArgumentList argumentList) {
            // Flag to set the build platform
            string platform = argumentList.GetValue(PLATFORM);
            if (platform != "x64" && platform != "x86") {
                Console.WriteLine("Invalid platform");
                return;
            }

            this.m_col = Console.ForegroundColor;
            switch (argumentList.GetValue(TARGET)) {
                case "db":
                    this.UpdateDatabase(argumentList);
                    break;
                case "checksum":
                    ComputeChecksum(platform);
                    break;
                default:
                    Console.WriteLine("Undefined update target.");
                    break;
            }
        }

        private void Err(string msg) {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(msg);
            Console.ForegroundColor = this.m_col;
        }

        record LastUse(string OutPath, string InstancePath, string ModGuid, string ModName);

        private void UpdateDatabase(CommandArgumentList args) {
            
            // Grab mod target
            string mod = args.GetValue(MOD);

            // Get path to xaml to json tool
            string xmlreader = Path.GetFullPath("..\\..\\..\\..\\..\\db-tool\\bin\\debug\\net6.0\\CoH2XML2JSON.exe");

            // Verify
            if (!File.Exists(xmlreader)) {
                xmlreader = Path.GetFullPath("..\\..\\..\\..\\..\\db-tool\\bin\\debug\\net5.0\\CoH2XML2JSON.exe");
                if (!File.Exists(xmlreader)) {
                    this.Err("Fatal Error: Failed to locate xml2json executable");
                    return;
                }
            }

            // Store recent
            string recent = xmlreader.Replace("CoH2XML2JSON.exe", "last.json");

            // Get input dir
            string xmlinput = mod switch {
                "vcoh" => Path.GetFullPath(args.GetValue(TOOL) + @"\assets\data\attributes\instances"),
                "mod_bg" => Path.GetFullPath("..\\..\\..\\..\\..\\coh2-battlegrounds-mod\\tuning_mod\\instances"),
                _ => $"$DIR({mod})"
            };

            // Log and bail
            if (!Directory.Exists(xmlinput)) {
                this.Err($"Fatal Error: Directory {xmlinput} not found for mod {mod}");
                return;
            }

            // Get output dir
            string jsonoutput = mod switch {
                "vcoh" => Path.GetFullPath("..\\..\\..\\..\\coh2-battlegrounds\\bg_common\\data"),
                "mod_bg" => Path.GetFullPath("..\\..\\..\\..\\coh2-battlegrounds\\bg_common\\data"),
                _ => throw new NotSupportedException()
            };

            // Get GUID
            string guid = mod switch {
                "vcoh" => "",
                "mod_bg" => "142b113740474c82a60b0a428bd553d5",
                _ => throw new NotSupportedException()
            };

            // Log
            Console.WriteLine($"XML to Json converter: {xmlreader}");
            Console.WriteLine($"XML input: {xmlinput}");
            Console.WriteLine($"Json Out: {jsonoutput}");

            // Save
            File.WriteAllText(recent, JsonSerializer.Serialize(new LastUse(jsonoutput, xmlinput, guid, mod)));

            // Invoke
            var proc = Process.Start(new ProcessStartInfo(xmlreader, "-do_last") { WorkingDirectory = Path.GetDirectoryName(xmlreader)});
            proc?.WaitForExit();
            
            Console.WriteLine();
            Console.WriteLine(" -> Task DONE");

        }

        internal static void ComputeChecksum(string platform) {

            // Get path to release build
            string releaseExe = Path.GetFullPath($"..\\..\\..\\..\\coh2-battlegrounds\\bin\\Release\\net6.0-windows\\win-{platform}\\publish\\coh2-battlegrounds.exe");

            // Log
            Console.WriteLine($"Checksum file: {ChecksumPath}");
            Console.WriteLine($"Release Build: {releaseExe}");

            // Make sure release build exists
            if (!File.Exists(releaseExe)) {
                Console.WriteLine("Release executable not found - aborting!");
                return;
            }

            // Compute hash
            Integrity.CheckIntegrity(releaseExe);

            // Log hash
            Console.WriteLine($"Computed integrity hash as: {Integrity.IntegrityHashString}");
            Console.WriteLine($"Saving integrity hash to checksum file.");

            // Save
            File.WriteAllText(ChecksumPath, Integrity.IntegrityHashString);
            File.Copy(ChecksumPath, releaseExe.Replace("coh2-battlegrounds.exe", "checksum.txt"), true);

            // Log
            Console.WriteLine("Saved hash to checksum file(s).");

        }

    }

    class MinimapperCommand : Command {

        public static readonly Argument<string> SCENARIO = new Argument<string>("-s", "Specifies the scenario to map", "2p_coh2_resistance");

        public MinimapperCommand() : base("mini", "Basic minimap position translator functionality testing", SCENARIO) {}

        public override void Execute(CommandArgumentList argumentList) {

            // Load BG
            LoadBGAndProceed();

            // Get scenario
            string s = argumentList.GetValue(SCENARIO);
            if (!ScenarioList.TryFindScenario(s, out Scenario? scen) && scen is null) {
                Console.WriteLine("Failed to find scenario " + s);
                return;
            }

            // Invoke mapper
            Minimapper.Map(scen);

        }

    }

    class CreateInstallFiles : Command {

        const string dotnet_path = @"C:\Program Files\dotnet";

        public static readonly Argument<bool> SKIP_COMPILE = new Argument<bool>("-c", "Skip compilation (must be handled by user then)", false);
        public static readonly Argument<bool> DONT_ZIP = new Argument<bool>("-dz", "Don't zip the file", false);
        public static readonly Argument<string> PLATFORM = new Argument<string>("-p", "Specifies the build platform", "x64");

        public CreateInstallFiles() : base("mki", "Makes an installer file (Zips the release build folder).", PLATFORM, SKIP_COMPILE, DONT_ZIP) { }

        public override void Execute(CommandArgumentList argumentList) {

            // Flag to check if compilation should be skipped
            bool skipFlag = argumentList.GetValue(SKIP_COMPILE);

            // Flag to check if we should zip
            bool zipFlag = argumentList.GetValue(DONT_ZIP);

            // Flag to set the build platform
            string platform = argumentList.GetValue(PLATFORM);
            if (platform != "x64" && platform != "x86") {
                Console.WriteLine("Invalid platform");
                return;
            }

            // Make sure msbuild exists
            if (!File.Exists(dotnet_path + "\\dotnet.exe")) {
                Console.WriteLine("Failed to locate dotnet.exe");
                Console.WriteLine("Create install data file anyways (y/n)?");
                if (Console.Read() is (int)'Y' or (int)'y') {
                    skipFlag = true;
                } else {
                    return;
                }
            }

            // Find abs path for working directory
            string workdir = Path.GetFullPath("..\\..\\..\\..\\");

            // Check if compile flag is set
            if (!skipFlag) {

                // Compile aux library and bail if failure
                if (!CompileProject("coh2-battlegrounds-bin.csproj", Path.Combine(workdir, "coh2-battlegrounds-bin"), platform)) {
                    return;
                }

                // Compile main app
                if (!CompileProject("coh2-battlegrounds.csproj", Path.Combine(workdir, "coh2-battlegrounds"), platform)) {
                    return;
                }

            }

            // Update checksum
            Update.ComputeChecksum(platform);

            // Log
            Console.WriteLine();
            Console.WriteLine("Copying checksum file to release directory");

            // Copy
            File.Copy(Update.ChecksumPath, Path.Combine(workdir, $"coh2-battlegrounds\\bin\\Release\\net6.0-windows\\win-{platform}\\publish\\checksum.txt"), true);
            Console.WriteLine("Copied checksum file to release directory");

            if (zipFlag) {

                const string dir = "bg-release";
                if (Directory.Exists(dir)) {
                    Directory.Delete(dir, true);
                }

                Directory.CreateDirectory(dir);

                Helpers.CopyFilesRecursively(Path.Combine(workdir, $"coh2-battlegrounds\\bin\\Release\\net6.0-windows\\win-{platform}\\publish"), dir);
                Console.WriteLine($"Created {dir} successfully");


            } else {

                Console.WriteLine("\tNow packing into zipfile...");

                // Define zip path
                const string zippy = "bg-release.zip";
                if (File.Exists(zippy)) {
                    File.Delete(zippy);
                }

                // Creaste zip
                ZipFile.CreateFromDirectory(Path.Combine(workdir, $"coh2-battlegrounds\\bin\\Release\\net6.0-windows\\win-{platform}\\publish"), zippy);
                Console.WriteLine("Zip file created");

            }

        }

        private static bool CompileProject(string project, string projectDir, string platform) {

            // Firstly we trigger MSBuild.exe on our solution file
            ProcessStartInfo msbuild_bin = new ProcessStartInfo() {
                UseShellExecute = false,
                FileName = $"{dotnet_path}\\dotnet.exe",
                Arguments = $"publish {project} -r win-{platform} -c Release",
                WorkingDirectory = projectDir,
            };

            // Trigger compile
            Process? binbuilder = Process.Start(msbuild_bin);
            if (binbuilder is null) {
                Console.WriteLine("Failed to create DOTNET build process");
                return false;
            }

            // Grab io
            binbuilder.OutputDataReceived += (s, e) => {
                Console.WriteLine($"dotnet: {e.Data}");
            };
            binbuilder.ErrorDataReceived += (s, e) => {
                Console.WriteLine($"error: {e.Data}");
            };

            // Wait for exit
            binbuilder.WaitForExit();

            // Return OK
            return true;

        }

    }

    class AIDefenceAnalyser : Command {

        public static readonly Argument<string> SCENARIO = new Argument<string>("-s", "Specifies the scenario to map", "2p_coh2_resistance");

        public static readonly Argument<bool> DOALL = new Argument<bool>("-a", "Specifies the tool should be applied to all known maps",  false);

        public AIDefenceAnalyser() : base("aidef", "Invokes the AI defence minimap analysis tool.", SCENARIO, DOALL) { }

        public override void Execute(CommandArgumentList argumentList) {

            // Load BG
            LoadBGAndProceed();

            // Do all if flag is set; otherwise just do example scenario
            if (argumentList.GetValue(DOALL)) {

                // Grab all scenarios
                var scenarios = ScenarioList.GetList();

                // Create required instances
                var ls = new Dictionary<string, AIMapAnalysis>();
                var sync = new object();

                // Time it
                var stop = Stopwatch.StartNew();

                // Loop over
                Parallel.ForEach(scenarios, x => {
                    if (DoScenario(x) is AIMapAnalysis a) {
                        lock (sync) {
                            ls[x.RelativeFilename] = a;
                        }
                    }
                });

                // Halt
                stop.Stop();

                // Log
                Console.WriteLine($"Finished processing {scenarios.Count} scenarios in {stop.Elapsed.TotalSeconds}s");
                Thread.Sleep(5000);

                File.WriteAllText($"vcoh-aimap-db.json", JsonSerializer.Serialize(ls, new JsonSerializerOptions() { WriteIndented = true }));

            } else {

                // Get scenario
                string s = argumentList.GetValue(SCENARIO);
                if (!ScenarioList.TryFindScenario(s, out Scenario? scen) && scen is null) {
                    Console.WriteLine("Failed to find scenario " + s);
                    return;
                }

                // Do the scenario
                DoScenario(scen);

            }

        }

        [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "Will always run on Windows")]
        private static AIMapAnalysis? DoScenario(Scenario scen) {

            // Log
            Console.WriteLine($"Doing Analysis on: {scen.RelativeFilename}.");

            // Create AI map analyser
            var analyser = new AIMapAnalyser(scen);
            if (analyser.Analyze(out var data, 8, 3) is not AIMapAnalysis analysis) {
                Console.WriteLine("Failed to analyse scenario file: " + scen.Name);
                return null;
            }

            // Grab bitmap
            using var bmp = new Bitmap(data.GetLength(0),
                                       data.GetLength(1),
                                       PixelFormat.Format32bppArgb);

            // Draw bmp
            for (int y = 0; y < bmp.Height; y++) {
                for (int x = 0; x < bmp.Width; x++) {
                    bmp.SetPixel(x, y, Color.FromArgb(data[x, y].R, data[x, y].G, data[x, y].B));
                }
            }

            // Draw analysis data
            using var g = Graphics.FromImage(bmp);
            foreach (var n in analyser.Nodes) {
                g.DrawEllipse(new Pen(Brushes.Red), new Rectangle(n.X - 1, n.Y - 1, 2, 2));
            }
            foreach (var e in analyser.Edges) {
                g.DrawLine(new Pen(Brushes.DarkGreen, 1.5f), new Point(e.A.X, e.A.Y), new Point(e.B.X, e.B.Y));
            }
            foreach (var p in analysis.StrategicPositions) {
                var mm = scen.ToMinimapPosition(data.GetLength(0), data.GetLength(1), p.Position);
                g.DrawEllipse(new Pen(Brushes.Purple, 2), new Rectangle((int)(mm.X - 2), (int)(mm.Y - 2), 4, 4));
            }

            g.Save();
            
            if (!Directory.Exists("ai_tests")) {
                Directory.CreateDirectory("ai_tests");
            }

            bmp.Save($"ai_tests\\{scen.RelativeFilename}.png");

            // Return
            return analysis;

        }

    }

    class AIDefencePlannerTest : Command {

        public AIDefencePlannerTest() : base("aipdef", "Runs a test on AI defence planner code") { }

        public override void Execute(CommandArgumentList argumentList) {

            // Load BG
            LoadBGAndProceed();

            // Grab the two default companies
            //var sov = CreateSovietCompany();
            var ger = CreateGermanCompany();

            // Load AI database
            AIDatabase.LoadAIDatabase();

            // Grab scenario
            if (!ScenarioList.TryFindScenario("2p_coh2_resistance", out Scenario? scen) && scen is null) {
                Console.WriteLine("Failed to find scenario '2p_coh2_resistance'");
                return;
            }

            // Get gamode instance
            var mode = ModManager.GetPackageOrError("mod_bg").Gamemodes.FirstOrDefault(x => x.ID == "bg_defence", new());

            // Grab analysis
            var analysis = AIDatabase.GetMapAnalysis("2p_coh2_resistance");

            // Create planner
            var planner = new AIDefencePlanner(analysis, mode);
            planner.Subdivide(scen, 1, new byte[] { 0 });

            // Create plan
            planner.CreateDefencePlan(1, 0, ger, scen);

        }

    }

    class Echo : Command {

        public static readonly Argument<string> TEXT = new Argument<string>("-m", "Message to echo", "");

        public Echo() : base("echo", "Will echo the input", TEXT) {}

        public override void Execute(CommandArgumentList argumentList) {
            string txt = argumentList.GetValue(TEXT);
            Console.WriteLine(txt);
        }

    }

    class SSHCmd : Command {

        public SSHCmd() : base("ssh", "The program will enter into a SSH mode to the remote server") { }

        public override void Execute(CommandArgumentList argumentList) {

            Console.Clear();
            Console.WriteLine("====== Entering SSH Mode ======");
            Console.Write("Username: ");
            string? username = Console.ReadLine();

            // Bail if username is not vlaid
            if (string.IsNullOrEmpty(username)) {
                Console.WriteLine("Username not valid");
                return;
            }

            // Read password
            Console.Write("Password: ");
            string? passwrd = Console.ReadLine();

            // Bail if username is not vlaid
            if (string.IsNullOrEmpty(passwrd)) {
                Console.WriteLine("Password not valid");
                return;
            }

            // Create info
            var info = new ConnectionInfo("194.37.80.249", username, new PasswordAuthenticationMethod(username, passwrd));

            using var client = new SshClient(info);

            try {

                // Try connect
                client.Connect();

                // Run while asked
                bool runClient = true;
                while (runClient) {

                    string? cmd = Console.ReadLine();
                    if (string.IsNullOrEmpty(cmd)) {
                        continue;
                    }

                    switch (cmd) {
                        case "exit":
                            runClient = false;
                            break;
                        case "@script":
                            break;
                        default:

                            // Run command and write out result
                            var result = client.RunCommand(cmd);
                            Console.WriteLine(result.Result);

                            break;
                    }

                }

                // Disconnect
                client.Disconnect();

            } catch (Exception e) {
                Console.WriteLine(e);
            }

        }

    }

    class Repl : Command {

        public Repl() : base("repl", "The program will enter a repl mode and allow for various inputs.") { }

        public override void Execute(CommandArgumentList argumentList) {
            Console.Clear();
            Console.WriteLine("====== Entered REPL Mode ======");
            var defcolour = Console.ForegroundColor;
            while (true) {
                Console.WriteLine();
                Console.Write(">> ");

                Console.ForegroundColor = ConsoleColor.Green;
                string[] input = (Console.ReadLine() ?? string.Empty).Split(' ');
                Console.ForegroundColor = defcolour;

                if (input.Length is 1) {
                    if (input[0] is "exit")
                        break;
                    else if (input[0] is "clear") {
                        Console.Clear();
                        continue;
                    }
                }

                // Parse
                flags?.Parse(input);

            }
            Console.WriteLine("====== Exited REPL Mode ======");
        }

    }

    static ITuningMod? tuningMod;

    static Flags? flags;

    static bool IsDatabaseUp = false;

    static void Main(string[] args) {

        // Write args
        Console.WriteLine(string.Join(' ', args));

        // Create flags parse and executor
        flags = new();
        flags.RegisterCommand<GfxVerify>();
        flags.RegisterCommand<GfxCompile>();
        flags.RegisterCommand<TestCompanies>();
        flags.RegisterCommand<MapExtract>();
        flags.RegisterCommand<CampaignCompile>();
        flags.RegisterCommand<ReplayAnalysis>();
        flags.RegisterCommand<ServerCheck>();
        flags.RegisterCommand<MinimapperCommand>();
        flags.RegisterCommand<CreateInstallFiles>();
        flags.RegisterCommand<AIDefenceAnalyser>();
        flags.RegisterCommand<AIDefencePlannerTest>();
        flags.RegisterCommand<Echo>();
        flags.RegisterCommand<Repl>();
#if DEBUG
        flags.RegisterCommand<SSHCmd>();
        flags.RegisterCommand<Update>();
        flags.RegisterCommand<CreateInstaller>();
        flags.RegisterCommand<ExecutableFirewall>();
#endif

        // Parse (and dispatch)
        flags.Parse(args);

    }

    private static void LoadBGAndProceed() {

        // Bail
        if (IsDatabaseUp)
            return;

        // Load BG
        BattlegroundsInstance.LoadInstance();

        // Wait for all to load
        bool isLoaded = false;

        // Important this is done
        DatabaseManager.LoadAllDatabases((_, _) => isLoaded = true);

        // Wait for database to load
        while (!isLoaded) {
            Thread.Sleep(100);
        }

        // Get package
        var package = ModManager.GetPackage("mod_bg");
        if (package is null) {
            Trace.WriteLine("Failed to find mod_bg package");
            Environment.Exit(-1);
        }

        tuningMod = ModManager.GetMod<ITuningMod>(package.TuningGUID);

        // Mark loaded
        IsDatabaseUp = true;

    }

    private static Company CreateSovietCompany() {

        // Do bail check
        if (tuningMod is null)
            throw new Exception("Tuning mod not defined!");

        // Grab tuning mod GUID
        var g = tuningMod.Guid;

        // Create a dummy company
        CompanyBuilder bld =
            CompanyBuilder.NewCompany("26th Rifle Division", new BasicCompanyType(), CompanyAvailabilityType.MultiplayerOnly, Faction.Soviet, g);

        // Grab blueprints
        var conscripts = BlueprintManager.FromBlueprintName<SquadBlueprint>("conscript_squad_bg");
        var frontoviki = BlueprintManager.FromBlueprintName<SquadBlueprint>("frontoviki_squad_bg");
        var busters = BlueprintManager.FromBlueprintName<SquadBlueprint>("tank_buster_bg");
        var shocks = BlueprintManager.FromBlueprintName<SquadBlueprint>("shock_troops_bg");
        var commissar = BlueprintManager.FromBlueprintName<SquadBlueprint>("commissar_squad_bg");
        var maxim = BlueprintManager.FromBlueprintName<SquadBlueprint>("m1910_maxim_heavy_machine_gun_squad_bg");
        var at = BlueprintManager.FromBlueprintName<SquadBlueprint>("m1942_zis-3_76mm_at_gun_squad_bg");
        var mortar = BlueprintManager.FromBlueprintName<SquadBlueprint>("pm-82_41_mortar_squad_bg");
        var m5 = BlueprintManager.FromBlueprintName<SquadBlueprint>("m5_halftrack_squad_bg");
        var t3476 = BlueprintManager.FromBlueprintName<SquadBlueprint>("t_34_76_squad_bg");
        var t3485 = BlueprintManager.FromBlueprintName<SquadBlueprint>("t_34_85_squad_bg");
        var su85 = BlueprintManager.FromBlueprintName<SquadBlueprint>("su-85_bg");
        var kv = BlueprintManager.FromBlueprintName<SquadBlueprint>("kv-1_bg");

        // Basic infantry
        bld.AddUnit(UnitBuilder.NewUnit(conscripts).SetVeterancyRank(1).AddUpgrade("ppsh-41_sub_machine_gun_upgrade_bg").SetDeploymentPhase(DeploymentPhase.PhaseInitial))
        .AddUnit(UnitBuilder.NewUnit(conscripts).SetVeterancyRank(1).AddUpgrade("ppsh-41_sub_machine_gun_upgrade_bg").SetDeploymentPhase(DeploymentPhase.PhaseInitial))
        .AddUnit(UnitBuilder.NewUnit(conscripts).SetVeterancyRank(2).AddUpgrade("dp-28_lmg_upgrade_bg").SetDeploymentPhase(DeploymentPhase.PhaseStandard))
        .AddUnit(UnitBuilder.NewUnit(conscripts).SetVeterancyRank(2).AddUpgrade("dp-28_lmg_upgrade_bg").SetDeploymentPhase(DeploymentPhase.PhaseStandard))
        .AddUnit(UnitBuilder.NewUnit(conscripts).SetVeterancyRank(4).AddUpgrade("ppsh-41_sub_machine_gun_upgrade_bg").SetDeploymentPhase(DeploymentPhase.PhaseStandard))
        .AddUnit(UnitBuilder.NewUnit(busters).SetVeterancyRank(2).SetDeploymentPhase(DeploymentPhase.PhaseStandard))
        .AddUnit(UnitBuilder.NewUnit(busters).SetVeterancyRank(3).SetDeploymentPhase(DeploymentPhase.PhaseStandard))
        .AddUnit(UnitBuilder.NewUnit(busters).SetVeterancyRank(3).SetDeploymentPhase(DeploymentPhase.PhaseStandard))
        .AddUnit(UnitBuilder.NewUnit(shocks).SetVeterancyRank(4).SetDeploymentPhase(DeploymentPhase.PhaseStandard))
        .AddUnit(UnitBuilder.NewUnit(shocks).SetVeterancyRank(3).SetDeploymentPhase(DeploymentPhase.PhaseStandard))
        .AddUnit(UnitBuilder.NewUnit(shocks).SetVeterancyRank(5).SetDeploymentPhase(DeploymentPhase.PhaseStandard))
        .AddUnit(UnitBuilder.NewUnit(shocks).SetVeterancyRank(2).SetDeploymentPhase(DeploymentPhase.PhaseStandard))
        .AddUnit(UnitBuilder.NewUnit(shocks).SetVeterancyRank(2).SetDeploymentPhase(DeploymentPhase.PhaseStandard))
        .AddUnit(UnitBuilder.NewUnit(commissar).SetVeterancyRank(3).SetDeploymentPhase(DeploymentPhase.PhaseStandard));

        // Transported Infantry
        bld.AddUnit(UnitBuilder.NewUnit(conscripts)
            .SetTransportBlueprint("zis_6_transport_truck_bg")
            .SetDeploymentMethod(DeploymentMethod.DeployAndExit)
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            )
        .AddUnit(UnitBuilder.NewUnit(conscripts)
            .SetTransportBlueprint("zis_6_transport_truck_bg")
            .SetDeploymentMethod(DeploymentMethod.DeployAndExit)
            .SetVeterancyRank(3)
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            )
        .AddUnit(UnitBuilder.NewUnit(conscripts)
            .SetTransportBlueprint("zis_6_transport_truck_bg")
            .SetDeploymentMethod(DeploymentMethod.DeployAndExit)
            .SetVeterancyRank(4)
            .AddUpgrade("ppsh-41_sub_machine_gun_upgrade_bg")
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            )
        .AddUnit(UnitBuilder.NewUnit(conscripts)
            .SetTransportBlueprint("zis_6_transport_truck_bg")
            .SetDeploymentMethod(DeploymentMethod.DeployAndExit)
            .SetVeterancyRank(5)
            .AddUpgrade("ppsh-41_sub_machine_gun_upgrade_bg")
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            )
        .AddUnit(UnitBuilder.NewUnit(frontoviki)
            .SetTransportBlueprint("m5_halftrack_squad_bg")
            .SetDeploymentMethod(DeploymentMethod.DeployAndExit)
            .SetVeterancyRank(0)
            .AddUpgrade("ppsh-41_sub_machine_gun_upgrade_bg")
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            )
        .AddUnit(UnitBuilder.NewUnit(frontoviki)
            .SetTransportBlueprint("m5_halftrack_squad_bg")
            .SetDeploymentMethod(DeploymentMethod.DeployAndExit)
            .SetVeterancyRank(1)
            .AddUpgrade("ppsh-41_sub_machine_gun_upgrade_bg")
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            );

        // Support Weapons
        bld.AddUnit(UnitBuilder.NewUnit(at)
            .SetTransportBlueprint("zis_6_transport_truck_bg")
            .SetDeploymentMethod(DeploymentMethod.DeployAndExit)
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            )
        .AddUnit(UnitBuilder.NewUnit(at)
            .SetTransportBlueprint("zis_6_transport_truck_bg")
            .SetDeploymentMethod(DeploymentMethod.DeployAndExit)
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            )
        .AddUnit(UnitBuilder.NewUnit(maxim)
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            )
        .AddUnit(UnitBuilder.NewUnit(maxim)
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            )
        .AddUnit(UnitBuilder.NewUnit(mortar)
            .SetVeterancyRank(2)
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            )
        .AddUnit(UnitBuilder.NewUnit(mortar)
            .SetVeterancyRank(2)
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            );

        // Vehicles
        bld.AddUnit(UnitBuilder.NewUnit(m5)
            .SetVeterancyRank(1)
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            )
        .AddUnit(UnitBuilder.NewUnit(t3476)
            .SetVeterancyRank(0)
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            )
        .AddUnit(UnitBuilder.NewUnit(t3476)
            .SetVeterancyRank(0)
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            )
        .AddUnit(UnitBuilder.NewUnit(t3476)
            .SetVeterancyRank(2)
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            )
        .AddUnit(UnitBuilder.NewUnit(su85)
            .SetVeterancyRank(1)
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            )
        .AddUnit(UnitBuilder.NewUnit(t3485)
            .SetVeterancyRank(4)
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            )
        .AddUnit(UnitBuilder.NewUnit(t3485)
            .SetVeterancyRank(5)
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            )
        .AddUnit(UnitBuilder.NewUnit(kv)
            .SetVeterancyRank(2)
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            )
        .AddUnit(UnitBuilder.NewUnit(kv)
            .SetVeterancyRank(2)
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            );

        // Commit changes
        return bld.Commit().Result;
    }

    private static Company CreateGermanCompany() {

        // Do bail check
        if (tuningMod is null)
            throw new Exception("Tuning mod not defined!");

        // Grab tuning mod GUID
        var g = tuningMod.Guid;

        // Create a dummy company
        CompanyBuilder bld = CompanyBuilder.NewCompany("69th Panzer Kompanie", new BasicCompanyType(), CompanyAvailabilityType.MultiplayerOnly, Faction.Wehrmacht, g);

        // Grab blueprints
        var pioneers = BlueprintManager.FromBlueprintName<SquadBlueprint>("pioneer_squad_bg");
        var sniper = BlueprintManager.FromBlueprintName<SquadBlueprint>("sniper_squad_bg");
        var gren = BlueprintManager.FromBlueprintName<SquadBlueprint>("grenadier_squad_bg");
        var pgren = BlueprintManager.FromBlueprintName<SquadBlueprint>("panzer_grenadier_squad_bg");
        var pak = BlueprintManager.FromBlueprintName<SquadBlueprint>("pak40_75mm_at_gun_squad_bg");
        var mg = BlueprintManager.FromBlueprintName<SquadBlueprint>("mg42_heavy_machine_gun_squad_bg");
        var mortar = BlueprintManager.FromBlueprintName<SquadBlueprint>("mortar_team_81mm_bg");
        var puma = BlueprintManager.FromBlueprintName<SquadBlueprint>("sdkfz_234_puma_ost_bg");
        var panther = BlueprintManager.FromBlueprintName<SquadBlueprint>("panther_squad_bg");
        var pziv = BlueprintManager.FromBlueprintName<SquadBlueprint>("panzer_iv_squad_bg");
        var ostwind = BlueprintManager.FromBlueprintName<SquadBlueprint>("ostwind_squad_bg");
        var tiger = BlueprintManager.FromBlueprintName<SquadBlueprint>("tiger_squad_bg");
        var brumm = BlueprintManager.FromBlueprintName<SquadBlueprint>("brummbar_squad_bg");

        // Basic infantry
        bld.AddUnit(UnitBuilder.NewUnit(pioneers).SetDeploymentPhase(DeploymentPhase.PhaseInitial))
        .AddUnit(UnitBuilder.NewUnit(pioneers).SetDeploymentPhase(DeploymentPhase.PhaseStandard))
        .AddUnit(UnitBuilder.NewUnit(pioneers).SetDeploymentPhase(DeploymentPhase.PhaseStandard))
        .AddUnit(UnitBuilder.NewUnit(pioneers).SetDeploymentPhase(DeploymentPhase.PhaseStandard))
        .AddUnit(UnitBuilder.NewUnit(sniper).SetDeploymentPhase(DeploymentPhase.PhaseInitial))
        .AddUnit(UnitBuilder.NewUnit(sniper).SetDeploymentPhase(DeploymentPhase.PhaseStandard));

        // Transported Infantry
        bld.AddUnit(UnitBuilder.NewUnit(gren)
            .SetTransportBlueprint("sdkfz_251_halftrack_squad_bg")
            .SetDeploymentMethod(DeploymentMethod.DeployAndStay)
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            )
        .AddUnit(UnitBuilder.NewUnit(gren)
            .SetTransportBlueprint("sdkfz_251_halftrack_squad_bg")
            .SetDeploymentMethod(DeploymentMethod.DeployAndStay)
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            )
        .AddUnit(UnitBuilder.NewUnit(gren)
            .SetTransportBlueprint("sdkfz_251_halftrack_squad_bg")
            .SetDeploymentMethod(DeploymentMethod.DeployAndStay)
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            )
        .AddUnit(UnitBuilder.NewUnit(gren)
            .SetTransportBlueprint("sdkfz_251_halftrack_squad_bg")
            .SetDeploymentMethod(DeploymentMethod.DeployAndStay)
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            )
        .AddUnit(UnitBuilder.NewUnit(gren)
            .SetTransportBlueprint("opel_blitz_transport_squad_bg")
            .SetDeploymentMethod(DeploymentMethod.DeployAndExit)
            .SetVeterancyRank(2)
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            )
        .AddUnit(UnitBuilder.NewUnit(gren)
            .SetTransportBlueprint("opel_blitz_transport_squad_bg")
            .SetDeploymentMethod(DeploymentMethod.DeployAndExit)
            .SetVeterancyRank(2)
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            )
        .AddUnit(UnitBuilder.NewUnit(gren)
            .SetTransportBlueprint("opel_blitz_transport_squad_bg")
            .SetDeploymentMethod(DeploymentMethod.DeployAndExit)
            .SetVeterancyRank(2)
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            )
        .AddUnit(UnitBuilder.NewUnit(gren)
            .SetTransportBlueprint("opel_blitz_transport_squad_bg")
            .SetDeploymentMethod(DeploymentMethod.DeployAndExit)
            .SetVeterancyRank(2)
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            )
        .AddUnit(UnitBuilder.NewUnit(pgren)
            .SetTransportBlueprint("opel_blitz_transport_squad_bg")
            .SetDeploymentMethod(DeploymentMethod.DeployAndExit)
            .SetVeterancyRank(3)
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            )
        .AddUnit(UnitBuilder.NewUnit(pgren)
            .SetTransportBlueprint("opel_blitz_transport_squad_bg")
            .SetDeploymentMethod(DeploymentMethod.DeployAndExit)
            .SetVeterancyRank(3)
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            )
        .AddUnit(UnitBuilder.NewUnit(pgren)
            .SetTransportBlueprint("opel_blitz_transport_squad_bg")
            .SetDeploymentMethod(DeploymentMethod.DeployAndExit)
            .SetVeterancyRank(3)
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            );

        // Support Weapons
        bld.AddUnit(UnitBuilder.NewUnit(pak)
            .SetTransportBlueprint("opel_blitz_transport_squad_bg")
            .SetDeploymentMethod(DeploymentMethod.DeployAndExit)
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            )
        .AddUnit(UnitBuilder.NewUnit(pak)
            .SetTransportBlueprint("opel_blitz_transport_squad_bg")
            .SetDeploymentMethod(DeploymentMethod.DeployAndExit)
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            )
        .AddUnit(UnitBuilder.NewUnit(pak)
            .SetTransportBlueprint("opel_blitz_transport_squad_bg")
            .SetDeploymentMethod(DeploymentMethod.DeployAndExit)
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            )
        .AddUnit(UnitBuilder.NewUnit(mg)
            .SetDeploymentPhase(DeploymentPhase.PhaseInitial)
            )
        .AddUnit(UnitBuilder.NewUnit(mg)
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            )
        .AddUnit(UnitBuilder.NewUnit(mg)
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            )
        .AddUnit(UnitBuilder.NewUnit(mg)
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            )
        .AddUnit(UnitBuilder.NewUnit(mortar).SetDeploymentPhase(DeploymentPhase.PhaseStandard))
        .AddUnit(UnitBuilder.NewUnit(mortar).SetDeploymentPhase(DeploymentPhase.PhaseStandard));

        // Vehicles
        bld.AddUnit(UnitBuilder.NewUnit(puma)
            .SetVeterancyRank(1)
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            )
        .AddUnit(UnitBuilder.NewUnit(puma)
            .SetVeterancyRank(1)
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            )
        .AddUnit(UnitBuilder.NewUnit(panther)
            .SetVeterancyRank(1)
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            )
        .AddUnit(UnitBuilder.NewUnit(pziv)
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            )
        .AddUnit(UnitBuilder.NewUnit(pziv)
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            )
        .AddUnit(UnitBuilder.NewUnit(pziv)
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            )
        .AddUnit(UnitBuilder.NewUnit(pziv)
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            )
        .AddUnit(UnitBuilder.NewUnit(ostwind)
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            )
        .AddUnit(UnitBuilder.NewUnit(ostwind)
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            )
        .AddUnit(UnitBuilder.NewUnit(tiger)
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            )
        .AddUnit(UnitBuilder.NewUnit(tiger)
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            )
        .AddUnit(UnitBuilder.NewUnit(tiger)
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            )
        .AddUnit(UnitBuilder.NewUnit(brumm)
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            .SetVeterancyRank(3)
            );

        // Commit changes
        return bld.Commit().Result;
    }

}
