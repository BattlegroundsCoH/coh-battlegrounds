using System;
using System.Diagnostics;
using System.Threading;

using Battlegrounds.Developer.Commands;
using Battlegrounds.Game.Database.Management;
using Battlegrounds.Modding;

namespace Battlegrounds.Developer;

class Program {
    
    public static ITuningMod? tuningMod;

    public static Flags? flags;

    public static bool IsDatabaseUp = false;

    static void Main(string[] args) {

        // Write args
        Console.WriteLine(string.Join(' ', args));

        // Create flags parse and executor
        flags = new();
        flags.RegisterCommand<GfxVerifyCommand>();
        flags.RegisterCommand<GfxCompileCommand>();
        flags.RegisterCommand<TestCompaniesCommand>();
        flags.RegisterCommand<MapExtractCommand>();
        flags.RegisterCommand<CampaignCompileCommand>();
        flags.RegisterCommand<ReplayAnalysisCommand>();
        flags.RegisterCommand<ServerCheckCommand>();
        flags.RegisterCommand<MinimapperCommand>();
        flags.RegisterCommand<CreateInstallerCommand>();
        flags.RegisterCommand<AIDefenceAnalyserCommand>();
        flags.RegisterCommand<AIDefencePlannerTestCommand>();
        flags.RegisterCommand<ReplCommand>();
#if DEBUG
        flags.RegisterCommand<UpdateCommand>();
        flags.RegisterCommand<CreateInstallerCommand>();
        flags.RegisterCommand<ExecutableFirewallCommand>();
#endif

        // Parse (and dispatch)
        flags.Parse(args);

    }

    public static void LoadBGAndProceed() {

        // Bail
        if (IsDatabaseUp)
            return;

        // Load BG
        BattlegroundsContext.LoadInstance();

        // Wait for all to load
        bool isLoaded = false;

        // Important this is done
        //DatabaseManager.LoadAllDatabases((_, _) => isLoaded = true);

        // Wait for database to load
        while (!isLoaded) {
            Thread.Sleep(100);
        }

        // Get package
        var package = BattlegroundsContext.ModManager.GetPackage("mod_bg");
        if (package is null) {
            Trace.WriteLine("Failed to find mod_bg package");
            Environment.Exit(-1);
        }

        tuningMod = BattlegroundsContext.ModManager.GetMod<ITuningMod>(package.TuningGUID);

        // Mark loaded
        IsDatabaseUp = true;

    }

}
