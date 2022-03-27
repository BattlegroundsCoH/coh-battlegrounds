using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;

using Battlegrounds.ErrorHandling;
using Battlegrounds.Functional;
using Battlegrounds.Modding;

using RegexMatch = System.Text.RegularExpressions.Match;

namespace Battlegrounds.Game.Database.Management;

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

    /// <summary>
    /// Weapon Blueprint
    /// </summary>
    WBP = 64,

}

/// <summary>
/// Database over all relevant blueprints in use
/// </summary>
public static class BlueprintManager {

    /// <summary>
    /// Const value for the invalid local blueprint ID.
    /// </summary>
    public const ushort InvalidLocalBlueprint = 0;

    private static Dictionary<BlueprintUID, Blueprint>? __entities;
    private static Dictionary<BlueprintUID, Blueprint>? __squads;
    private static Dictionary<BlueprintUID, Blueprint>? __abilities;
    private static Dictionary<BlueprintUID, Blueprint>? __upgrades;
    private static Dictionary<BlueprintUID, Blueprint>? __criticals;
    private static Dictionary<BlueprintUID, Blueprint>? __slotitems;
    private static Dictionary<BlueprintUID, Blueprint>? __weapons;

    private static List<Dictionary<BlueprintUID, Blueprint>>? __selfList;

