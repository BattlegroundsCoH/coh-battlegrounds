using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

using Battlegrounds;
using Battlegrounds.Compiler;
using Battlegrounds.Game.Scenarios;

namespace coh2_battlegrounds_console;

public static class MapExtractor {

    public static string? Output { get; set; }

    public static void Extract() {

        if (string.IsNullOrWhiteSpace(Output)) {
            return;
        }

        string path = Pathfinder.GetOrFindCoHPath();
        string[] map_archives = {
            $"{path}CoH2\\Archives\\MPScenarios.sga",
            $"{path}CoH2\\Archives\\MPXP1Scenarios.sga",
        };

        string extract = Path.GetFullPath(Output);
        if (Directory.Exists(extract)) {
            Directory.Delete(extract, true);
        }

        _ = Directory.CreateDirectory(extract);

        List<Scenario> scenarios = new();

        foreach (string archive in map_archives) {

            Console.WriteLine($"Extracting .sga '{archive}'");

            if (!Archiver.Extract(archive, extract, Console.Out)) {
                Console.WriteLine($"Failed to extract '{archive}'");
            }

            try {

                string[] lookIn = {
                    $"{extract}\\scenarios\\mp\\community\\",
                    $"{extract}\\scenarios\\mp\\",
                    $"{extract}\\scenarios\\pm\\community\\",
                    $"{extract}\\scenarios\\pm\\",
                };

                List<string> dirs = new();

                foreach (string look in lookIn) {
                    if (Directory.Exists(look)) {
                        dirs.AddRange(Directory.GetDirectories(look));
                    }
                }

                foreach (string dir in dirs) {

                    // Log current file
                    Console.WriteLine($"Parsing scenario folder: {dir}");

                    // Read and add scenario
                    if (ScenarioList.GetScenarioFromDirectory(dir, Path.GetFileName(archive)) is Scenario scen) {
                        if (scen.IsVisibleInLobby) {
                            scenarios.Add(scen);
                        } else {
                            Console.WriteLine($"Skipping {scen.RelativeFilename} - NOT visible in lobby!");
                        }
                    }

                }

            } catch (Exception e) {
                Console.WriteLine($"Failed to read sga \"{archive}\" (Skipping, message = '{e.Message}')");
            }


        }

        // Save database
        File.WriteAllText("vcoh-map-db.json", JsonSerializer.Serialize(scenarios, options: new() { WriteIndented = true, IncludeFields = true }));

    }

    public static void ReadTestmap() {

        var initpath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "my games\\company of heroes 2\\mods\\scenarios\\bg_testmap");
        var scen = Scenario.ReadScenario(Path.Combine(initpath, "bg_testmap_lao.dds"), Path.Combine(initpath, "bg_testmap.info"), Path.Combine(initpath, "bg_testmap.options"), "");

    }

}
