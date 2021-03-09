using System;
using System.Collections.Generic;
using System.IO;

using Battlegrounds;
using Battlegrounds.Compiler;
using Battlegrounds.Game.Database;
using Battlegrounds.Json;

namespace coh2_battlegrounds_console {
    
    public static class MapExtractor {

        public static string Output { get; set; }

        public static void Extract() {

            string path = Pathfinder.GetOrFindCoHPath();
            string[] map_archives = {
                $"{path}CoH2\\Archives\\MPScenarios.sga",
                $"{path}CoH2\\Archives\\MPXP1Scenarios.sga",
            };

            string extract = Path.GetFullPath("~tmp\\scen");
            if (Directory.Exists(extract)) {
                Directory.Delete(extract, true);
            }

            Directory.CreateDirectory(extract);

            List<Scenario> scenarios = new List<Scenario>();

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

                    List<string> dirs = new List<string>();

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

            // Get the record
            IJsonObject rec = new ScenarioList.ScenarioRecord(scenarios);

            // Save database
            File.WriteAllText("vcoh-map-db.json", rec.Serialize());

        }

    }

}
