using System.IO;

using Battlegrounds.Cli;
using Battlegrounds.Models.Blueprints;
using Battlegrounds.Models.Blueprints.Extensions;
using Battlegrounds.Models.Companies;
using Battlegrounds.Models.Playing;
using Battlegrounds.Serializers;
using Battlegrounds.Services;
using Battlegrounds.Services.Data;

using Microsoft.Extensions.Logging.Abstractions;

// ---------------------------------------------------------------------------
// Bootstrap
// ---------------------------------------------------------------------------

Console.OutputEncoding = System.Text.Encoding.UTF8;

Console.WriteLine("bgc-edit — Company File Editor");
Console.WriteLine("Loading blueprints...");

var localeService = new GameLocaleService(NullLogger<GameLocaleService>.Instance);
await localeService.LoadLocalesAsync();

var blueprintService = new BlueprintService(localeService, NullLogger<BlueprintService>.Instance);
await blueprintService.LoadBlueprints();

if (!blueprintService.IsLoaded) {
    Console.Error.WriteLine("ERROR: Failed to load blueprints. Check that the Assets/ folder is present.");
    return 1;
}

Console.WriteLine("Blueprints loaded.");

// ---------------------------------------------------------------------------
// Open file
// ---------------------------------------------------------------------------

string filePath;
if (args.Length > 0) {
    filePath = args[0];
} else {
    filePath = MenuHelpers.PromptString("Company file path (.bgc)");
}

filePath = Path.GetFullPath(filePath);
if (!File.Exists(filePath)) {
    Console.Error.WriteLine($"ERROR: File not found: {filePath}");
    return 1;
}

Company company;
try {
    var deserializer = new BinaryCompanyDeserializer(blueprintService) { IgnoreUnknownSquads = false };
    using var stream = File.OpenRead(filePath);
    company = deserializer.DeserializeCompany(stream);
} catch (Exception ex) {
    Console.Error.WriteLine($"ERROR: Could not read company file: {ex.Message}");
    return 1;
}

var editor = new CompanyEditor(company);
Console.WriteLine($"Opened: {company.Name}  ({filePath})");

// ---------------------------------------------------------------------------
// REPL loop
// ---------------------------------------------------------------------------

bool running = true;
while (running) {
    PrintMainMenu(editor, filePath);
    Console.Write("  Choice: ");
    string? input = Console.ReadLine()?.Trim();

    switch (input) {
        case "1": ListSquads(editor); break;
        case "2": AddSquad(editor, blueprintService); break;
        case "3": RemoveSquad(editor); break;
        case "4": EditSquad(editor, blueprintService); break;
        case "5": ListCapturedItems(editor); break;
        case "6": AddCapturedItem(editor, blueprintService); break;
        case "7": RemoveCapturedItem(editor); break;
        case "8": SaveCompany(editor, ref filePath); break;
        case "0":
            if (editor.HasUnsavedChanges) {
                if (!MenuHelpers.Confirm("You have unsaved changes. Exit anyway?"))
                    break;
            }
            running = false;
            break;
        default:
            Console.WriteLine("  Unknown option.");
            break;
    }
}

return 0;

// ---------------------------------------------------------------------------
// Menu helpers
// ---------------------------------------------------------------------------

static void PrintMainMenu(CompanyEditor editor, string filePath) {
    Console.WriteLine();
    MenuHelpers.Header($"{editor.CompanyName}  |  {editor.Faction}  |  {editor.GameId}  |  {editor.Squads.Count} squads  |  {editor.CapturedItems.Count} items{(editor.HasUnsavedChanges ? "  [UNSAVED]" : "")}");
    Console.WriteLine("  [1] List squads");
    Console.WriteLine("  [2] Add squad");
    Console.WriteLine("  [3] Remove squad");
    Console.WriteLine("  [4] Edit squad");
    Console.WriteLine("  [5] List captured items");
    Console.WriteLine("  [6] Add captured item");
    Console.WriteLine("  [7] Remove captured item");
    Console.WriteLine("  [8] Save");
    Console.WriteLine("  [0] Exit");
}

