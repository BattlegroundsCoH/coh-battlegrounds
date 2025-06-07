using System.Diagnostics.CodeAnalysis;

using Battlegrounds.Models;
using Battlegrounds.Models.Blueprints;
using Battlegrounds.Models.Blueprints.Extensions;
using Battlegrounds.Models.Playing;

namespace Battlegrounds.Services;

public sealed class BlueprintService(IGameLocaleService localeService) : IBlueprintService {

    private class BlueprintRepository {

        private readonly Dictionary<string, Dictionary<string, Blueprint>> _blueprints = [];

        public Dictionary<string, Dictionary<string, Blueprint>> Blueprints {
            get => _blueprints;
            init => _blueprints = value;
        }

        public T GetBlueprint<T>(string blueprintId) where T : Blueprint {
            string blueprintType = typeof(T).Name;
            if (_blueprints.TryGetValue(blueprintType, out var blueprints)) {
                if (blueprints.TryGetValue(blueprintId, out var blueprint)) {
                    return (T)blueprint;
                }
            }
            throw new KeyNotFoundException($"Blueprint of type {blueprintType} with ID {blueprintId} not found.");
        }

        public bool TryGetBlueprint<T>(string blueprintId, [NotNullWhen(true)] out T? blueprint) where T : Blueprint {
            string blueprintType = typeof(T).Name;
            if (_blueprints.TryGetValue(blueprintType, out var blueprints)) {
                if (blueprints.TryGetValue(blueprintId, out var foundBlueprint)) {
                    blueprint = (T)foundBlueprint;
                    return true;
                }
            }
            blueprint = null;
            return false;
        }

    }

    private readonly IGameLocaleService _localeService = localeService;
    private readonly Dictionary<string, BlueprintRepository> _gameBlueprintRepositories = [];

    private bool isLoaded = false;

    public bool IsLoaded => isLoaded;

    public T2 GetBlueprint<T1, T2>(string blueprintId)
        where T1 : Game
        where T2 : Blueprint {
        return GetBlueprintRepository<T1>().GetBlueprint<T2>(blueprintId);
    }

    public T GetBlueprint<T>(string gameId, string blueprintId) where T : Blueprint {
        if (_gameBlueprintRepositories.TryGetValue(gameId, out var repository)) {
            return repository.GetBlueprint<T>(blueprintId);
        }
        throw new KeyNotFoundException($"Blueprint repository for game {gameId} not found.");
    }

    public bool TryGetBlueprint<T>(string gameId, string blueprintId, [NotNullWhen(true)] out T? blueprint) where T : Blueprint {
        if (_gameBlueprintRepositories.TryGetValue(gameId, out var repository)) {
            if (repository.TryGetBlueprint<T>(blueprintId, out var foundBlueprint)) {
                blueprint = foundBlueprint;
                return true;
            }
        }
        blueprint = null;
        return false;
    }

    public bool TryGetBlueprint<T1, T2>(string blueprintId, [NotNullWhen(true)] out T2? blueprint)
        where T1 : Game
        where T2 : Blueprint {
        if (_gameBlueprintRepositories.TryGetValue(GetGameId<T1>(), out var repository)) {
            return repository.TryGetBlueprint(blueprintId, out blueprint);
        }
        blueprint = null;
        return false;
    }

    private BlueprintRepository GetBlueprintRepository<T1>()
        where T1 : Game {
        var gameId = GetGameId<T1>();
        if (!_gameBlueprintRepositories.TryGetValue(gameId, out var repository)) {
            throw new KeyNotFoundException($"Blueprint repository for game {gameId} not found.");
        }
        return repository;
    }

