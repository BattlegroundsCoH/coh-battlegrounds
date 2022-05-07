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
using Battlegrounds.Lua.Generator;
using Battlegrounds.Game;
using Battlegrounds.Game.Gameplay.Supply;
using Battlegrounds.Game.Gameplay;

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

        // Write the precompiled database
        this.WritePrecompiledDatabase(sourceBuilder, participants.Map(x => x.SelectedCompany).NotNull());

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
                data["steam_index"] = player.GetID().ToString(); // Store as string (Not sure Lua can handle steam ids, 64-bit unsigned integers)
            }
            result.Add(data);
        }

        return result;

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
            _ = lua.Assignment($"bg_db.slot_items[\"{ibp.GetScarName()}\"]", ibpData);

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