static void ListSquads(CompanyEditor editor) {
    var squads = editor.Squads;
    if (squads.Count == 0) {
        Console.WriteLine("  No squads in this company.");
        return;
    }
    MenuHelpers.Separator();
    Console.WriteLine($"  {"ID",4}  {"Blueprint",-35}  {"Display Name",-30}  {"Phase",-14}  {"Exp",7}  {"Rank",4}");
    MenuHelpers.Separator();
    foreach (var squad in squads) {
        string displayName = squad.Blueprint.TryGetExtension<UIExtension>(out var ui)
            ? (string)ui.ScreenName
            : squad.Blueprint.Id;
        string name = squad.HasCustomName ? $"{squad.Name} ({squad.Blueprint.Id})" : squad.Blueprint.Id;
        Console.WriteLine($"  {squad.Id,4}  {name,-35}  {displayName,-30}  {squad.Phase,-14}  {squad.Experience,7:F0}  {squad.Rank,4}");
    }
    MenuHelpers.Separator();
}

static void AddSquad(CompanyEditor editor, IBlueprintService blueprintService) {
    var allBlueprints = blueprintService.GetBlueprintsForGame<CoH3, SquadBlueprint>()
        .Where(bp => bp.Enabled && (string.IsNullOrEmpty(bp.FactionAssociation) || bp.FactionAssociation == editor.Faction))
        .OrderBy(bp => bp.Id)
        .ToList();

    MenuHelpers.Header("Add Squad");

    var selected = MenuHelpers.SelectFromList("Squad Blueprints", allBlueprints,
        bp => {
            string display = bp.TryGetExtension<UIExtension>(out var ui) ? (string)ui.ScreenName : bp.Id;
            return $"{bp.Id,-40} {display}";
        });

    if (selected is null) {
        Console.WriteLine("  Cancelled.");
        return;
    }

    Console.WriteLine("  Phases:");
    Console.WriteLine("    [0] Reserves (default, deploys after 10 min)");
    Console.WriteLine("    [1] Skirmish (first 5 min)");
    Console.WriteLine("    [2] Battle (mid-game, after 5 min)");
    Console.WriteLine("    [3] Starting (deployed immediately)");
    int phaseInput = MenuHelpers.PromptInt("  Phase", 0, 3);
    var phase = (SquadPhase)phaseInput;

    var squad = editor.AddSquad(selected, phase);
    string displayName = selected.TryGetExtension<UIExtension>(out var uiExt) ? (string)uiExt.ScreenName : selected.Id;
    Console.WriteLine($"  Added squad #{squad.Id}: {displayName} ({selected.Id}) — {phase}");
}

static void RemoveSquad(CompanyEditor editor) {
    var squads = editor.Squads;
    if (squads.Count == 0) {
        Console.WriteLine("  No squads to remove.");
        return;
    }

    MenuHelpers.Header("Remove Squad");
    ListSquads(editor);

    int? id = MenuHelpers.PromptIntOptional("  Squad ID to remove (Enter to cancel)");
    if (id is null) {
        Console.WriteLine("  Cancelled.");
        return;
    }

    var target = squads.FirstOrDefault(s => s.Id == id.Value);
    if (target is null) {
        Console.WriteLine($"  No squad with ID {id.Value}.");
        return;
    }

    string displayName = target.Blueprint.TryGetExtension<UIExtension>(out var ui)
        ? (string)ui.ScreenName : target.Blueprint.Id;
    if (!MenuHelpers.Confirm($"  Remove squad #{target.Id} ({displayName})?"))
        return;

    editor.RemoveSquad(id.Value);
    Console.WriteLine($"  Removed squad #{id.Value}.");
}