    public async void LoadBlueprints() {

        await Task.Delay(50); // Simulate loading time

        var COH3STR = _localeService.FromGame<CoH3>; // For mock use until proper blueprint loading is implemented

        // Load blueprints for CoH3
        var coh3BpRepository = new BlueprintRepository() {
            Blueprints = new Dictionary<string, Dictionary<string, Blueprint>>() {
                { nameof(SquadBlueprint), new List<Blueprint>() {
                    new SquadBlueprint("tommy_uk", SquadCategory.Infantry, new HashSet<BlueprintExtension>([
                        new CostExtension(300, 0, 0),
                        new UIExtension(COH3STR(11153223), COH3STR(11265170), COH3STR(11266227), "tommy_uk", "", "tommy_uk_portrait"),
                        new UpgradesExtension(1, ["recon_package_tommy_uk", "boys_anti_tank_rifles_tommy_uk", "lmg_bren_tommy_uk"]),
                        new VeterancyExtension([
                            new VeterancyExtension.VeterancyRank(900, "Vet1"),
                            new VeterancyExtension.VeterancyRank(2700, "Vet2"),
                            new VeterancyExtension.VeterancyRank(5400, "Vet3")
                            ])
                        ])) { FactionAssociation = "british_africa", IsInfantry = true },
                    new SquadBlueprint("cwt_15_truck_uk", SquadCategory.Support, new HashSet<BlueprintExtension>([
                        new CostExtension(200, 0, 20),
                        new UIExtension((LocaleString)"CWT 15 Truck", LocaleString.Empty, LocaleString.Empty, "cwt_15_uk", "cwt_truck", ""),
                        new VeterancyExtension([
                            new VeterancyExtension.VeterancyRank(1000, "Vet1"),
                            new VeterancyExtension.VeterancyRank(3000, "Vet2"),
                            new VeterancyExtension.VeterancyRank(6000, "Vet3")
                            ]),
                        new HoldExtension(),
                        ])) { FactionAssociation = "british_africa", IsInfantry = false },
                    new SquadBlueprint("matilda_uk", SquadCategory.Armour, new HashSet<BlueprintExtension>([
                        new CostExtension(360, 0, 90),
                        new UIExtension((LocaleString)"Matilda Tank", LocaleString.Empty, LocaleString.Empty, "matilda_africa_uk", "", ""),
                        new VeterancyExtension([
                            new VeterancyExtension.VeterancyRank(1800, "Vet1"),
                            new VeterancyExtension.VeterancyRank(5400, "Vet2"),
                            new VeterancyExtension.VeterancyRank(10800, "Vet3")
                            ])
                        ])) { FactionAssociation = "british_africa", IsInfantry = false },
                    new SquadBlueprint("crusader_uk", SquadCategory.Armour, new HashSet<BlueprintExtension>([
                        new CostExtension(300, 0, 60),
                        new UIExtension((LocaleString)"Crusader Tank", LocaleString.Empty, LocaleString.Empty, "crusader_uk", "", ""),
                        new VeterancyExtension([
                            new VeterancyExtension.VeterancyRank(1800, "Vet1"),
                            new VeterancyExtension.VeterancyRank(5400, "Vet2"),
                            new VeterancyExtension.VeterancyRank(10800, "Vet3")
                            ])
                        ])) { FactionAssociation = "british_africa", IsInfantry = false },
                    new SquadBlueprint("panzergrenadier_ak", SquadCategory.Infantry, new HashSet<BlueprintExtension>([
                        new CostExtension(300, 0, 0),
                        new UIExtension((LocaleString)"Panzergrenadiers", LocaleString.Empty, LocaleString.Empty, "", "", ""),
                        new VeterancyExtension([
                            new VeterancyExtension.VeterancyRank(1400, "Vet1"),
                            new VeterancyExtension.VeterancyRank(2800, "Vet2"),
                            new VeterancyExtension.VeterancyRank(4200, "Vet3")
                            ])
                        ])) { FactionAssociation = "afrika_korps", IsInfantry = true },
                    new SquadBlueprint("halftrack_250_ak", SquadCategory.Support, new HashSet<BlueprintExtension>([
                        new CostExtension(260, 0, 15),
                        new UIExtension((LocaleString)"Sdkfz 250 Halftrack", LocaleString.Empty, LocaleString.Empty, "", "", ""),
                        new VeterancyExtension([
                            new VeterancyExtension.VeterancyRank(1400, "Vet1"),
                            new VeterancyExtension.VeterancyRank(2800, "Vet2"),
                            new VeterancyExtension.VeterancyRank(4200, "Vet3")
                            ]),
                        new HoldExtension(),
                        ])) { FactionAssociation = "afrika_korps", IsInfantry = false },
                    new SquadBlueprint("panzer_iii_ak", SquadCategory.Armour, new HashSet<BlueprintExtension>([
                        new CostExtension(360, 0, 80),
                        new UIExtension((LocaleString)"Panzer III", LocaleString.Empty, LocaleString.Empty, "", "", ""),
                        new VeterancyExtension([
                            new VeterancyExtension.VeterancyRank(1400, "Vet1"),
                            new VeterancyExtension.VeterancyRank(2800, "Vet2"),
                            new VeterancyExtension.VeterancyRank(4200, "Vet3")
                            ])
                        ])) { FactionAssociation = "afrika_korps", IsInfantry = false },
                }.ToDictionary(k => k.Id)},
                { nameof(UpgradeBlueprint), new List<Blueprint>() {
                    new UpgradeBlueprint("lmg_bren_tommy_uk", new HashSet<BlueprintExtension>([
                        new CostExtension(0, 90, 0),
                        new UIExtension((LocaleString)"Bren Light Machine Gun Package", LocaleString.Empty, LocaleString.Empty, "icon_upgrade_british_bren", "weapon_bren", "")
                        ])) { FactionAssociation = "british_africa" },
                    new UpgradeBlueprint("boys_anti_tank_rifles_tommy_uk", new HashSet<BlueprintExtension>([
                        new CostExtension(0, 60, 0),
                        new UIExtension((LocaleString)"Boys Anti-Tank Rifles Package", LocaleString.Empty, LocaleString.Empty, "icon_upgrade_boys_at_gun", "weapon_boys_at_gun", "")
                        ])) { FactionAssociation = "british_africa" },
                    new UpgradeBlueprint("recon_package_tommy_uk", new HashSet<BlueprintExtension>([
                        new CostExtension(0, 60, 0),
                        new UIExtension((LocaleString)"Recce Package", LocaleString.Empty, LocaleString.Empty, "scoped_lee_enfield_flare_gun", "sniper", "")
                        ])) { FactionAssociation = "british_africa" },
                    new UpgradeBlueprint("lmg_panzergrenaider_ak", new HashSet<BlueprintExtension>([
                        new CostExtension(0, 90, 0),
                        new UIExtension((LocaleString)"MG-34 Light Machine Gun Package", LocaleString.Empty, LocaleString.Empty, "", "", "")
                        ])) { FactionAssociation = "afrika_korps" },
                }.ToDictionary(k => k.Id)},
                { nameof(SlotItemBlueprint), [] } // No slot items for CoH3, but we need it here to avoid NPEs later
            }
        };
        _gameBlueprintRepositories.Add(CoH3.GameId, coh3BpRepository);


        isLoaded = true;
    }

    public ICollection<Blueprint> GetBlueprintsForGame(string gameId) {
        if (_gameBlueprintRepositories.TryGetValue(gameId, out var repository)) {
            return [.. repository.Blueprints.Values.SelectMany(bp => bp.Values)];
        }
        throw new KeyNotFoundException($"Blueprint repository for game {gameId} not found.");
    }

    public ICollection<Blueprint> GetBlueprintsForGame<T>() where T : Game => GetBlueprintsForGame(GetGameId<T>());

    public ICollection<T2> GetBlueprintsForGame<T1, T2>()
        where T1 : Game
        where T2 : Blueprint => GetBlueprintsForGame<T2>(GetGameId<T1>());

    public ICollection<T> GetBlueprintsForGame<T>(string gameId) where T : Blueprint 
        => [.. GetBlueprintsForGame(gameId).OfType<T>()];

    private static string GetGameId<T>() where T : Game {
        if (typeof(T) == typeof(CoH3)) {
            return CoH3.GameId;
        }
        throw new NotSupportedException($"Game type {typeof(T).Name} is not supported.");
    }

}
