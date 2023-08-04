using Battlegrounds.AI.Lobby;
using Battlegrounds.Game.Database.Management.CoH2;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Drawing.Imaging;
using System.Drawing;
using System.Text.Json;
using System.Threading;
using Battlegrounds.Game.Scenarios;

namespace Battlegrounds.Developer.Commands;

public class AIDefenceAnalyserCommand : Command {

    public static readonly Argument<string> SCENARIO = new Argument<string>("-s", "Specifies the scenario to map", "2p_coh2_resistance");

    public static readonly Argument<bool> DOALL = new Argument<bool>("-a", "Specifies the tool should be applied to all known maps", false);

    public AIDefenceAnalyserCommand() : base("aidef", "Invokes the AI defence minimap analysis tool.", SCENARIO, DOALL) { }

    public override void Execute(CommandArgumentList argumentList) {

        // Load BG
        Program.LoadBGAndProceed();

        // Get scenarios
        CoH2ScenarioList scenarioList = new CoH2ScenarioList();

        // Do all if flag is set; otherwise just do example scenario
        if (argumentList.GetValue(DOALL)) {

            // Grab all scenarios
            var scenarios = scenarioList.GetList();

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

            File.WriteAllText($"vcoh2-aimap-db.json", JsonSerializer.Serialize(ls, new JsonSerializerOptions() { WriteIndented = true }));

        } else {

            // Get scenario
            string s = argumentList.GetValue(SCENARIO);
            if (!scenarioList.TryFindScenario(s, out IScenario? scen) && scen is null) {
                Console.WriteLine("Failed to find scenario " + s);
                return;
            }

            // Do the scenario
            DoScenario(scen);

        }

    }

    private static AIMapAnalysis? DoScenario(IScenario scen) {

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
