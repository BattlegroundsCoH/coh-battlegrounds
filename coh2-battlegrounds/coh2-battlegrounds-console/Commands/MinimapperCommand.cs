using Battlegrounds.Game.Scenarios;

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
        if (!ScenarioList.TryFindScenario(s, out Scenario? scen) && scen is null) {
            Console.WriteLine("Failed to find scenario " + s);
            return;
        }

        // Invoke mapper
        Minimapper.Map(scen);

    }

}
