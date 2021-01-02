using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using Battlegrounds.Json;

namespace Battlegrounds.Game.Database {

    /// <summary>
    /// The type a <see cref="Blueprint"/> may represent in the <see cref="BlueprintManager"/>.
    /// </summary>
    public enum BlueprintType {
        
        /// <summary>
        /// Ability Blueprint
        /// </summary>
        ABP,

        /// <summary>
        /// Upgrade Blueprint
        /// </summary>
        UBP,
        
        /// <summary>
        /// Critical Blueprint
        /// </summary>
        CBP,

        /// <summary>
        /// Slot Item Blueprint
        /// </summary>
        IBP,

        /// <summary>
        /// Entity Blueprint
        /// </summary>
        EBP = 16,

        /// <summary>
        /// Squad Blueprint
        /// </summary>
        SBP = 32,

    }

    /// <summary>
    /// Database over all relevant blueprints in use
    /// </summary>
    public static class BlueprintManager {

        private static Dictionary<ulong, Blueprint> __entities;
        private static Dictionary<ulong, Blueprint> __squads;
        private static Dictionary<ulong, Blueprint> __abilities;
        private static Dictionary<ulong, Blueprint> __upgrades;
        private static Dictionary<ulong, Blueprint> __criticals;
        private static Dictionary<ulong, Blueprint> __slotitems;

        private static List<Dictionary<ulong, Blueprint>> __selfList;

        /// <summary>
        /// Create the database (or clear it) and load the vcoh database.
        /// </summary>
        public static void CreateDatabase() {

            // Create Dictionaries
            __entities = new Dictionary<ulong, Blueprint>();
            __squads = new Dictionary<ulong, Blueprint>();
            __abilities = new Dictionary<ulong, Blueprint>();
            __upgrades = new Dictionary<ulong, Blueprint>();
            __criticals = new Dictionary<ulong, Blueprint>();
            __slotitems = new Dictionary<ulong, Blueprint>();

            // Create list over self
            __selfList = new List<Dictionary<ulong, Blueprint>>() {
                __abilities,
                __criticals,
                __entities,
                __slotitems,
                __squads,
                __upgrades,
            };

            // Load the vcoh database
            LoadDatabaseWithMod("vcoh", string.Empty);

        }

        /// <summary>
        /// Load a database for a specific mod.
        /// </summary>
        /// <param name="mod">The name of the mod to load.</param>
        /// <param name="guid">The GUID of the mod to load.</param>
        public static void LoadDatabaseWithMod(string mod, string guid) {

            // Get the database path
            string dbpath = DatabaseManager.SolveDatabasepath();

            // Load data here
            string[] db_paths = Directory.GetFiles(dbpath, "*.json");
            int failCounter = 0;
            int loadCounter = 0;

            // loop through all the json paths
            for (int i = 0; i < db_paths.Length; i++) {
                Match match = Regex.Match(db_paths[i], $@"{mod}-(?<type>\w{{3}})-db"); // Do regex match
                if (match.Success) { // If match found
                    string toUpper = match.Groups["type"].Value.ToUpper(); // get the type in upper form
                    if (Enum.TryParse(toUpper, out BlueprintType t)) { // try and parse to blueprint type
                        if (!LoadJsonDatabase(db_paths[i], t, guid)) { // load the db
                            failCounter++; // we failed, so increment fail counter.
                        } else {
                            loadCounter++;
                        }
                    }
                }
            }

            if (failCounter > 0 || loadCounter == 0) {
                throw new Exception("Failed to load one or more blueprint databases!");
            }

        }

        internal static bool LoadJsonDatabase(string jsonfile, BlueprintType bType, string guid) {

            // Parse the file
            var ls = JsonParser.Parse(jsonfile);

            // If Empty
            if (ls.Count == 0) {
                return false;
            }

            // Make sure we got a json array
            if (ls.First() is JsonArray content) {

                // Get the target dictionary
                var targetDictionary = bType switch
                {
                    BlueprintType.ABP => __abilities,
                    BlueprintType.CBP => __criticals,
                    BlueprintType.EBP => __entities,
                    BlueprintType.SBP => __squads,
                    BlueprintType.UBP => __upgrades,
                    BlueprintType.IBP => __slotitems,
                    _ => throw new ArgumentException(),
                };

                // Add all the found elements
                foreach (IJsonElement element in content) {
                    if (element is Blueprint bp) {
                        bp.BlueprintType = bType;
                        bp.ModGUID = guid;
                        targetDictionary.Add(bp.PBGID, bp);
                    } else {
                        return false;
                    }
                }

            } else {
                return false;
            }

            return true;

        }

