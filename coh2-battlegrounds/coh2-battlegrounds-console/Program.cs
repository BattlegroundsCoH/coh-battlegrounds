using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading;

using Battlegrounds;
using Battlegrounds.Game.Database;
using Battlegrounds.Game.Database.Management;
using Battlegrounds.Game.DataCompany;
using Battlegrounds.Game.DataSource.Replay;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Game.Match;
using Battlegrounds.Game.Match.Data;
using Battlegrounds.Gfx;
using Battlegrounds.Modding;
using Battlegrounds.Networking;
using Battlegrounds.Verification;

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
            => GfxFolderCompiler.Compile(argumentList.GetValue(DIR), argumentList.GetValue(OUT));

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

        public MapExtract() : base("coh2-extract-maps", "Verifies the integrity of a gfx file.", PATH) { }

        public override void Execute(CommandArgumentList argumentList) {

            MapExtractor.Output = argumentList.GetValue(PATH);
            MapExtractor.Extract();

        }

    }

    class CampaignCompile : Command {

        public static readonly Argument<string> PATH = new Argument<string>("-c", "Specifies the directory to compile", string.Empty);
        public static readonly Argument<string> OUT = new Argument<string>("-o", "Specifies compiled campaign output file.", "campaign.camp");

        public CampaignCompile() : base("campaign", "Verifies the integrity of a gfx file.", PATH, OUT) { }

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
            var response = NetworkInterface.PingServer("194.37.80.249", 80);
            watch.Stop();
            Console.WriteLine($"Server response: {response} - ping: {watch.ElapsedMilliseconds}ms");
        }

    }

    class Update : Command {
        
        public static readonly Argument<string> TARGET = new Argument<string>("-target", "Specifies the update target [db, checksum]", "db");

        public static readonly Argument<string> MOD = new Argument<string>("-m", "Specifies the mod to update, applicable when target=db", "vcoh");

        public static readonly Argument<string> TOOL = 
            new Argument<string>("-t", "Specifies the full path to the tools folder", @"C:\Program Files (x86)\Steam\steamapps\common\Company of Heroes 2 Tools");

        public Update() : base("update", "Will update specified elements of the source build.", TARGET, MOD, TOOL) { }

        private ConsoleColor m_col;

        public override void Execute(CommandArgumentList argumentList) {
            this.m_col = Console.ForegroundColor;
            switch (argumentList.GetValue(TARGET)) {
                case "db":
                    this.UpdateDatabase(argumentList);
                    break;
                case "checksum":
                    this.ComputeChecksum();
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
            string xmlreader = Path.GetFullPath("..\\..\\..\\..\\..\\db-tool\\bin\\debug\\net5.0\\CoH2XML2JSON.exe");

            // Verify
            if (!File.Exists(xmlreader)) {
                this.Err("Fatal Error: Failed to locate xml2json executable");
                return;
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

        private void ComputeChecksum() {

            // Get full checksum path
            string checksumPath = Path.GetFullPath("..\\..\\..\\..\\coh2-battlegrounds\\checksum.txt");

            // Get path to release build
            string releaseExe = Path.GetFullPath("..\\..\\..\\..\\coh2-battlegrounds\\bin\\Release\\net6.0-windows\\coh2-battlegrounds.exe");

            // Log
            Console.WriteLine($"Checksum file: {checksumPath}");
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
            File.WriteAllText(checksumPath, Integrity.IntegrityHashString);
            File.Copy(checksumPath, releaseExe.Replace("coh2-battlegrounds.exe", "checksum.txt"), true);

            // Log
            Console.WriteLine("Saved hash to checksum file(s).");

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
        flags.RegisterCommand<Repl>();
#if DEBUG
        flags.RegisterCommand<Update>();
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
            CompanyBuilder.NewCompany("26th Rifle Division", CompanyType.Infantry, CompanyAvailabilityType.MultiplayerOnly, Faction.Soviet, g);

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
        .AddUnit(UnitBuilder.NewUnit(conscripts).SetVeterancyRank(2).AddUpgrade("dp-28_lmg_upgrade_bg").SetDeploymentPhase(DeploymentPhase.PhaseA))
        .AddUnit(UnitBuilder.NewUnit(conscripts).SetVeterancyRank(2).AddUpgrade("dp-28_lmg_upgrade_bg").SetDeploymentPhase(DeploymentPhase.PhaseA))
        .AddUnit(UnitBuilder.NewUnit(conscripts).SetVeterancyRank(4).AddUpgrade("ppsh-41_sub_machine_gun_upgrade_bg").SetDeploymentPhase(DeploymentPhase.PhaseA))
        .AddUnit(UnitBuilder.NewUnit(busters).SetVeterancyRank(2).SetDeploymentPhase(DeploymentPhase.PhaseA))
        .AddUnit(UnitBuilder.NewUnit(busters).SetVeterancyRank(3).SetDeploymentPhase(DeploymentPhase.PhaseA))
        .AddUnit(UnitBuilder.NewUnit(busters).SetVeterancyRank(3).SetDeploymentPhase(DeploymentPhase.PhaseA))
        .AddUnit(UnitBuilder.NewUnit(shocks).SetVeterancyRank(4).SetDeploymentPhase(DeploymentPhase.PhaseA))
        .AddUnit(UnitBuilder.NewUnit(shocks).SetVeterancyRank(3).SetDeploymentPhase(DeploymentPhase.PhaseA))
        .AddUnit(UnitBuilder.NewUnit(shocks).SetVeterancyRank(5).SetDeploymentPhase(DeploymentPhase.PhaseA))
        .AddUnit(UnitBuilder.NewUnit(shocks).SetVeterancyRank(2).SetDeploymentPhase(DeploymentPhase.PhaseB))
        .AddUnit(UnitBuilder.NewUnit(shocks).SetVeterancyRank(2).SetDeploymentPhase(DeploymentPhase.PhaseB))
        .AddUnit(UnitBuilder.NewUnit(commissar).SetVeterancyRank(3).SetDeploymentPhase(DeploymentPhase.PhaseA));

        // Transported Infantry
        bld.AddUnit(UnitBuilder.NewUnit(conscripts)
            .SetTransportBlueprint("zis_6_transport_truck_bg")
            .SetDeploymentMethod(DeploymentMethod.DeployAndExit)
            .SetDeploymentPhase(DeploymentPhase.PhaseA)
            )
        .AddUnit(UnitBuilder.NewUnit(conscripts)
            .SetTransportBlueprint("zis_6_transport_truck_bg")
            .SetDeploymentMethod(DeploymentMethod.DeployAndExit)
            .SetVeterancyRank(3)
            .SetDeploymentPhase(DeploymentPhase.PhaseA)
            )
        .AddUnit(UnitBuilder.NewUnit(conscripts)
            .SetTransportBlueprint("zis_6_transport_truck_bg")
            .SetDeploymentMethod(DeploymentMethod.DeployAndExit)
            .SetVeterancyRank(4)
            .AddUpgrade("ppsh-41_sub_machine_gun_upgrade_bg")
            .SetDeploymentPhase(DeploymentPhase.PhaseA)
            )
        .AddUnit(UnitBuilder.NewUnit(conscripts)
            .SetTransportBlueprint("zis_6_transport_truck_bg")
            .SetDeploymentMethod(DeploymentMethod.DeployAndExit)
            .SetVeterancyRank(5)
            .AddUpgrade("ppsh-41_sub_machine_gun_upgrade_bg")
            .SetDeploymentPhase(DeploymentPhase.PhaseA)
            )
        .AddUnit(UnitBuilder.NewUnit(frontoviki)
            .SetTransportBlueprint("m5_halftrack_squad_bg")
            .SetDeploymentMethod(DeploymentMethod.DeployAndExit)
            .SetVeterancyRank(0)
            .AddUpgrade("ppsh-41_sub_machine_gun_upgrade_bg")
            .SetDeploymentPhase(DeploymentPhase.PhaseB)
            )
        .AddUnit(UnitBuilder.NewUnit(frontoviki)
            .SetTransportBlueprint("m5_halftrack_squad_bg")
            .SetDeploymentMethod(DeploymentMethod.DeployAndExit)
            .SetVeterancyRank(1)
            .AddUpgrade("ppsh-41_sub_machine_gun_upgrade_bg")
            .SetDeploymentPhase(DeploymentPhase.PhaseB)
            );

        // Support Weapons
        bld.AddUnit(UnitBuilder.NewUnit(at)
            .SetTransportBlueprint("zis_6_transport_truck_bg")
            .SetDeploymentMethod(DeploymentMethod.DeployAndExit)
            .SetDeploymentPhase(DeploymentPhase.PhaseA)
            )
        .AddUnit(UnitBuilder.NewUnit(at)
            .SetTransportBlueprint("zis_6_transport_truck_bg")
            .SetDeploymentMethod(DeploymentMethod.DeployAndExit)
            .SetDeploymentPhase(DeploymentPhase.PhaseA)
            )
        .AddUnit(UnitBuilder.NewUnit(maxim)
            .SetDeploymentPhase(DeploymentPhase.PhaseA)
            )
        .AddUnit(UnitBuilder.NewUnit(maxim)
            .SetDeploymentPhase(DeploymentPhase.PhaseA)
            )
        .AddUnit(UnitBuilder.NewUnit(mortar)
            .SetVeterancyRank(2)
            .SetDeploymentPhase(DeploymentPhase.PhaseA)
            )
        .AddUnit(UnitBuilder.NewUnit(mortar)
            .SetVeterancyRank(2)
            .SetDeploymentPhase(DeploymentPhase.PhaseA)
            );

        // Vehicles
        bld.AddUnit(UnitBuilder.NewUnit(m5)
            .SetVeterancyRank(1)
            .SetDeploymentPhase(DeploymentPhase.PhaseA)
            )
        .AddUnit(UnitBuilder.NewUnit(t3476)
            .SetVeterancyRank(0)
            .SetDeploymentPhase(DeploymentPhase.PhaseA)
            )
        .AddUnit(UnitBuilder.NewUnit(t3476)
            .SetVeterancyRank(0)
            .SetDeploymentPhase(DeploymentPhase.PhaseA)
            )
        .AddUnit(UnitBuilder.NewUnit(t3476)
            .SetVeterancyRank(2)
            .SetDeploymentPhase(DeploymentPhase.PhaseB)
            )
        .AddUnit(UnitBuilder.NewUnit(su85)
            .SetVeterancyRank(1)
            .SetDeploymentPhase(DeploymentPhase.PhaseB)
            )
        .AddUnit(UnitBuilder.NewUnit(t3485)
            .SetVeterancyRank(4)
            .SetDeploymentPhase(DeploymentPhase.PhaseB)
            )
        .AddUnit(UnitBuilder.NewUnit(t3485)
            .SetVeterancyRank(5)
            .SetDeploymentPhase(DeploymentPhase.PhaseC)
            )
        .AddUnit(UnitBuilder.NewUnit(kv)
            .SetVeterancyRank(2)
            .SetDeploymentPhase(DeploymentPhase.PhaseB)
            )
        .AddUnit(UnitBuilder.NewUnit(kv)
            .SetVeterancyRank(2)
            .SetDeploymentPhase(DeploymentPhase.PhaseC)
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
        CompanyBuilder bld = CompanyBuilder.NewCompany("", CompanyType.Mechanized, CompanyAvailabilityType.MultiplayerOnly, Faction.Wehrmacht, g);

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
        .AddUnit(UnitBuilder.NewUnit(pioneers).SetDeploymentPhase(DeploymentPhase.PhaseA))
        .AddUnit(UnitBuilder.NewUnit(pioneers).SetDeploymentPhase(DeploymentPhase.PhaseA))
        .AddUnit(UnitBuilder.NewUnit(pioneers).SetDeploymentPhase(DeploymentPhase.PhaseA))
        .AddUnit(UnitBuilder.NewUnit(sniper).SetDeploymentPhase(DeploymentPhase.PhaseInitial))
        .AddUnit(UnitBuilder.NewUnit(sniper).SetDeploymentPhase(DeploymentPhase.PhaseA));

        // Transported Infantry
        bld.AddUnit(UnitBuilder.NewUnit(gren)
            .SetTransportBlueprint("sdkfz_251_halftrack_squad_bg")
            .SetDeploymentMethod(DeploymentMethod.DeployAndStay)
            .SetDeploymentPhase(DeploymentPhase.PhaseA)
            )
        .AddUnit(UnitBuilder.NewUnit(gren)
            .SetTransportBlueprint("sdkfz_251_halftrack_squad_bg")
            .SetDeploymentMethod(DeploymentMethod.DeployAndStay)
            .SetDeploymentPhase(DeploymentPhase.PhaseA)
            )
        .AddUnit(UnitBuilder.NewUnit(gren)
            .SetTransportBlueprint("sdkfz_251_halftrack_squad_bg")
            .SetDeploymentMethod(DeploymentMethod.DeployAndStay)
            .SetDeploymentPhase(DeploymentPhase.PhaseA)
            )
        .AddUnit(UnitBuilder.NewUnit(gren)
            .SetTransportBlueprint("sdkfz_251_halftrack_squad_bg")
            .SetDeploymentMethod(DeploymentMethod.DeployAndStay)
            .SetDeploymentPhase(DeploymentPhase.PhaseA)
            )
        .AddUnit(UnitBuilder.NewUnit(gren)
            .SetTransportBlueprint("opel_blitz_transport_squad_bg")
            .SetDeploymentMethod(DeploymentMethod.DeployAndExit)
            .SetVeterancyRank(2)
            .SetDeploymentPhase(DeploymentPhase.PhaseA)
            )
        .AddUnit(UnitBuilder.NewUnit(gren)
            .SetTransportBlueprint("opel_blitz_transport_squad_bg")
            .SetDeploymentMethod(DeploymentMethod.DeployAndExit)
            .SetVeterancyRank(2)
            .SetDeploymentPhase(DeploymentPhase.PhaseA)
            )
        .AddUnit(UnitBuilder.NewUnit(gren)
            .SetTransportBlueprint("opel_blitz_transport_squad_bg")
            .SetDeploymentMethod(DeploymentMethod.DeployAndExit)
            .SetVeterancyRank(2)
            .SetDeploymentPhase(DeploymentPhase.PhaseA)
            )
        .AddUnit(UnitBuilder.NewUnit(gren)
            .SetTransportBlueprint("opel_blitz_transport_squad_bg")
            .SetDeploymentMethod(DeploymentMethod.DeployAndExit)
            .SetVeterancyRank(2)
            .SetDeploymentPhase(DeploymentPhase.PhaseA)
            )
        .AddUnit(UnitBuilder.NewUnit(pgren)
            .SetTransportBlueprint("opel_blitz_transport_squad_bg")
            .SetDeploymentMethod(DeploymentMethod.DeployAndExit)
            .SetVeterancyRank(3)
            .SetDeploymentPhase(DeploymentPhase.PhaseA)
            )
        .AddUnit(UnitBuilder.NewUnit(pgren)
            .SetTransportBlueprint("opel_blitz_transport_squad_bg")
            .SetDeploymentMethod(DeploymentMethod.DeployAndExit)
            .SetVeterancyRank(3)
            .SetDeploymentPhase(DeploymentPhase.PhaseA)
            )
        .AddUnit(UnitBuilder.NewUnit(pgren)
            .SetTransportBlueprint("opel_blitz_transport_squad_bg")
            .SetDeploymentMethod(DeploymentMethod.DeployAndExit)
            .SetVeterancyRank(3)
            .SetDeploymentPhase(DeploymentPhase.PhaseA)
            );

        // Support Weapons
        bld.AddUnit(UnitBuilder.NewUnit(pak)
            .SetTransportBlueprint("opel_blitz_transport_squad_bg")
            .SetDeploymentMethod(DeploymentMethod.DeployAndExit)
            .SetDeploymentPhase(DeploymentPhase.PhaseA)
            )
        .AddUnit(UnitBuilder.NewUnit(pak)
            .SetTransportBlueprint("opel_blitz_transport_squad_bg")
            .SetDeploymentMethod(DeploymentMethod.DeployAndExit)
            .SetDeploymentPhase(DeploymentPhase.PhaseA)
            )
        .AddUnit(UnitBuilder.NewUnit(pak)
            .SetTransportBlueprint("opel_blitz_transport_squad_bg")
            .SetDeploymentMethod(DeploymentMethod.DeployAndExit)
            .SetDeploymentPhase(DeploymentPhase.PhaseA)
            )
        .AddUnit(UnitBuilder.NewUnit(mg)
            .SetDeploymentPhase(DeploymentPhase.PhaseInitial)
            )
        .AddUnit(UnitBuilder.NewUnit(mg)
            .SetDeploymentPhase(DeploymentPhase.PhaseA)
            )
        .AddUnit(UnitBuilder.NewUnit(mg)
            .SetDeploymentPhase(DeploymentPhase.PhaseA)
            )
        .AddUnit(UnitBuilder.NewUnit(mg)
            .SetDeploymentPhase(DeploymentPhase.PhaseA)
            )
        .AddUnit(UnitBuilder.NewUnit(mortar).SetDeploymentPhase(DeploymentPhase.PhaseA))
        .AddUnit(UnitBuilder.NewUnit(mortar).SetDeploymentPhase(DeploymentPhase.PhaseA));

        // Vehicles
        bld.AddUnit(UnitBuilder.NewUnit(puma)
            .SetVeterancyRank(1)
            .SetDeploymentPhase(DeploymentPhase.PhaseA)
            )
        .AddUnit(UnitBuilder.NewUnit(puma)
            .SetVeterancyRank(1)
            .SetDeploymentPhase(DeploymentPhase.PhaseA)
            )
        .AddUnit(UnitBuilder.NewUnit(panther)
            .SetVeterancyRank(1)
            .SetDeploymentPhase(DeploymentPhase.PhaseA)
            )
        .AddUnit(UnitBuilder.NewUnit(pziv)
            .SetDeploymentPhase(DeploymentPhase.PhaseA)
            )
        .AddUnit(UnitBuilder.NewUnit(pziv)
            .SetDeploymentPhase(DeploymentPhase.PhaseA)
            )
        .AddUnit(UnitBuilder.NewUnit(pziv)
            .SetDeploymentPhase(DeploymentPhase.PhaseA)
            )
        .AddUnit(UnitBuilder.NewUnit(pziv)
            .SetDeploymentPhase(DeploymentPhase.PhaseA)
            )
        .AddUnit(UnitBuilder.NewUnit(ostwind)
            .SetDeploymentPhase(DeploymentPhase.PhaseA)
            )
        .AddUnit(UnitBuilder.NewUnit(ostwind)
            .SetDeploymentPhase(DeploymentPhase.PhaseA)
            )
        .AddUnit(UnitBuilder.NewUnit(tiger)
            .SetDeploymentPhase(DeploymentPhase.PhaseA)
            )
        .AddUnit(UnitBuilder.NewUnit(tiger)
            .SetDeploymentPhase(DeploymentPhase.PhaseA)
            )
        .AddUnit(UnitBuilder.NewUnit(tiger)
            .SetDeploymentPhase(DeploymentPhase.PhaseA)
            )
        .AddUnit(UnitBuilder.NewUnit(brumm)
            .SetDeploymentPhase(DeploymentPhase.PhaseA)
            .SetVeterancyRank(3)
            );

        // Commit changes
        return bld.Commit().Result;
    }

}

