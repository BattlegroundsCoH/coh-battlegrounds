using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Battlegrounds.Functional;
using Battlegrounds.Game.Blueprints;
using Battlegrounds.Game.Blueprints.Converters;
using Battlegrounds.Game.Database.Management.CoH2;
using Battlegrounds.Game.Database.Management.CoH3;
using Battlegrounds.Game.DataSource;
using Battlegrounds.Logging;
using Battlegrounds.Modding;

using RegexMatch = System.Text.RegularExpressions.Match;

namespace Battlegrounds.Game.Database.Management;

/// <summary>
/// Class representing the whole database of a game mod.
/// </summary>
public class ModDatabase : IModDb {

    private static readonly Logger logger = Logger.CreateLogger();

    private readonly IModPackage _modPackage;
    private readonly IModManager _modManager;
    private readonly Regex _modFileRegex;

    private readonly IModBlueprintDatabase? _modCoH2Database;
    private readonly IModBlueprintDatabase? _modCoH3Database;

    private readonly IList<JsonConverter>? _modCoH2Converters;
    private readonly IList<JsonConverter>? _modCoH3Converters;

    private readonly IGamemodeList? _winconditionCoH2List;
    private readonly IGamemodeList? _winconditionCoH3List;

    private readonly IModLocale? _modCoH2Locale;
    private readonly IModLocale? _modCoH3Locale;

