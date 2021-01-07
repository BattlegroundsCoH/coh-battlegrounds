using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Battlegrounds.Functional;
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

        /// <summary>
        /// Const value for the invalid local blueprint ID.
        /// </summary>
        public const ushort InvalidLocalBlueprint = 0;

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

            // The mod-local pbgid
            ushort modPbgidCntr = 1;

            // loop through all the json paths
            for (int i = 0; i < db_paths.Length; i++) {
                Match match = Regex.Match(db_paths[i], $@"{mod}-(?<type>\w{{3}})-db"); // Do regex match
                if (match.Success) { // If match found
                    string toUpper = match.Groups["type"].Value.ToUpper(); // get the type in upper form
                    if (Enum.TryParse(toUpper, out BlueprintType t)) { // try and parse to blueprint type
                        if (!LoadJsonDatabase(db_paths[i], t, guid, ref modPbgidCntr)) { // load the db
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

        internal static bool LoadJsonDatabase(string jsonfile, BlueprintType bType, string guid, ref ushort bpCntr) {

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
                    _ => throw new ArgumentException("Unkown blueprint type", nameof(bType)),
                };

                // Add all the found elements
                foreach (IJsonElement element in content) {
                    if (element is Blueprint bp) {
                        bp.BlueprintType = bType;
                        bp.ModGUID = guid;
                        bp.ModPBGID = bpCntr++;
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static List<Blueprint> Select(Predicate<Blueprint> predicate) {
            // TODO: Make a more optimized and specific version of this...
            List<Blueprint> blueprints = new List<Blueprint>();

            foreach (var type in __selfList) {
                blueprints.AddRange(type.Values.Where(x => predicate(x)));
            }
            
            return blueprints;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static BlueprintCollection<Blueprint> GetCollection(BlueprintType type) => new BlueprintCollection<Blueprint>(GetAllBlueprintsOfType(type));

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static BlueprintCollection<T> GetCollection<T>() where T : Blueprint => new BlueprintCollection<T>(GetAllBlueprintsOfType(BlueprintTypeFromType<T>()));

        /// <summary>
        /// Get a <see cref="Blueprint"/> instance by its unique ID and <see cref="BlueprintType"/>.
        /// </summary>
        /// <param name="id">The unique ID of the blueprint to find.</param>
        /// <param name="bType">The <see cref="BlueprintType"/> to look for when looking up the <see cref="Blueprint"/>.</param>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="KeyNotFoundException"/>
        /// <returns>The correct <see cref="Blueprint"/>, null if not found or a <see cref="ArgumentException"/> if <see cref="BlueprintType"/> was somehow invalid.</returns>
        public static Blueprint FromPbgId(ushort id, BlueprintType bType) => GetAllBlueprintsOfType(bType)[id];

        /// <summary>
        /// Get a <see cref="Blueprint"/> instance from its string name (file name).
        /// </summary>
        /// <param name="id">The string ID to look for in sub-databases.</param>
        /// <param name="bType">The <see cref="BlueprintType"/> to look for when looking up the <see cref="Blueprint"/>.</param>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <returns>The correct <see cref="Blueprint"/>, null if not found or a <see cref="ArgumentException"/> if <see cref="BlueprintType"/> was somehow invalid.</returns>
        public static Blueprint FromBlueprintName(string id, BlueprintType bType) => GetAllBlueprintsOfType(bType).FirstOrDefault(x => (x.Value.Name ?? "").CompareTo(id) == 0).Value;

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
            if (ushort.TryParse(jsonReference[4..], out ushort result)) {
                return FromPbgId(result, type);
            } else {
                return FromBlueprintName(jsonReference[4..], type);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Dictionary<ulong, Blueprint> GetAllBlueprintsOfType(BlueprintType type) => type switch {
            BlueprintType.ABP => __abilities,
            BlueprintType.CBP => __criticals,
            BlueprintType.EBP => __entities,
            BlueprintType.SBP => __squads,
            BlueprintType.UBP => __upgrades,
            BlueprintType.IBP => __slotitems,
            _ => throw new ArgumentException(null, nameof(type)),
        };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="isMax"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static ulong GetUIDValueExtrema(bool isMax, BlueprintType type)
                => isMax.Then(() => GetAllBlueprintsOfType(type).Max(x => x.Key))
                .Else(_ => GetAllBlueprintsOfType(type).Min(x => x.Key));

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="isMax"></param>
        /// <param name="type"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static TResult GetValueExtrema<TResult>(bool isMax, BlueprintType type, Func<Blueprint, TResult> selector, Predicate<KeyValuePair<ulong, Blueprint>> predicate = null)
            => isMax.Then(() => GetAllBlueprintsOfType(type).Where(x => predicate?.Invoke(x) ?? true).Max(x => selector(x.Value)))
                .Else(_ => GetAllBlueprintsOfType(type).Where(x => predicate?.Invoke(x) ?? true).Min(x => selector(x.Value)));

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TBlueprint"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="isMax"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static TResult GetValueExtrema<TBlueprint, TResult>(bool isMax, Func<TBlueprint, TResult> selector, Predicate<KeyValuePair<ulong, TBlueprint>> predicate = null) 
            where TBlueprint : Blueprint
            => isMax.Then(() => GetAllBlueprintsOfType(BlueprintTypeFromType<TBlueprint>())
            .Where(x => predicate?.Invoke(new KeyValuePair<ulong, TBlueprint>(x.Key, x.Value as TBlueprint)) ?? true)
            .Max(x => selector(x.Value as TBlueprint)))
                .Else(_ => GetAllBlueprintsOfType(BlueprintTypeFromType<TBlueprint>())
                .Where(x => predicate?.Invoke(new KeyValuePair<ulong, TBlueprint>(x.Key, x.Value as TBlueprint)) ?? true).Min(x => selector(x.Value as TBlueprint)));

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static BlueprintType BlueprintTypeFromType<T>() where T : Blueprint {
            if (typeof(T) == typeof(SquadBlueprint)) {
                return BlueprintType.SBP;
            } else if (typeof(T) == typeof(EntityBlueprint)) {
                return BlueprintType.EBP;
            } else if (typeof(T) == typeof(AbilityBlueprint)) {
                return BlueprintType.ABP;
            } else if (typeof(T) == typeof(SlotItemBlueprint)) {
                return BlueprintType.IBP;
            } else if (typeof(T) == typeof(CriticalBlueprint)) {
                return BlueprintType.CBP;
            } else if (typeof(T) == typeof(UpgradeBlueprint)) {
                return BlueprintType.UBP;
            } else {
                throw new ArgumentException("Invalid type argument");
            }
        }

    }

}
