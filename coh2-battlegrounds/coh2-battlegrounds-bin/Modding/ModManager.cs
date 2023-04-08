using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Collections.Generic;

using Battlegrounds.Functional;
using Battlegrounds.Logging;
using Battlegrounds.Modding.Battlegrounds;
using Battlegrounds.Modding.Vanilla;
using Battlegrounds.Game;

namespace Battlegrounds.Modding;

/// <summary>
/// Manager class for managing <see cref="IModPackage"/> instances. Implements <see cref="IModManager"/> and is the default implementation for <see cref="BattlegroundsContext"/>.
/// </summary>
public sealed class ModManager : IModManager {

    private static readonly Logger logger = Logger.CreateLogger();

    private readonly List<IModPackage> __packages;
    private readonly Dictionary<ModGuid, IGameMod> __mods;
    private readonly Dictionary<string, IModFactory> __modFactories;

    private readonly VanillaModPackage _coh2;
    private readonly VanillaModPackage _coh3;

    /// <summary>
    /// Initialise a new <see cref="ModManager"/> instance.
    /// </summary>
    public ModManager() {
        this.__packages = new();
        this.__mods = new();
        this.__modFactories = new();

        this._coh2 = new VanillaModPackage() {
            ID = "vcoh2",
            PackageName = "Company of Heroes 2",
            SupportedGames = GameCase.CompanyOfHeroes2,
            LocaleFiles = new ModLocale[] {
                new ModLocale("Engine", "VCoH2", "CompanyOfHeroes2")
            }
        };

        this._coh3 = new VanillaModPackage() {
            ID = "vcoh3",
            PackageName = "Company of Heroes 3",
            SupportedGames = GameCase.CompanyOfHeroes3,
            LocaleFiles = new ModLocale[] {
                new ModLocale("Engine", "VCoH3", "CompanyOfHeroes3")
            }
        };

        this.__packages.Add(_coh2);
        this.__packages.Add(_coh3);


    }

    /// <inheritdoc/>
    public void LoadMods() {

        // Get package files
        string[] packageFiles = Directory.GetFiles(BattlegroundsContext.GetRelativePath(BattlegroundsPaths.MOD_USER_FOLDER), "*.package.json")
            .Append(BattlegroundsContext.GetRelativePath(BattlegroundsPaths.BINARY_FOLDER, "battlegrounds.mod.package.json"));

        // TODO: Add support for plugins such that plugins handle mod creaton themselves.
        // If no plugin, we do completely default behaviour here.

        // Load all packages
        foreach (string packageFilepath in packageFiles) {
            string packageName = Path.GetFileNameWithoutExtension(packageFilepath);
            try {

                // Read the mod package
                IModPackage? package = JsonSerializer.Deserialize<ModPackage>(File.OpenRead(packageFilepath));
                if (package is null) {
                    logger.Error($"Failed to load mod package '{packageFilepath}' (Error reading file).");
                    continue;
                }

                // Ensure no duplicate
                if (__packages.Any(x => x.ID == package.ID)) {
                    logger.Error($"Failed to load mod package '{package.ID}' (Duplicate ID entry).");
                    continue;
                }

                // Add mod package
                __packages.Add(package);

                // Get mod factory
                var modFactory = GetModFactory(package);

                // Submod counter
                int submods = 0;

                // Load asset pack
                if (package.AssetGUID != ModGuid.BaseGame) {
                    __mods[package.AssetGUID] = new CustomAsset(package);
                    submods++;
                }

                // Load tuning pack
                if (package.TuningGUID != ModGuid.BaseGame) {
                    __mods[package.TuningGUID] = modFactory.GetTuning();
                    submods++;
                }

                // Load wincondition pack
                if (package.GamemodeGUID != ModGuid.BaseGame) {

                    // Create mod
                    IWinconditionMod gamemodePack = modFactory.GetWinconditionMod();

                    // Set mod
                    __mods[package.GamemodeGUID]
                        = gamemodePack;

                    // Increment submod counter
                    submods++;

                }

                // Log
                logger.Info($"Loaded mod package '{package.ID}' (With {submods} mods).");

            } catch (Exception ex) {

                // Log error and file
                logger.Error($"Failed to read mod package '{packageName}'.");
                logger.Error($"{packageName} Error is: {ex}");

            }
        }

    }

    private IModFactory GetModFactory(IModPackage package) { 
        if (package.ID is "mod_bg") {
            return new BattlegroundsModFactory(package);
        } else if (__modFactories.TryGetValue(package.ID, out IModFactory? factory) && factory is not null) {
            return factory;
        }
        throw new Exception("Mod factory not found - please verify the plugin is installed correctly.");
    }

    /// <inheritdoc/>
    public IModPackage? GetPackage(string packageID)
        => __packages.FirstOrDefault(x => x.ID == packageID);

    /// <inheritdoc/>
    public IModPackage GetPackageOrError(string packageID)
        => __packages.FirstOrDefault(x => x.ID == packageID) ?? throw new Exception($"Package '{packageID}' not found.");

    /// <inheritdoc/>
    public void EachPackage(Action<IModPackage> modPackageAction)
        => __packages.ForEach(modPackageAction);

    /// <inheritdoc/>
    public TMod? GetMod<TMod>(ModGuid guid) where TMod : class, IGameMod
        => __mods.TryGetValue(guid, out IGameMod? mod) ? mod as TMod : null;

    /// <inheritdoc/>
    public IModPackage? GetPackageFromGuid(ModGuid guid)
        => __packages.FirstOrDefault(x => x.TuningGUID == guid || x.GamemodeGUID == guid || x.AssetGUID == guid);

    /// <inheritdoc/>
    public IList<IModPackage> GetPackages() => __packages;

    /// <inheritdoc/>
    public IModPackage GetVanillaPackage(GameCase gameCase) => gameCase switch {
        GameCase.CompanyOfHeroes3 => _coh3,
        GameCase.CompanyOfHeroes2 => _coh2,
        _ => throw new Exception("invalid game case")
    };

}
