using Battlegrounds.AI.Lobby;
using Battlegrounds.AI;
using Battlegrounds.Game.Scenarios;
using Battlegrounds.Game.Database.Management.CoH2;

using System;
using System.Linq;

namespace Battlegrounds.Developer.Commands;

public class AIDefencePlannerTestCommand : Command {

    public AIDefencePlannerTestCommand() : base("aipdef", "Runs a test on AI defence planner code") { }

    public override void Execute(CommandArgumentList argumentList) {

        // Load BG
        Program.LoadBGAndProceed();

        // Grab the two default companies
        //var sov = CreateSovietCompany();
        var ger = Generator.CompanyGenerator.CreateGermanCompany(Program.tuningMod);

        // Load AI database
        AIDatabase.LoadAIDatabase();

        // Grab scenario list
        var scenarioList = new CoH2ScenarioList();

        // Grab scenario
        if (!scenarioList.TryFindScenario("2p_coh2_resistance", out Scenario? scen) && scen is null) {
            Console.WriteLine("Failed to find scenario '2p_coh2_resistance'");
            return;
        }

        // Get gamode instance
        var mode = BattlegroundsContext.ModManager.GetPackageOrError("mod_bg").Gamemodes.FirstOrDefault(x => x.ID == "bg_defence", new());

        // Grab analysis
        var analysis = AIDatabase.GetMapAnalysis("2p_coh2_resistance");

        // Create planner
        var planner = new AIDefencePlanner(analysis, mode);
        planner.Subdivide(scen, 1, new byte[] { 0 });

        // Create plan
        planner.CreateDefencePlan(1, 0, ger, scen);

    }

}
