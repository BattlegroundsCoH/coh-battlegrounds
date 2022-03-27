using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

using Battlegrounds.Modding;

namespace Battlegrounds.Game.Database.Management {

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

        private static bool __databasesLoaded = false;
        private static string __databaseFoundPath = string.Empty;

        /// <summary>
        /// Get the path of the mod database folder
        /// </summary>
        public static readonly string ModDatabaseSource = "usr\\mods\\mod_db\\";

        /// <summary>
        /// Has the databases been loaded.
        /// </summary>
        public static bool DatabaseLoaded => __databasesLoaded;

        /// <summary>
        /// Load all databases in a non-interupting manner.
        /// </summary>
        public static async void LoadAllDatabases(DatabaseLoadedCallbackHandler onDatabaseLoaded) {

            // There's no need to do this twice
            if (__databasesLoaded) {
                return;
            }

            Trace.WriteLine($"Database path: \"{SolveDatabasepath()}\"", nameof(DatabaseManager));

            int loaded = 0;
            int failed = 0;

            try {

                // Wait for the blueprint database to load
                await Task.Run(BlueprintManager.CreateDatabase);

                // Wait for additional blueprint loaded
                await Task.Run(() => {

                    // Run through each mod package and load their database
                    ModManager.EachPackage(x => {
                        Stopwatch stopwatch = Stopwatch.StartNew();
                        BlueprintManager.LoadDatabaseWithMod(x.ID, x.TuningGUID.GUID);
                        stopwatch.Stop();
                        Trace.WriteLine($"Loaded database for package '{x.PackageName}' in {stopwatch.Elapsed.TotalSeconds:0.000}s.", nameof(BlueprintManager));
                    });

                });

                // Increment the loaded count
                loaded++;

            } catch (Exception e) {
                Trace.WriteLine($"Failed to load database: Blueprints [{e.Message}]", nameof(DatabaseManager));
                failed++;
            }

            try {

                // Wait for the scenario list to load
                await Task.Run(ScenarioList.LoadList);

                // Increment the loaded count
                loaded++;

            } catch (Exception e) {
                Trace.WriteLine($"Failed to load database: Scenarios [{e.Message}]", nameof(DatabaseManager));
                failed++;
            }

            // Set database loaded flag
            __databasesLoaded = true;

            // Invoke the callback
            onDatabaseLoaded?.Invoke(loaded, failed);

        }

        /// <summary>
        /// Try and solve (find) a valid path to the database.
        /// </summary>
        /// <returns>The first valid database path found or the empty string if no path is found.</returns>
        public static string SolveDatabasepath() {
            
            // Bail fast if already solved
            if (!string.IsNullOrEmpty(__databaseFoundPath)) {
                return __databaseFoundPath;
            }

            // Define paths to test existance of.
            string[] testPaths = new string[] {
                    "..\\..\\..\\..\\..\\..\\db-battlegrounds\\",
                    "..\\..\\..\\..\\..\\db-battlegrounds\\",
                    "..\\..\\..\\..\\db-battlegrounds\\",
                    "..\\..\\..\\db-battlegrounds\\",
                    "db-battlegrounds\\",
                    "bg_common\\data\\", // should be the place for release builds!
                    ModDatabaseSource, // should be the place for mods
                };
            
            // Do exist check, return first
            foreach (string path in testPaths) {
                if (Directory.Exists(path)) {
                    __databaseFoundPath = Path.GetFullPath(path);
                    return __databaseFoundPath;
                }
            }

            // Log failure
            Trace.WriteLine("Failed to find any valid path to the database folder. Please verify install path.", nameof(DatabaseManager));

            // Return none
            return __databaseFoundPath;

        }

    }

}