    private readonly IScenarioList? _modCoH2Scenarios;
    private readonly IScenarioList? _modCoH3Scenarios;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="modManager"></param>
    /// <param name="package"></param>
    public ModDatabase(IModManager modManager, IModPackage package) {
        this._modPackage = package;
        this._modManager = modManager;
        this._modFileRegex = new Regex($@"{this._modPackage.ID}-(?<type>\w{{3}})-db-(?<game>(coh2|coh3))");

        if (this._modPackage.SupportedGames is GameCase.CompanyOfHeroes2 or GameCase.All) {
            this._modCoH2Database = new CoH2BlueprintDatabase();
            this._modCoH2Converters = new JsonConverter[] {
                new SquadBlueprintConverter(package.TuningGUID, GameCase.CompanyOfHeroes2),
                new EntityBlueprintConverter(package.TuningGUID, GameCase.CompanyOfHeroes2),
                new AbilityBlueprintConverter(package.TuningGUID, GameCase.CompanyOfHeroes2),
                new WeaponBlueprintConverter(package.TuningGUID, GameCase.CompanyOfHeroes2),
                new UpgradeBlueprintConverter(package.TuningGUID, GameCase.CompanyOfHeroes2)
            };
            this._winconditionCoH2List = new CoH2GamemodeList();
            this._modCoH2Locale = new CoH2Locale();
            this._modCoH2Scenarios = new CoH2ScenarioList();
        }

        if (this._modPackage.SupportedGames is GameCase.CompanyOfHeroes3 or GameCase.All) {
            this._modCoH3Database = new CoH3BlueprintDatabase();
            this._modCoH3Converters = new JsonConverter[] {
                new SquadBlueprintConverter(package.TuningGUID, GameCase.CompanyOfHeroes3),
                new EntityBlueprintConverter(package.TuningGUID, GameCase.CompanyOfHeroes3),
                new AbilityBlueprintConverter(package.TuningGUID, GameCase.CompanyOfHeroes3),
                new WeaponBlueprintConverter(package.TuningGUID, GameCase.CompanyOfHeroes3),
                new UpgradeBlueprintConverter(package.TuningGUID, GameCase.CompanyOfHeroes3)
            };
            this._winconditionCoH3List = new CoH3GamemodeList();
            this._modCoH3Locale = new CoH3Locale();
            this._modCoH3Scenarios = new CoH3ScenarioList();
        }

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="databaseSource"></param>
    /// <exception cref="DirectoryNotFoundException"></exception>
    public async Task<(int,int)> LoadBlueprints(string databaseSource) {

        // Bail if database source does not exist
        if (!Directory.Exists(databaseSource)) {
            throw new DirectoryNotFoundException("Failed finding database source folder {databaseSource}");
        }

        // Select json files
        (string, RegexMatch)[] jsonfiles = await Task.Run(() => Directory.GetFiles(databaseSource, "*.json")
            .Map(x => (x, _modFileRegex.Match(Path.GetFileNameWithoutExtension(x)))).Filter(x => x.Item2.Success));

        // Counters
        int successCounter = 0, failCounter = 0;

        // Load
        Stopwatch stopwatch = Stopwatch.StartNew();
        foreach (var databaseFile in jsonfiles) {

            // Determine what kind of blueprint to load
            if (!Enum.TryParse(databaseFile.Item2.Groups["type"].Value.ToUpper(), out BlueprintType databaseType)) { // try and parse to blueprint type
                failCounter++; // we failed, so increment fail counter.
                logger.Warning("Unknown blueprint type '{0}' defined by file '{1}'", databaseFile.Item2.Groups["type"].Value, databaseFile.Item1);
                continue;
            }

            // Determine what kind of game blueprint to load
            GameCase databaseGameType = databaseFile.Item2.Groups["game"].Value.ToLowerInvariant() switch {
                "coh3" => GameCase.CompanyOfHeroes3,
                "coh2" => GameCase.CompanyOfHeroes2,
                _ => GameCase.Unspecified
            };

            // Load database
            if (!await LoadBlueprintDatabaseFile(databaseFile.Item1, databaseType, databaseGameType)) {
                failCounter++;
            } else {
                successCounter++;
            }

        }
        stopwatch.Stop();

        // Log how long it took to load the specified database
        logger.Info("Loaded mod database '{0}' in {1}s", _modPackage.PackageName, stopwatch.Elapsed.TotalSeconds.ToString("0.00", CultureInfo.InvariantCulture));

        // Return
        return (successCounter, failCounter);

    }

    private async Task<bool> LoadBlueprintDatabaseFile(string blueprintFilepath, BlueprintType type, GameCase game) {
        
        try {

            // Get .NET blueprint type
            Type blueprintType = GetManagedTypeFromBlueprintType(type);

            // Open file stream
            using FileStream fs = File.OpenRead(blueprintFilepath);

            // Defube options
            JsonSerializerOptions options = new JsonSerializerOptions();
            (game switch {
                GameCase.CompanyOfHeroes2 => _modCoH2Converters!,
                GameCase.CompanyOfHeroes3 => _modCoH3Converters!,
                _ => throw new ApplicationException("Failed getting correct converter whie loading database file")
            }).ForEach(options.Converters.Add);

            // Deserialise
            object? blueprints = await JsonSerializer.DeserializeAsync(fs, blueprintType.MakeArrayType(), options);
            if (blueprints is not Array blueprintArray) {
                throw new ApplicationException("Failed to deserialise blueprints");
            }

            // Get the database
            var db = game switch {
                GameCase.CompanyOfHeroes2 => _modCoH2Database ?? throw new NotSupportedException($"Mod package '{_modPackage.PackageName}' does not support CoH2 blueprints"),
                GameCase.CompanyOfHeroes3 => _modCoH3Database ?? throw new NotSupportedException($"Mod package '{_modPackage.PackageName}' does not support CoH3 blueprints"),
                _ => throw new ApplicationException("Failed getting game database")
            };

            // Inject into database
            db.AddBlueprints(blueprintArray, type);

            // Log
            logger.Info("Loaded {0} {1} blueprints for mod '{2}'", blueprintArray.Length, type, _modPackage.PackageName);

            // Return OK => loaded
            return true;

        } catch (IOException ioex) {
            logger.Exception(ioex);
        } catch (Exception ex) {
            logger.Error("Failed loading blueprint database '{0}' with error '{1}'", blueprintFilepath, ex.Message);
        }

        return false;

    }

    private static Type GetManagedTypeFromBlueprintType(BlueprintType blueprintType) => blueprintType switch {
        BlueprintType.CBP => typeof(CriticalBlueprint),
        BlueprintType.IBP => typeof(SlotItemBlueprint),
        BlueprintType.ABP => typeof(AbilityBlueprint),
        BlueprintType.UBP => typeof(UpgradeBlueprint),
        BlueprintType.WBP => typeof(WeaponBlueprint),
        BlueprintType.EBP => typeof(EntityBlueprint),
        BlueprintType.SBP => typeof(SquadBlueprint),
        _ => throw new ApplicationException()
    };

    /// <summary>
    /// 
    /// </summary>
    public void LoadLocales() {

        // Determine language to use
        string language = BattlegroundsContext.Localize.Language.ToString();
        if (language is "Default") {
            language = "English";
        }

        // Load
        _modPackage.LocaleFiles.ForEach(locale => {
            try {
                var src = locale.GameCase switch {
                    GameCase.CompanyOfHeroes2 => _modCoH2Locale ?? throw new Exception(),
                    GameCase.CompanyOfHeroes3 => _modCoH3Locale ?? throw new Exception(),
                    _ => throw new ApplicationException()
                };
                if (locale.GetLocale(_modPackage.ID, language) is UcsFile ucs) {
                    src.RegisterUcsFile(locale.ModType switch {
                        ModType.Asset => _modPackage.AssetGUID,
                        ModType.Gamemode => _modPackage.GamemodeGUID,
                        ModType.Tuning => _modPackage.TuningGUID,
                        _ => throw new NotImplementedException()
                    }, ucs);
                }
            } catch (Exception ex) {
                logger.Exception(ex);
            }
        });

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="databaseSource"></param>
    /// <returns></returns>
    public int LoadScenarios(string databaseSource) {

        string rawJsonDb = Path.Combine(databaseSource, _modPackage.PackageName + "-map-db.json");
        if (!File.Exists(rawJsonDb)) {
            return 0;
        }

        return IScenarioList.LoadScenarioDatabaseFile(rawJsonDb).ForEach(x => {
            var failWarning = ()
                => logger.Warning("Failed adding duplicate scenario '{0}' from game pack '{1}' to scenario list for '{2}'.", x.Name, _modPackage.PackageName, x.Game);
            bool isAdded = x.Game switch {
                GameCase.CompanyOfHeroes2 when _modPackage.SupportedGames.HasFlag(GameCase.CompanyOfHeroes2)
                    => _modCoH2Scenarios!.RegisterScenario(x).IfFalse().Then(failWarning).Negate().ToBool(),
                GameCase.CompanyOfHeroes3 when _modPackage.SupportedGames.HasFlag(GameCase.CompanyOfHeroes3)
                    => _modCoH3Scenarios!.RegisterScenario(x).IfFalse().Then(failWarning).Negate().ToBool(),
                _ => true.Then(() => logger.Warning("Game pack '{0}' has invalid scenario '{1}' targetting game '{2}'", _modPackage.PackageName, x.Name, x.Game)).Negate().ToBool(),
            };
        }).ToList().Count;

    }

    /// <summary>
    /// 
    /// </summary>
    public void LoadWinconditions() {

        // Create mod
        IWinconditionMod? gamemodePack = _modManager.GetMod<IWinconditionMod>(_modPackage.GamemodeGUID);
        if (gamemodePack is null) {
            return;
        }

        // Register each gamemode
        foreach (IGamemode gamemode in gamemodePack.Gamemodes) {
            switch (gamemode.SupportedGame) {
                case GameCase.CompanyOfHeroes2 when _modPackage.SupportedGames.HasFlag(GameCase.CompanyOfHeroes2):
                    _winconditionCoH2List!.AddWincondition(gamemode);
                    break;
                case GameCase.CompanyOfHeroes3 when _modPackage.SupportedGames.HasFlag(GameCase.CompanyOfHeroes3):
                    _winconditionCoH3List!.AddWincondition(gamemode);
                    break;
                default:
                    logger.Warning("Game pack '{0}' has gamemode '{1}' with no valid game target specified ({2})", 
                        _modPackage.PackageName, gamemode.Name, gamemode.SupportedGame.ToString());
                    break;
            }
        }

    }

    /// <inheritdoc/>
    public IModBlueprintDatabase GetBlueprints(GameCase game) => GetBlueprintsOrNull(game) ?? throw new ArgumentNullException(nameof(game));

    /// <inheritdoc/>
    public IModBlueprintDatabase? GetBlueprintsOrNull(GameCase game) => game switch {
        GameCase.CompanyOfHeroes2 => _modCoH2Database,
        GameCase.CompanyOfHeroes3 => _modCoH3Database,
        _ => null
    };

    /// <inheritdoc/>
    public IGamemodeList GetGamemodes(GameCase game) => game switch {
        GameCase.CompanyOfHeroes2 => _winconditionCoH2List ?? throw new Exception(),
        GameCase.CompanyOfHeroes3 => _winconditionCoH3List ?? throw new Exception(),
        _ => throw new Exception()
    };

    /// <inheritdoc/>
    public IModLocale GetLocale(GameCase game) => game switch {
        GameCase.CompanyOfHeroes2 => _modCoH2Locale ?? throw new Exception(),
        GameCase.CompanyOfHeroes3 => _modCoH3Locale ?? throw new Exception(),
        _ => throw new Exception()
    };

    /// <inheritdoc/>
    public IScenarioList GetScenarios(GameCase game) => game switch {
        GameCase.CompanyOfHeroes2 => _modCoH2Scenarios ?? throw new Exception(),
        GameCase.CompanyOfHeroes3 => _modCoH3Scenarios ?? throw new Exception(),
        _ => throw new Exception()
    };

}
