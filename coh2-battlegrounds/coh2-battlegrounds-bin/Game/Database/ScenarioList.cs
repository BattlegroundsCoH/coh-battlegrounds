using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Battlegrounds.Json;

namespace Battlegrounds.Game.Database {
    
    /// <summary>
    /// 
    /// </summary>
    public static class ScenarioList {

        private static Dictionary<string, Scenario> __scenarios;

        /// <summary>
        /// 
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

                    Console.WriteLine("");

                } else {
                    Console.WriteLine("");
                }

            } catch (Exception e) {
                Console.WriteLine(e.Message);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static Scenario FromFilename(string filename) => __scenarios[filename];

    }

}
