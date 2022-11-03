using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Battlegrounds.Game.Database.Management;
using Battlegrounds.Game.DataCompany;
using Battlegrounds.Game.Database;
using Battlegrounds.Game.Match;
using Battlegrounds.Functional;
using Battlegrounds.Modding;
using Battlegrounds.Game;
using Battlegrounds.Game.Gameplay.Supply;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.AI;
using Battlegrounds.Data.Generators.Lua;

namespace Battlegrounds.Compiler;

/// <summary>
/// Basic <see cref="Session"/> to Lua code compiler. Can be extended to add support for custom features.
/// </summary>
public class SessionCompiler : ISessionCompiler {

    private ICompanyCompiler? m_companyCompiler;

    /// <summary>
    /// Create a new <see cref="SessionCompiler{T}"/> instance.
    /// </summary>
    public SessionCompiler() => this.m_companyCompiler = null;

    public virtual string CompileSession(ISession session) {

        // Make sure we have a compiler compiler
        if (this.m_companyCompiler is null) {
            throw new ArgumentNullException(nameof(this.m_companyCompiler), "Cannot compile a session without a Company Compiler instance.");
        }

        // Get the participants
        var participants = session.GetParticipants();

        // Prepare game settings data
        Dictionary<string, object> bg_settings = new() {
            ["playercount"] = participants.Count(x => x.IsHuman),
            ["session_guid"] = session.SessionID.ToString(),
            ["map"] = Path.GetFileNameWithoutExtension(session.Scenario.RelativeFilename),
            ["tuning_mod"] = new Dictionary<string, object>() {
                ["mod_name"] = session.TuningMod.Name,
                ["mod_guid"] = session.TuningMod.Guid.GUID,
                ["mod_verify_upg"] = $"{session.TuningMod.Guid.GUID}:{session.TuningMod.VerificationUpgrade}",
            },
            ["gamemode"] = session.Gamemode?.Name ?? "Victory Points",
            ["gameoptions"] = session.Settings,
            ["team_setup"] = new Dictionary<string, object>() {
                ["allies"] = this.GetTeam("allies", participants.Where(x => x.TeamIndex is ParticipantTeam.TEAM_ALLIES)),
                ["axis"] = this.GetTeam("axis", participants.Where(x => x.TeamIndex is ParticipantTeam.TEAM_AXIS)),
            },
        };

        // If there's a specific team order, write it out
        switch (session.TeamOrder) {
            case TeamMode.Fixed:
            case TeamMode.FixedReverse:
                bg_settings["roles_swapped"] = session.TeamOrder is TeamMode.FixedReverse;
                break;
            default:
                break;
        }

        // Prepare company data
        Dictionary<string, int> aiCounters = new() { ["allies"] = 0, ["axis"] = 0 };
        Dictionary<string, Dictionary<string, object>> bg_companies = participants
            .Map(x => this.GetCompany(x, session.Names, aiCounters))
            .ToDictionary();

        // Save to lua
        var sourceBuilder = new LuaSourceBuilder()
            .Assignment(nameof(bg_settings), bg_settings)
            .Assignment(nameof(bg_companies), bg_companies)
            .Assignment("bg_db.towing_upgrade", GetBlueprintName(session.TuningMod.Guid, session.TuningMod.TowingUpgrade))
            .Assignment("bg_db.towed_upgrade", GetBlueprintName(session.TuningMod.Guid, session.TuningMod.TowUpgrade));

#if DEBUG
        // Write any debug flags that may have been set
        for (int i = 0; i < BattlegroundsInstance.Debug.ScarFlags.Length; i++) {
            sourceBuilder.Writer.WriteVerbatim(BattlegroundsInstance.Debug.ScarFlags[i]);
            sourceBuilder.Writer.EndLine(true);
        }
#endif

        // If any planning data
        if (session.HasPlanning) {
            sourceBuilder.Assignment("bg_plandata", GetPlanning(session));
        }

        // Write the precompiled database
        this.WritePrecompiledDatabase(sourceBuilder, participants.MapNotNull(x => x.SelectedCompany));

        // Return built source code
        return sourceBuilder.GetSourceText();

    }

    private static string GetBlueprintName(ModGuid guid, string blueprint)
        => $"{guid.GUID}:{blueprint}";

    private static string GetCompanyUsername(ISessionParticipant participant, string team, Dictionary<string, int> aiIndexCount)
        => participant.IsHuman ? participant.GetName() : $"AIPlayer#{team}:{aiIndexCount[team]++}";

    private static string GetTeamString(Faction faction) => faction.IsAllied ? "allies" : "axis";

    private KeyValuePair<string, Dictionary<string, object>> GetCompany(ISessionParticipant x, IList<string> customNames, Dictionary<string, int> aiIndexCount) {
        
        // Error if compiler is not found
        if (this.m_companyCompiler is null)
            throw new Exception("Company compiler instance is undefined!");

        // Error if company is not valid
        if (x.SelectedCompany is null)
            throw new Exception($"Expected company for participant '{x.PlayerIngameIndex}' but found none");
        
        // Return company
        string name = GetCompanyUsername(x, GetTeamString(x.SelectedCompany.Army), aiIndexCount);
        return new(name, this.m_companyCompiler.CompileToLua(x.SelectedCompany, !x.IsHuman, x.PlayerIndexOnTeam, customNames));

    }

