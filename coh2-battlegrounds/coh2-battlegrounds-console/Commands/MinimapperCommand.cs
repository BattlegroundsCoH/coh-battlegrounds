using Battlegrounds.Game.Database.Management.CoH2;
using Battlegrounds.Game.Scenarios;
using Battlegrounds.Game.Scenarios.CoH2;
using System;

namespace Battlegrounds.Developer.Commands;

public class MinimapperCommand : Command {

    public static readonly Argument<string> SCENARIO = new Argument<string>("-s", "Specifies the scenario to map", "2p_coh2_resistance");

    public MinimapperCommand() : base("mini", "Basic minimap position translator functionality testing", SCENARIO) { }

    public override void Execute(CommandArgumentList argumentList) {

        // Load BG
        Program.LoadBGAndProceed();

        // Get scenario
        string s = argumentList.GetValue(SCENARIO);
        if (!(new CoH2ScenarioList()).TryFindScenario(s, out IScenario? scen) && scen is null) {
            Console.WriteLine("Failed to find scenario " + s);
            return;
        }

        // Invoke mapper
        if (scen is CoH2Scenario sc)
            Minimapper.Map(sc);

    }

}