static void EditSquad(CompanyEditor editor, IBlueprintService blueprintService) {
    var squads = editor.Squads;
    if (squads.Count == 0) {
        Console.WriteLine("  No squads to edit.");
        return;
    }

    MenuHelpers.Header("Edit Squad");
    ListSquads(editor);

    int? id = MenuHelpers.PromptIntOptional("  Squad ID to edit (Enter to cancel)");
    if (id is null) {
        Console.WriteLine("  Cancelled.");
        return;
    }

    var squad = squads.FirstOrDefault(s => s.Id == id.Value);
    if (squad is null) {
        Console.WriteLine($"  No squad with ID {id.Value}.");
        return;
    }

    bool editLoop = true;
    while (editLoop) {
        string displayName = squad.Blueprint.TryGetExtension<UIExtension>(out var uiExt)
            ? (string)uiExt.ScreenName : squad.Blueprint.Id;
        Console.WriteLine();
        Console.WriteLine($"  Editing squad #{squad.Id}: {displayName} ({squad.Blueprint.Id})  Exp={squad.Experience:F0}  Rank={squad.Rank}  Phase={squad.Phase}");
        Console.WriteLine("  [1] Rename");
        Console.WriteLine("  [2] Set experience");
        Console.WriteLine("  [3] Change phase");
        Console.WriteLine("  [4] Manage upgrades");
        Console.WriteLine("  [0] Done");
        Console.Write("  Choice: ");
        string? choice = Console.ReadLine()?.Trim();

        switch (choice) {
            case "1": {
                string newName = MenuHelpers.PromptString("  New name", squad.HasCustomName ? squad.Name : null);
                editor.UpdateSquad(squad.Id, name: newName);
                squad = editor.Squads.First(s => s.Id == squad.Id);
                break;
            }
            case "2": {
                float? exp = MenuHelpers.PromptFloatOptional("  Experience (Enter to cancel)", min: 0f);
                if (exp is null) break;
                editor.UpdateSquad(squad.Id, experience: exp.Value);
                squad = editor.Squads.First(s => s.Id == squad.Id);
                Console.WriteLine($"  Experience set to {exp.Value:F0}. New rank: {squad.Rank}");
                break;
            }
            case "3": {
                Console.WriteLine("    [0] Reserves  [1] Skirmish  [2] Battle  [3] Starting");
                int phaseInput = MenuHelpers.PromptInt("  New phase", 0, 3);
                editor.UpdateSquad(squad.Id, phase: (SquadPhase)phaseInput);
                squad = editor.Squads.First(s => s.Id == squad.Id);
                break;
            }
            case "4":
                ManageUpgrades(editor, blueprintService, squad);
                squad = editor.Squads.First(s => s.Id == squad.Id);
                break;
            case "0":
                editLoop = false;
                break;
            default:
                Console.WriteLine("  Unknown option.");
                break;
        }
    }
}

static void ManageUpgrades(CompanyEditor editor, IBlueprintService blueprintService, Squad squad) {
    bool loop = true;
    while (loop) {
        var available = squad.Blueprint.Upgrades.Available;
        var currentUpgrades = squad.Upgrades.ToList();

        Console.WriteLine();
        Console.WriteLine($"  Current upgrades ({currentUpgrades.Count}):");
        if (currentUpgrades.Count == 0)
            Console.WriteLine("    (none)");
        for (int i = 0; i < currentUpgrades.Count; i++) {
            string name = currentUpgrades[i].TryGetExtension<UIExtension>(out var ui)
                ? (string)ui.ScreenName : currentUpgrades[i].Id;
            Console.WriteLine($"    [{i + 1}] {currentUpgrades[i].Id,-40} {name}");
        }
        Console.WriteLine("  [A] Add upgrade  [R] Remove upgrade  [0] Back");
        Console.Write("  Choice: ");
        string? choice = Console.ReadLine()?.Trim();

        if (choice?.Equals("0") == true) { loop = false; break; }
        if (choice?.Equals("A", StringComparison.OrdinalIgnoreCase) == true) {
            // Show only upgrades that are in the blueprint's available list and not already equipped
            var addable = available
                .Select(id => blueprintService.TryGetBlueprint<CoH3, UpgradeBlueprint>(id, out var bp) ? bp : null)
                .Where(bp => bp is not null && currentUpgrades.All(u => u.Id != bp.Id))
                .Cast<UpgradeBlueprint>()
                .ToList();

            if (addable.Count == 0) {
                Console.WriteLine("  No more upgrades available.");
                continue;
            }

            var selected = MenuHelpers.SelectFromList("Available Upgrades", addable,
                bp => {
                    string display = bp.TryGetExtension<UIExtension>(out var ui) ? (string)ui.ScreenName : bp.Id;
                    return $"{bp.Id,-40} {display}";
                });

            if (selected is null) continue;
            currentUpgrades.Add(selected);
            editor.UpdateSquad(squad.Id, upgrades: currentUpgrades);
            squad = editor.Squads.First(s => s.Id == squad.Id);
            Console.WriteLine($"  Added upgrade: {selected.Id}");
        } else if (choice?.Equals("R", StringComparison.OrdinalIgnoreCase) == true) {
            if (currentUpgrades.Count == 0) { Console.WriteLine("  Nothing to remove."); continue; }
            int? idx = MenuHelpers.PromptIntOptional("  Number to remove (Enter to cancel)", 1, currentUpgrades.Count);
            if (idx is null) continue;
            var removed = currentUpgrades[idx.Value - 1];
            currentUpgrades.RemoveAt(idx.Value - 1);
            editor.UpdateSquad(squad.Id, upgrades: currentUpgrades);
            squad = editor.Squads.First(s => s.Id == squad.Id);
            Console.WriteLine($"  Removed: {removed.Id}");
        } else {
            Console.WriteLine("  Unknown option.");
        }
    }
}