    protected virtual List<Dictionary<string, object>> GetTeam(string team, IEnumerable<ISessionParticipant> players) {

        List<Dictionary<string, object>> result = new();

        foreach (var player in players) {
            Dictionary<string, object> data = new() {
                ["display_name"] = player.GetName(),
                ["ai_value"] = (byte)player.Difficulty,
                ["id"] = player.PlayerIndexOnTeam,
                ["uid"] = player.PlayerIngameIndex
            };
            if (player.Difficulty is AIDifficulty.Human) {
                data["steam_index"] = player.GetId().ToString(); // Store as string (Not sure Lua can handle steam ids, 64-bit unsigned integers)
            }
            result.Add(data);
        }

        return result;

    }

    protected virtual Dictionary<string, object> GetPlanning(ISession session) {

        // Create containers
        var entityList = new List<Dictionary<string, object>>();
        var squadList = new List<Dictionary<string, object>>();
        var goalList = new List<Dictionary<string, object>>();

        // Grab data
        var entities = session.GetPlanEntities();
        var squads = session.GetPlanSquads();
        var goals = session.GetPlanGoals();

        // Loop over entities
        for (int i = 0; i < entities.Length; i++) {
            var entity = new Dictionary<string, object>() {
                ["team"] = entities[i].TeamOwner + 1,
                ["player"] = entities[i].TeamMemberOwner,
                ["ebp"] = entities[i].Blueprint.GetScarName(),
                ["pos"] = entities[i].Spawn.SwapYZ(),
                ["mode"] = entities[i].Lookat is null ? "place" : (entities[i].IsDirectional ? "lookat" : "line"),
                ["width"] = entities[i].Width
            };            
            if (entities[i].Lookat is GamePosition lookat) {
                entity["target"] = lookat.SwapYZ();
            }
            entityList.Add(entity);
        }

        // Loop over entities
        for (int i = 0; i < squads.Length; i++) {
            var squad = new Dictionary<string, object>() {
                ["team"] = squads[i].TeamOwner + 1,
                ["player"] = squads[i].TeamMemberOwner,
                ["sid"] = squads[i].SpawnId,
                ["pos"] = squads[i].Spawn.SwapYZ(),
            };
            if (squads[i].Lookat is GamePosition lookat) {
                squad["target"] = lookat.SwapYZ();
            }
            squadList.Add(squad);
        }

        // Loop over goals
        for (int i = 0; i < goals.Length; i++) {
            var goal = new Dictionary<string, object>() {
                ["team"] = goals[i].ObjectiveTeam + 1,
                ["player"] = goals[i].ObjectivePlayer,
                ["order"] = goals[i].ObjectiveIndex,
                ["type"] = goals[i].ObjectiveType,
                ["pos"] = goals[i].ObjectivePosition.SwapYZ()
            };
            goalList.Add(goal);
        }

        // Return plan data
        return new() {
            ["entities"] = entityList,
            ["squads"] = squadList,
            ["objectives"] = goalList
        };

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="lua"></param>
    /// <param name="companies"></param>
    protected virtual void WritePrecompiledDatabase(LuaSourceBuilder lua, IEnumerable<Company> companies) {

        // Get all potential slot items
        var upgrades = companies.SelectMany(x => x.Units.SelectMany(y => y.Upgrades).Cast<UpgradeBlueprint>()).Distinct();
        var itemsInUpgrades = upgrades
            .SelectMany(x => x.SlotItems.Select(y => BlueprintManager.FromBlueprintName<SlotItemBlueprint>(y))).Distinct();
        var items = companies.SelectMany(x => x.Units.SelectMany(y => y.SlotItems.Cast<SlotItemBlueprint>()))
            .Union(itemsInUpgrades).Distinct();
        var upgradeItems = itemsInUpgrades.ToDictionary(k => k, v => upgrades.Where(x => x.SlotItems.Any(y => y == v.Name)).ToHashSet());

        // Loop through all the items
        foreach (var ibp in items) {

            // Generate data entry
            Dictionary<string, object> ibpData = new() {
                ["icon"] = ibp.UI.Icon,
                ["ignore_if"] = upgradeItems.TryGetValue(ibp, out var upgs) ? upgs.Select(x => x.GetScarName()) : Array.Empty<string>()
            };

            // Write DB
            lua.Assignment($"bg_db.slot_items[\"{ibp.GetScarName()}\"]", ibpData);

        }

    }

    public void SetCompanyCompiler(ICompanyCompiler companyCompiler) => this.m_companyCompiler = companyCompiler;
    
    public string CompileSupplyData(ISession session) {

        // Get the participants
        var participants = session.GetParticipants();

        // Get the companies
        var companies = participants.Map(x => x.SelectedCompany).NotNull();

        // Get the sqauds
        var squads = companies.SelectMany(x => x.Units);

        // TODO: Save squad specific upgrades

        // Get blueprints and create generic profiles
        var profiles = squads.Select(x => x.SBP)
            .Distinct()
            .Select(x => new KeyValuePair<string, SupplyProfile>(x.GetScarName(), new(x)))
            .ToDictionary();

        // Save to lua
        var sourceBuilder = new LuaSourceBuilder()
            .Assignment("bgsupplydata_profiles", profiles);

        // Return source code
        return sourceBuilder.GetSourceText();

    }

}
