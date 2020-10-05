using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Battlegrounds.Functional;
using Battlegrounds.Compiler;
using Battlegrounds.Json;
using System.Threading.Tasks;
using System.Threading;

namespace Battlegrounds.Game.Database {
    
    /// <summary>
    /// A list of all available scenarios that can be played.
    /// </summary>
    public static class ScenarioList {

        private static Dictionary<string, Scenario> __scenarios;

        /// <summary>
        /// Create and load a basic list of scenarios.
        /// </summary>
        public static void LoadList() {

            // Create the dictionary
            __scenarios = new Dictionary<string, Scenario>();

            try {

                if (JsonParser.Parse($"{DatabaseManager.SolveDatabasepath()}vcoh-map-db.json").FirstOrDefault() is JsonArray array) {

                    foreach (IJsonElement jsonElement in array) {

                        if (jsonElement is Scenario scenario) {
                            __scenarios.Add(Path.GetFileNameWithoutExtension(scenario.RelativeFilename), scenario);
                        }

                    }

                } else {

                    throw new InvalidDataException("The ScenarioList database was set-up incorrectly!");

                }

                try {
                    Task.Run(LoadWorkshopScenarios);
                } catch (Exception e) {
                    Trace.WriteLine(e);
                }

            } catch (Exception e) {
                Trace.WriteLine(e);
            }

        }

        private class WorkshopRecord : IJsonObject {
            public List<Scenario> scenarios = new List<Scenario>();
            public string ToJsonReference() => throw new NotImplementedException();
        }

        private static void LoadWorkshopScenarios() {

            List<Scenario> workshopScenarios = new List<Scenario>();
            string workshop_dbpath = $"{DatabaseManager.SolveDatabasepath()}workshop-map-db.json";
            if (File.Exists(workshop_dbpath)) {
                if (JsonParser.Parse(workshop_dbpath).FirstOrDefault() is WorkshopRecord array) {
                    foreach (IJsonElement jsonElement in array.scenarios) {

                        if (jsonElement is Scenario scenario) {
                            workshopScenarios.Add(scenario);
                        }

                    }
                } else {

                    throw new InvalidDataException("The ScenarioList database was set-up incorrectly!");

                }
            }

            string workshopScenarioFolder = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\my games\\Company of Heroes 2\\Mods\\Scenarios\\subscriptions";
            string[] files = Directory.GetFiles(workshopScenarioFolder, "*.sga");
            int newWorkshopEntries = 0;

            for (int i = 0; i < files.Length; i++) {

                string sga = Path.GetFileNameWithoutExtension(files[i]);
                if (!__scenarios.Any(x => x.Value.SgaName.CompareTo(sga) == 0) && !workshopScenarios.Any(x => x.SgaName.CompareTo(sga) == 0)) {

                    if (!Archiver.Extract(files[i], "~tmp\\~workshop-extract")) {
                        Trace.WriteLine($"Failed to extraxt workshop scenario {sga}.");
                    }

                    try {
                        string readfrom = $"~tmp\\~workshop-extract\\scenarios\\mp\\community\\";
                        if (!Directory.Exists(readfrom)) {
                            readfrom = $"~tmp\\~workshop-extract\\scenarios\\pm\\community\\";
                            if (!Directory.Exists(readfrom)) {
                                continue;
                            }
                        }
                        string[] dirs = Directory.GetDirectories(readfrom);
                        if (dirs.Length > 0) {
                            readfrom = dirs[0];

                            string[] scenarioFiles = Directory.GetFiles(readfrom);
                            Scenario scen = new Scenario(scenarioFiles.FirstOrDefault(x => x.EndsWith(".info")), scenarioFiles.FirstOrDefault(x => x.EndsWith(".options"))) {
                                SgaName = sga
                            };

                            workshopScenarios.Add(scen);

                            Directory.Delete(readfrom, true);

                            newWorkshopEntries++;

                        } else {
                            Trace.WriteLine($"Failed to read sga \"{sga}\" (Skipping)");
                        }
                    } catch { }

                }

            }

            if (newWorkshopEntries > 0) {

                IJsonObject rec = new WorkshopRecord {
                    scenarios = workshopScenarios
                };

                // Save database
                File.WriteAllText(workshop_dbpath, rec.Serialize());

                Trace.WriteLine($"Added {newWorkshopEntries} workshop maps.");

            }

            // Add all
            workshopScenarios.ForEach(x =>__scenarios.IfTrue(y => !y.ContainsKey(Path.GetFileNameWithoutExtension(x.RelativeFilename)))
                .Then(y => y.Add(Path.GetFileNameWithoutExtension(x.RelativeFilename), x)));

        }

        /// <summary>
        /// Add a new <see cref="Scenario"/> to the scenario list.
        /// </summary>
        /// <param name="scenario">The new scenario to add.</param>
        /// <returns>Will return true if the <see cref="Scenario"/> was added. Otherwise false - <see cref="Scenario"/> already exists.</returns>
        public static bool AddScenario(Scenario scenario) {
            string relpath = Path.GetFileNameWithoutExtension(scenario.RelativeFilename);
            if (!__scenarios.ContainsKey(relpath)) {
                __scenarios.Add(relpath, scenario);
                return true;
            } else {
                return false;
            }
        }

        /// <summary>
        /// Get a <see cref="Scenario"/> from its file name.
        /// </summary>
        /// <param name="filename">The name of the scenario's file name to get.</param>
        /// <returns>The found scenario. If no matching scenario is found, an exception is thrown.</returns>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="KeyNotFoundException"/>
        public static Scenario FromFilename(string filename) => __scenarios[filename];

        public static List<Scenario> GetList() => __scenarios.Values.ToList();

    }

}
