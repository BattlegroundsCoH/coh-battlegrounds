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
                    Task.Run(HandleWorkshopFiles);
                } catch (Exception e) {
                    Trace.WriteLine(e);
                }

            } catch (Exception e) {
                Trace.WriteLine(e);
            }

        }

        private record WorkshopRecord(List<Scenario> Scenarios) : IJsonObject {
            public string ToJsonReference() => throw new NotImplementedException();
            public WorkshopRecord() : this(new List<Scenario>()) { }
        }

        private static void HandleWorkshopFiles() {

            // Find path to workshop database
            string workshop_dbpath = $"{DatabaseManager.SolveDatabasepath()}workshop-map-db.json";

            // Load existing workshop items
            var workshopScenarios = LoadWorkshopScenarioDatabase(workshop_dbpath);

            // Load new scenarios
            LoadNewWorkshopScenarios(workshopScenarios, out int newWorkshopEntries);

            // If more than 0, update the database
            if (newWorkshopEntries > 0) {

                // Get the record
                IJsonObject rec = new WorkshopRecord(workshopScenarios);

                // Save database
                File.WriteAllText(workshop_dbpath, rec.Serialize());

                // Log how many new workship maps were added
                Trace.WriteLine($"Added {newWorkshopEntries} workshop maps.");

            }

            // Add all
            workshopScenarios.ForEach(x => __scenarios.IfTrue(y => !y.ContainsKey(Path.GetFileNameWithoutExtension(x.RelativeFilename)))
                .Then(y => y.Add(Path.GetFileNameWithoutExtension(x.RelativeFilename), x)));

        }

        private static List<Scenario> LoadWorkshopScenarioDatabase(string workshop_dbpath) {

            List<Scenario> workshopScenarios = new List<Scenario>();
            if (File.Exists(workshop_dbpath)) {
                if (JsonParser.Parse(workshop_dbpath).FirstOrDefault() is WorkshopRecord array) {
                    foreach (IJsonElement jsonElement in array.Scenarios) {

                        if (jsonElement is Scenario scenario) {
                            workshopScenarios.Add(scenario);
                        }

                    }
                } else {

                    throw new InvalidDataException("The ScenarioList database was set-up incorrectly!");

                }
            }

            return workshopScenarios;

        }

        private static bool IsValidExtractedWorkshopMapDirectory(string x) {
            if (Directory.Exists(x)) {
                string[] d = Directory.GetDirectories(x);
                return d.Length == 1 && Directory.GetFiles(d[0]).Length > 0;
            } else {
                return false;
            }
        }

        private static void LoadNewWorkshopScenarios(List<Scenario> workshopScenarios, out int newWorkshopEntries) {

            string workshopScenarioFolder = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\my games\\Company of Heroes 2\\Mods\\Scenarios\\subscriptions";
            string[] files = Directory.GetFiles(workshopScenarioFolder, "*.sga");
            newWorkshopEntries = 0;

            const string baseExtract = "~tmp\\~workshop-extract\\scenarios\\";

            // If the directory for map icons doesn't exist, create it
            if (!Directory.Exists($"usr\\mods\\map_icons")) {
                Directory.CreateDirectory($"usr\\mods\\map_icons");
            }

            // Loop through all the .sga files in the subscriptions folder
            for (int i = 0; i < files.Length; i++) {

                string sga = Path.GetFileNameWithoutExtension(files[i]);
                if (!__scenarios.Any(x => x.Value.SgaName.CompareTo(sga) == 0) && !workshopScenarios.Any(x => x.SgaName.CompareTo(sga) == 0)) {

                    if (!Archiver.Extract(files[i], "~tmp\\~workshop-extract")) {
                        Trace.WriteLine($"Failed to extraxt workshop scenario {sga}.");
                    }

                    try {

                        // Get the directory to read from
                        string readfrom = $"{baseExtract}\\mp\\community\\"
                            .IfTrue(IsValidExtractedWorkshopMapDirectory)
                            .ElseIf($"{baseExtract}\\mp\\", IsValidExtractedWorkshopMapDirectory)
                            .ElseIf($"{baseExtract}\\pm\\community\\", IsValidExtractedWorkshopMapDirectory)
                            .ElseIf($"{baseExtract}\\pm\\", IsValidExtractedWorkshopMapDirectory)
                            .Else(baseExtract, IsValidExtractedWorkshopMapDirectory);

                        string[] dirs = Directory.GetDirectories(readfrom);
                        if (dirs.Length > 0) {
                            readfrom = dirs[0];

                            string[] scenarioFiles = Directory.GetFiles(readfrom);
                            Scenario scen = new Scenario(scenarioFiles.FirstOrDefault(x => x.EndsWith(".info")), scenarioFiles.FirstOrDefault(x => x.EndsWith(".options"))) {
                                SgaName = sga
                            };

                            workshopScenarios.Add(scen);

                            // Find the index of the minimap
                            int minimapFile = scenarioFiles.IndexOf(x => x.EndsWith("_preview.tga")).IfTrue(x => x == -1).Then(x => scenarioFiles.IndexOf(x => x.EndsWith("_mm.tga")));

                            // Make sure it's a valid index
                            if (minimapFile != -1) {

                                // Save destination
                                string destination = $"usr\\mods\\map_icons\\{scen.Name}_map.tga";
                                if (File.Exists(destination)) {
                                    File.Delete(destination);
                                }

                                // Copy the found index file
                                File.Copy(scenarioFiles[minimapFile], destination);

                            }

                            // Delete the extracted files
                            Directory.Delete(baseExtract, true);

                            // Increment new entries
                            newWorkshopEntries++;

                        } else {
                            Trace.WriteLine($"Failed to read sga \"{sga}\" (Skipping)");
                        }
                    } catch { }

                }

            }

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static Scenario FromRelativeFilename(string filename) => __scenarios.FirstOrDefault(x => x.Value.RelativeFilename.CompareTo(filename) == 0).Value;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="identifier"></param>
        /// <param name="scenario"></param>
        /// <returns></returns>
        public static bool TryFindScenario(string identifier, out Scenario scenario) {
            if (__scenarios.ContainsKey(identifier)) {
                scenario = __scenarios[identifier];
                return true;
            } else {
                scenario = FromRelativeFilename(identifier);
                return scenario is not null;
            }
        }

        public static List<Scenario> GetList() => __scenarios.Values.ToList();

    }

}
