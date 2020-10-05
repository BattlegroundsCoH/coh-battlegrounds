using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Battlegrounds.Json;

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
                    LoadWorkshopScenarios();
                } catch (Exception e) {
                    Trace.WriteLine(e);
                }

            } catch (Exception e) {
                Trace.WriteLine(e);
            }

        }

        private static void LoadWorkshopScenarios() {

            string workshopScenarioFolder = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\my games\\Company of Heroes 2\\Mods\\Scenarios\\subscriptions";
            string[] files = Directory.GetFiles(workshopScenarioFolder, "*.sga");

            for (int i = 0; i < files.Length; i++) {
                using (BinaryReader sgareader = new BinaryReader(File.OpenRead(files[i]))) {

                    if (sgareader.ReadBytes(8).SequenceEqual(Encoding.ASCII.GetBytes("_ARCHIVE"))) {

                        long jmp = 16 * 16;

                        sgareader.BaseStream.Seek(jmp, SeekOrigin.Begin);

                        byte[] nameBytes = sgareader.ReadBytes(128);

                        string name = Encoding.ASCII.GetString(nameBytes);

                        Scenario scen = new Scenario(); // TODO: Finish this...

                    }

                }
            }

            return;

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