        public static List<Blueprint> Select(Predicate<Blueprint> predicate) {
            // TODO: Make a more optimized and specific version of this...
            List<Blueprint> blueprints = new List<Blueprint>();

            foreach (var type in __selfList) {
                blueprints.AddRange(type.Values.Where(x => predicate(x)));
            }
            
            return blueprints;

        }

        /// <summary>
        /// Get a <see cref="Blueprint"/> instance by its unique ID and <see cref="BlueprintType"/>.
        /// </summary>
        /// <param name="id">The unique ID of the blueprint to find.</param>
        /// <param name="bType">The <see cref="BlueprintType"/> to look for when looking up the <see cref="Blueprint"/>.</param>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="KeyNotFoundException"/>
        /// <returns>The correct <see cref="Blueprint"/>, null if not found or a <see cref="ArgumentException"/> if <see cref="BlueprintType"/> was somehow invalid.</returns>
        public static Blueprint FromPbgId(ushort id, BlueprintType bType) {
            return bType switch
            {
                BlueprintType.ABP => __abilities[id],
                BlueprintType.CBP => __criticals[id],
                BlueprintType.EBP => __entities[id],
                BlueprintType.SBP => __squads[id],
                BlueprintType.UBP => __upgrades[id],
                BlueprintType.IBP => __slotitems[id],
                _ => throw new ArgumentException(),
            };
        }

        /// <summary>
        /// Get a <see cref="Blueprint"/> instance from its string name (file name).
        /// </summary>
        /// <param name="id">The string ID to look for in sub-databases.</param>
        /// <param name="bType">The <see cref="BlueprintType"/> to look for when looking up the <see cref="Blueprint"/>.</param>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <returns>The correct <see cref="Blueprint"/>, null if not found or a <see cref="ArgumentException"/> if <see cref="BlueprintType"/> was somehow invalid.</returns>
        public static Blueprint FromBlueprintName(string id, BlueprintType bType) {
            if (id.Contains(":")) {
                id = id.Substring(id.IndexOf(':') + 1);
                Console.WriteLine(id);
            }
            return bType switch
            {
                BlueprintType.ABP => __abilities.FirstOrDefault(x => (x.Value.Name ?? "").CompareTo(id) == 0).Value,
                BlueprintType.CBP => __criticals.FirstOrDefault(x => (x.Value.Name ?? "").CompareTo(id) == 0).Value,
                BlueprintType.EBP => __entities.FirstOrDefault(x => (x.Value.Name ?? "").CompareTo(id) == 0).Value,
                BlueprintType.SBP => __squads.FirstOrDefault(x => (x.Value.Name ?? "").CompareTo(id) == 0).Value,
                BlueprintType.UBP => __upgrades.FirstOrDefault(x => (x.Value.Name ?? "").CompareTo(id) == 0).Value,
                BlueprintType.IBP => __slotitems.FirstOrDefault(x => (x.Value.Name ?? "").CompareTo(id) == 0).Value,
                _ => throw new ArgumentException(),
            };
        }

        /// <summary>
        /// Dereference a <see cref="Blueprint"/> reference from a json reference string of the form "BPT:BPName".
        /// </summary>
        /// <param name="jsonReference">The json reference string to dereference.</param>
        /// <returns>The <see cref="Blueprint"/> instance matching the reference in the database.</returns>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="OverflowException"/>
        /// <exception cref="FormatException"/>
        public static IJsonObject JsonDereference(string jsonReference) {
            BlueprintType type = (BlueprintType)Enum.Parse(typeof(BlueprintType), jsonReference.Substring(0, 3));
            if (ushort.TryParse(jsonReference.Substring(4), out ushort result)) {
                return FromPbgId(result, type);
            } else {
                return FromBlueprintName(jsonReference.Substring(4), type);
            }
        }
    }

}