static void ListCapturedItems(CompanyEditor editor) {
    var items = editor.CapturedItems;
    if (items.Count == 0) {
        Console.WriteLine("  No captured items.");
        return;
    }
    MenuHelpers.Separator();
    Console.WriteLine($"  {"ID",4}  {"Blueprint",-40}  {"Captured By",-12}  {"Captured At"}");
    MenuHelpers.Separator();
    foreach (var item in items) {
        string bpId = item.ItemBlueprint?.Id ?? "(none)";
        string capturedBy = item.CapturedBySquadId < 0 ? "unknown" : $"squad #{item.CapturedBySquadId}";
        Console.WriteLine($"  {item.Id,4}  {bpId,-40}  {capturedBy,-12}  {item.CapturedAt:u}");
    }
    MenuHelpers.Separator();
}

static void AddCapturedItem(CompanyEditor editor, IBlueprintService blueprintService) {
    var allEntities = blueprintService.GetBlueprintsForGame<CoH3, EntityBlueprint>()
        .OrderBy(bp => bp.Id)
        .ToList();

    MenuHelpers.Header("Add Captured Item");

    var selected = MenuHelpers.SelectFromList("Entity Blueprints", allEntities,
        bp => bp.Id);

    if (selected is null) {
        Console.WriteLine("  Cancelled.");
        return;
    }

    int? capturedBySquadId = MenuHelpers.PromptIntOptional(
        "  Captured by squad ID (-1 or Enter for unknown)");
    int squadId = capturedBySquadId ?? -1;
    if (squadId >= 0 && editor.Squads.All(s => s.Id != squadId)) {
        Console.WriteLine($"  Warning: no squad with ID {squadId} exists in this company.");
    }

    var item = editor.AddCapturedItem(selected, squadId);
    Console.WriteLine($"  Added captured item #{item.Id}: {selected.Id}");
}

static void RemoveCapturedItem(CompanyEditor editor) {
    var items = editor.CapturedItems;
    if (items.Count == 0) {
        Console.WriteLine("  No captured items to remove.");
        return;
    }

    MenuHelpers.Header("Remove Captured Item");
    ListCapturedItems(editor);

    int? id = MenuHelpers.PromptIntOptional("  Item ID to remove (Enter to cancel)");
    if (id is null) {
        Console.WriteLine("  Cancelled.");
        return;
    }

    var target = items.FirstOrDefault(c => c.Id == id.Value);
    if (target is null) {
        Console.WriteLine($"  No item with ID {id.Value}.");
        return;
    }

    if (!MenuHelpers.Confirm($"  Remove captured item #{id.Value} ({target.ItemBlueprint?.Id ?? "(none)"})?"))
        return;

    editor.RemoveCapturedItem(id.Value);
    Console.WriteLine($"  Removed captured item #{id.Value}.");
}

static void SaveCompany(CompanyEditor editor, ref string filePath) {
    string savePath = MenuHelpers.PromptString("  Save to", filePath);

    string updatedBy = MenuHelpers.PromptString("  Updated by (username/ID)", editor.LastSavedBy);

    try {
        var company = editor.BuildCompany(updatedBy);
        var serializer = new BinaryCompanySerializer();
        using var ms = new MemoryStream();
        serializer.SerializeCompany(ms, company);
        File.WriteAllBytes(savePath, ms.ToArray());
        filePath = Path.GetFullPath(savePath);
        editor.LastSavedBy = updatedBy;
        Console.WriteLine($"  Saved to: {filePath}  (version {company.Version})");
    } catch (Exception ex) {
        Console.Error.WriteLine($"  ERROR saving: {ex.Message}");
    }
}