    /// <summary>
    /// Create the database (or clear it) and load the vcoh database.
    /// </summary>
    public static void CreateDatabase() {

        // Create Dictionaries
        __entities = new();
        __squads = new();
        __abilities = new();
        __upgrades = new();
        __criticals = new();
        __slotitems = new();
        __weapons = new();

        // Create list over self
        __selfList = new List<Dictionary<BlueprintUID, Blueprint>>() {
                __abilities,
                __criticals,
                __entities,
                __slotitems,
                __squads,
                __upgrades,
                __weapons,
            };

        // Setup load timer
        Stopwatch stopwatch = Stopwatch.StartNew();

        // Load the vcoh database
        LoadDatabaseWithMod("vcoh", string.Empty);

        // Stop watch and log
        stopwatch.Stop();
        Trace.WriteLine($"Loaded database for basegame blueprints in {stopwatch.Elapsed.TotalSeconds:0.000}s.", nameof(BlueprintManager));

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

        // Try also look in mod database
        bool modFolder = Directory.Exists(DatabaseManager.ModDatabaseSource);
        if (mod is not "vcoh" && modFolder) {
            db_paths = db_paths.Concat(Directory.GetFiles(DatabaseManager.ModDatabaseSource, "*.json"));
        }

        // The mod-local pbgid
        ushort modPbgidCntr = 1;

        // loop through all the json paths
        for (int i = 0; i < db_paths.Length; i++) {
            RegexMatch match = Regex.Match(db_paths[i], $@"{mod}-(?<type>\w{{3}})-db"); // Do regex match
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

        // Throw exceptions if anything failed or no database was loaded.
        if (failCounter is not 0 || loadCounter is 0) {
            throw new Exception("Failed to load one or more blueprint databases!");
        }

    }

    internal static bool LoadJsonDatabase(string jsonfile, BlueprintType bType, string guid, ref ushort bpCntr) {

        // Create the ModGUID
        var modguid = ModGuid.FromGuid(guid);

        // Parse the file
        var jsonFileData = File.ReadAllText(jsonfile);
        var typeArray = GetManagedTypeFromBlueprintType(bType).MakeArrayType();

        // If null, yield
        if (JsonSerializer.Deserialize(jsonFileData, typeArray) is not Array blueprints)
            return false;

        // If Empty
        if (blueprints.Length is 0) {
            return false;
        }

        // Get target array
        var target = GetAllBlueprintsOfType(bType);

        // Loop over blueprints
        for (int i = 0; i < blueprints.Length; i++) {
            if (blueprints.GetValue(i) is Blueprint bp) {
                bp.ModPBGID = bpCntr++;
                target.Add(bp.PBGID, bp);
            }
        }

        // Log
        Trace.WriteLine($"Loaded {blueprints.Length} {bType}s for {(string.IsNullOrEmpty(guid) ? "base game" : $"mod[{guid}]")}", nameof(BlueprintManager));

        // Return true
        return true;

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public static List<Blueprint> Select(Predicate<Blueprint> predicate) {

        // TODO: Make a more optimized and specific version of this...
        var blueprints = new List<Blueprint>();

        if (__selfList is null)
            return blueprints;

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
    public static Blueprint FromPbgId(ushort id, BlueprintType bType) => GetAllBlueprintsOfType(bType)[new(id)];

    /// <summary>
    /// Get a <see cref="Blueprint"/> instance from its string name (file name).
    /// </summary>
    /// <param name="id">The string ID to look for in sub-databases.</param>
    /// <param name="bType">The <see cref="BlueprintType"/> to look for when looking up the <see cref="Blueprint"/>.</param>
    /// <exception cref="ArgumentException"/>
    /// <exception cref="ArgumentNullException"/>
    /// <returns>The correct <see cref="Blueprint"/>, null if not found or a <see cref="ArgumentException"/> if <see cref="BlueprintType"/> was somehow invalid.</returns>
    public static Blueprint FromBlueprintName(string id, BlueprintType bType) {
        if (GetModGUID(id, out ModGuid guid, out string bp)) {
            return GetAllBlueprintsOfType(bType).FirstOrDefault(x => x.Value?.PBGID.Mod == guid && x.Value?.Name == bp).Value;
        }
        return GetAllBlueprintsOfType(bType).FirstOrDefault(x => x.Value?.Name == id).Value;
    }

    /// <summary>
    /// Get a <see cref="Blueprint"/> instance from its string name (file name).
    /// </summary>
    /// <typeparam name="Bp">The specific <see cref="Blueprint"/> type to fetch.</typeparam>
    /// <param name="bpName">The string ID to look for.</param>
    /// <exception cref="ArgumentException"/>
    /// <exception cref="ArgumentNullException"/>
    /// <returns>The correct <see cref="Blueprint"/>, null if not found or a <see cref="ArgumentException"/> if <see cref="BlueprintType"/> was somehow invalid.</returns>
    public static Bp FromBlueprintName<Bp>(string bpName) where Bp : Blueprint
        => (Bp)FromBlueprintName(bpName, BlueprintTypeFromType<Bp>());

    /// <summary>
    /// Get all blueprints in database of <paramref name="type"/>.
    /// </summary>
    /// <param name="type">The type of blueprint to retrieve.</param>
    /// <returns>Dictionary of <see cref="Blueprint"/> instances linked with their <see cref="BlueprintUID"/> keys.</returns>
    /// <exception cref="ArgumentException"/>
    /// <exception cref="EnvironmentException"/>
    public static Dictionary<BlueprintUID, Blueprint> GetAllBlueprintsOfType(BlueprintType type) => (type switch {
        BlueprintType.ABP => __abilities,
        BlueprintType.CBP => __criticals,
        BlueprintType.EBP => __entities,
        BlueprintType.SBP => __squads,
        BlueprintType.UBP => __upgrades,
        BlueprintType.IBP => __slotitems,
        BlueprintType.WBP => __weapons,
        _ => throw new ArgumentException(null, nameof(type)),
    }) ?? throw new EnvironmentException("Fatal error: Database not instantiated.");

    /// <summary>
    /// Get the <see cref="Type"/> representing the concrete blueprint type of the <see cref="BlueprintType"/> enum value.
    /// </summary>
    /// <param name="type">The <see cref="BlueprintType"/> to get managed <see cref="Type"/> from.</param>
    /// <returns>The <see cref="Type"/> of the concrete <see cref="Blueprint"/> type associated with <see cref="BlueprintType"/>.</returns>
    /// <exception cref="ArgumentException"/>
    public static Type GetManagedTypeFromBlueprintType(BlueprintType type) => type switch {
        BlueprintType.ABP => typeof(AbilityBlueprint),
        BlueprintType.CBP => typeof(CriticalBlueprint),
        BlueprintType.EBP => typeof(EntityBlueprint),
        BlueprintType.SBP => typeof(SquadBlueprint),
        BlueprintType.UBP => typeof(UpgradeBlueprint),
        BlueprintType.IBP => typeof(SlotItemBlueprint),
        BlueprintType.WBP => typeof(WeaponBlueprint),
        _ => throw new ArgumentException($"Cannot get blueprint type of '{type}'", nameof(type))
    };

    /// <summary>
    /// Get the <see cref="BlueprintType"/> from the given type parameter <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Concrete <see cref="Blueprint"/> type.</typeparam>
    /// <returns>The <see cref="BlueprintType"/> associated with <typeparamref name="T"/>.</returns>
    /// <exception cref="ArgumentException"/>
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
        } else if (typeof(T) == typeof(WeaponBlueprint)) {
            return BlueprintType.WBP;
        } else {
            throw new ArgumentException("Invalid type argument");
        }
    }

    /// <summary>
    /// Retrieve the <see cref="ModGuid"/> from a string fully qualified blueprint name.
    /// </summary>
    /// <param name="bpname">The full blueprint name.</param>
    /// <param name="modGuid">The stored mod GUID in the blueprint name.</param>
    /// <param name="bp">The blueprint that follows the mod guid.</param>
    /// <returns>If extraction of both values was successful, <see langword="true"/>; Otherwise <see langword="false"/>.</returns>
    public static bool GetModGUID(string bpname, out ModGuid modGuid, out string bp) {
        int j = bpname.IndexOf(':');
        if (j == ModGuid.FIXED_LENGTH) {
            modGuid = ModGuid.FromGuid(bpname[0..j]);
            bp = bpname[(j + 1)..];
            return true;
        } else {
            modGuid = ModGuid.FromGuid(Guid.Empty);
            bp = bpname;
            return false;
        }
    }

}
