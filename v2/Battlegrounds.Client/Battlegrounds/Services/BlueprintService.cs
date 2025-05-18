using Battlegrounds.Models.Blueprints;
using Battlegrounds.Models.Blueprints.Extensions;
using Battlegrounds.Models.Playing;

namespace Battlegrounds.Services;

public sealed class BlueprintService : IBlueprintService {
    
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

    }

    private readonly Dictionary<string, BlueprintRepository> _gameBlueprintRepositories = [];

    private bool isLoaded = false;

    public bool IsLoaded => isLoaded;

    public T2 GetBlueprint<T1, T2>(string blueprintId)
        where T1 : Game
        where T2 : Blueprint {
        return GetBlueprintRepository<T1>().GetBlueprint<T2>(blueprintId);
    }

    private BlueprintRepository GetBlueprintRepository<T1>()
        where T1 : Game {
        var gameId = typeof(T1).Name;
        if (!_gameBlueprintRepositories.TryGetValue(gameId, out var repository)) {
            throw new KeyNotFoundException($"Blueprint repository for game {gameId} not found.");
        }
        return repository;
    }

    public async void LoadBlueprints() {

        await Task.Delay(50); // Simulate loading time

        // Load blueprints for CoH3
        var coh3BpRepository = new BlueprintRepository() {
            Blueprints = new Dictionary<string, Dictionary<string, Blueprint>>() {
                { nameof(SquadBlueprint), new List<Blueprint>() {
                    new SquadBlueprint("tommy_uk", SquadCategory.Infantry, new HashSet<BlueprintExtension>([
                        new CostExtension(300, 0, 0),
                        new UIExtension("Infantry Section", "", "", "", "", "")
                        ])),
                    new SquadBlueprint("halftrack_m3_uk", SquadCategory.Support, new HashSet<BlueprintExtension>([
                        new CostExtension(260, 0, 15),
                        new UIExtension("M3 Halftrack", "", "", "", "", "")
                        ])),
                    new SquadBlueprint("matilda_uk", SquadCategory.Armour, new HashSet<BlueprintExtension>([
                        new CostExtension(360, 0, 80),
                        new UIExtension("Matilda Infantry Support Vehicle", "", "", "", "", "")
                        ])),
                    new SquadBlueprint("crusader_uk", SquadCategory.Armour, new HashSet<BlueprintExtension>([
                        new CostExtension(320, 0, 60),
                        new UIExtension("Crusader Tank", "", "", "", "", "")
                        ])),
                    new SquadBlueprint("panzergrenadier_ak", SquadCategory.Infantry, new HashSet<BlueprintExtension>([
                        new CostExtension(300, 0, 0),
                        new UIExtension("Panzergrenadiers", "", "", "", "", "")
                        ])),
                    new SquadBlueprint("hafltrack_250_ak", SquadCategory.Support, new HashSet<BlueprintExtension>([
                        new CostExtension(260, 0, 15),
                        new UIExtension("Sdkfz 250 Halftrack", "", "", "", "", "")
                        ])),
                    new SquadBlueprint("panzer_iii_ak", SquadCategory.Armour, new HashSet<BlueprintExtension>([
                        new CostExtension(360, 0, 80),
                        new UIExtension("Panzer III", "", "", "", "", "")
                        ])),
                }.ToDictionary(k => k.Id)},
                { nameof(UpgradeBlueprint), new List<Blueprint>() {
                    new UpgradeBlueprint("lmg_bren_tommy_uk", new HashSet<BlueprintExtension>([
                        new CostExtension(0, 90, 0),
                        new UIExtension("Bren Light Machine Gun Package", "", "", "", "", "")
                        ])),
                    new UpgradeBlueprint("lmg_panzergrenaider_ak", new HashSet<BlueprintExtension>([
                        new CostExtension(0, 90, 0),
                        new UIExtension("MG-34 Light Machine Gun Package", "", "", "", "", "")
                        ])),
                }.ToDictionary(k => k.Id)}
            }
        };
        _gameBlueprintRepositories.Add(nameof(CoH3), coh3BpRepository);


        isLoaded = true;
    }

}
