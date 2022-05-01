using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Diagnostics;
using System.Collections.Generic;

using Battlegrounds.Functional;
using Battlegrounds.Game.Database;

namespace Battlegrounds.Modding;

/// <summary>
/// Static manager class for managing <see cref="ModPackage"/> instances.
/// </summary>
public static class ModManager {

    private static readonly List<ModPackage> __packages = new();
    private static readonly Dictionary<ModGuid, IGameMod> __mods = new();

    /// <summary>
    /// Initialise the <see cref="ModManager"/> and load available <see cref="ModPackage"/> elements.
    /// </summary>
    public static void Init() {

        // Create wincondition list
        WinconditionList.CreateDatabase();

        // Get package files
        string[] packageFiles = Directory.GetFiles(BattlegroundsInstance.GetRelativePath(BattlegroundsPaths.MOD_OTHER_FOLDER), "*.package.json");

        // TODO: Add support for plugins such that plugins handle mod creaton themselves.
        // If no plugin, we do completely default behaviour here.

        // Load all packages
        foreach (string packageFilepath in packageFiles) {
            string packageName = Path.GetFileNameWithoutExtension(packageFilepath);
            try {

                // Read the mod package
                ModPackage? package = JsonSerializer.Deserialize<ModPackage>(File.ReadAllText(packageFilepath));
                if (package is null) {
                    Trace.WriteLine($"Failed to load mod package '{packageFilepath}' (Error reading file).", nameof(ModManager));
                    continue;
                }

                if (__packages.Any(x => x.ID == package.ID)) {
                    Trace.WriteLine($"Failed to load mod package '{package.ID}' (Duplicate ID entry).", nameof(ModManager));
                    continue;
                }

                // Add mod package
                __packages.Add(package);

                // Submod counter
                int submods = 0;

                // Load asset pack
                if (package.AssetGUID != ModGuid.BaseGame) {
                    __mods[package.AssetGUID] = new CustomAsset(package);
                    submods++;
                }

                // Load tuning pack
                if (package.TuningGUID != ModGuid.BaseGame) {
                    __mods[package.TuningGUID]
                        = package.TuningGUID.GUID is "142b113740474c82a60b0a428bd553d5" ? new BattlegroundsTuning(package) : new CustomTuning(package);
                    submods++;
                }

                // Load wincondition pack
                if (package.GamemodeGUID != ModGuid.BaseGame) {

                    // Create mod
                    IWinconditionMod gamemodePack
                        = package.GamemodeGUID.GUID is "6a0a13b89555402ca75b85dc30f5cb04" ? new BattlegroundsWincondition(package) : new CustomWincondition(package);

                    // Set mod
                    __mods[package.GamemodeGUID]
                        = gamemodePack;

                    // Register mod in wincondition list
                    gamemodePack.Gamemodes.ForEach(WinconditionList.AddWincondition);

                    // Increment submod counter
                    submods++;
                }

                // Log
                Trace.WriteLine($"Loaded mod package '{package.ID}' (With {submods} mods).", nameof(ModManager));

            } catch (Exception ex) {

                // Log error and file
                Trace.WriteLine($"Failed to read mod package '{packageName}'.", nameof(ModManager));
                Trace.WriteLine($"{packageName} Error is --> {ex}", nameof(ModManager));

            }
        }

    }

    /// <summary>
    /// Get package from its <paramref name="packageID"/>.
    /// </summary>
    /// <param name="packageID">The ID to use to identify the <see cref="ModPackage"/>.</param>
    /// <returns>The <see cref="ModPackage"/> associated with <paramref name="packageID"/>.</returns>
    public static ModPackage? GetPackage(string packageID)
        => __packages.FirstOrDefault(x => x.ID == packageID);

    /// <summary>
    /// Iterate over each <see cref="ModPackage"/> in the system.
    /// </summary>
    /// <param name="modPackageAction">The action to invoke with each package element.</param>
    public static void EachPackage(Action<ModPackage> modPackageAction)
        => __packages.ForEach(modPackageAction);

    /// <summary>
    /// Get the abstract <see cref="IGameMod"/> instance represented by <paramref name="guid"/>.
    /// </summary>
    /// <typeparam name="TMod">The specifc <see cref="IGameMod"/> type to get.</typeparam>
    /// <param name="guid">The GUID of the mod to fetch.</param>
    /// <returns>The <see cref="IGameMod"/> instance associated with the <paramref name="guid"/>.</returns>
    public static TMod? GetMod<TMod>(ModGuid guid) where TMod : class, IGameMod
        => __mods[guid] as TMod;

    /// <summary>
    /// Get a <see cref="ModPackage"/> based on one of its submod <paramref name="guid"/> elements.
    /// </summary>
    /// <param name="guid">The <see cref="ModGuid"/> to get <see cref="ModPackage"/> from.</param>
    /// <returns>The <see cref="ModPackage"/> associated with the submod associated <paramref name="guid"/>.</returns>
    public static ModPackage? GetPackageFromGuid(ModGuid guid)
        => __packages.FirstOrDefault(x => x.TuningGUID == guid || x.GamemodeGUID == guid || x.AssetGUID == guid);

}

