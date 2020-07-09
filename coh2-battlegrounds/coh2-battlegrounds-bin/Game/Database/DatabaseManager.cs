using System;
using System.IO;
using System.Threading.Tasks;

namespace Battlegrounds.Game.Database {

    /// <summary>
    /// Callback handler for the <see cref="DatabaseManager"/> being done loading databases.
    /// </summary>
    /// <param name="db_loaded">The amount of databases that were loaded.</param>
    /// <param name="db_failed">The amount of databases that failed to load.</param>
    public delegate void DatabaseLoadedCallbackHandler(int db_loaded, int db_failed);

    /// <summary>
    /// Static class for abstracted interaction with the database managers.
    /// </summary>
    public static class DatabaseManager {

        private static bool m_databasesLoaded = false;

        private static string m_databaseFoundPath = null;

        /// <summary>
        /// Has the databases been loaded.
        /// </summary>
        public static bool DatabaseLoaded => m_databasesLoaded;

        /// <summary>
        /// Load all databases in a non-interupting manner.
        /// </summary>
        public static async void LoadAllDatabases(DatabaseLoadedCallbackHandler onDatabaseLoaded) {

            // There's no need to do this twice
            if (m_databasesLoaded) {
                return;
            }

            Console.WriteLine($"Database path: \"{SolveDatabasepath()}\"");

            int loaded = 0;
            int failed = 0;

            try {

                // Wait for the blueprint database to load
                await Task.Run(BlueprintManager.CreateDatabase);

                // Wait for additional blueprint loaded
                await Task.Run(() => {

                    // Load the battlegrounds DB
                    BlueprintManager.LoadDatabaseWithMod("battlegrounds", BattlegroundsInstance.BattleGroundsTuningMod.Guid.ToString());

                    // TODO: Load more here if needed

                });

                // Increment the loaded count
                loaded++;

            } catch (Exception e) {
                Console.WriteLine($"Failed to load database: Blueprints [{e.Message}]");
                failed++;
            }

            try {

                // Wait for the scenario list to load
                await Task.Run(ScenarioList.LoadList);

                // Increment the loaded count
                loaded++;

            } catch (Exception e) {
                Console.WriteLine($"Failed to load database: Scenarios [{e.Message}]");
                failed++;
            }

            // Create the win condition list (less troublesome)
            await Task.Run(WinconditionList.CreateAndLoadDatabase);
            loaded++;

            // Set database loaded
            m_databasesLoaded = true;

            // Invoke the callback
            onDatabaseLoaded?.Invoke(loaded, failed);

        }

        /// <summary>
        /// Try and solve (find) a valid path to the database.
        /// </summary>
        /// <returns>The first valid database path found or the empty string if no path is found.</returns>
        public static string SolveDatabasepath() {
            if (m_databaseFoundPath != null) {
                return m_databaseFoundPath;
            } else {
                string[] testPaths = new string[] {
                    "..\\..\\..\\..\\..\\db-battlegrounds\\",
                    "..\\..\\..\\..\\db-battlegrounds\\",
                    "..\\..\\..\\db-battlegrounds\\",
                    "db-battlegrounds\\",
                    "data\\json-db\\", // should be the place for release builds!
                };
                foreach (string path in testPaths) {
                    if (Directory.Exists(path)) {
                        m_databaseFoundPath = Path.GetFullPath(path);
                        return m_databaseFoundPath;
                    }
                }
                m_databaseFoundPath = string.Empty;
                return m_databaseFoundPath;
            }
        }

    }

}
