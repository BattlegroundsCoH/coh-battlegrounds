using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Battlegrounds.Game.Database.Management;
using Battlegrounds.Game.DataCompany;
using Battlegrounds.Game.Database;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Game.Match;
using Battlegrounds.Functional;
using Battlegrounds.Modding;
using Battlegrounds.Lua.Generator;
using Battlegrounds.Game;

namespace Battlegrounds.Compiler {

    /// <summary>
    /// Basic <see cref="Session"/> to Lua code compiler. Can be extended to add support for custom features.
    /// </summary>
    public class SessionCompiler : ISessionCompiler {

        private ICompanyCompiler m_companyCompiler;

        /// <summary>
        /// Create a new <see cref="SessionCompiler{T}"/> instance.
        /// </summary>
        public SessionCompiler() => this.m_companyCompiler = null;

        public virtual string CompileSession(Session session) {

            // Make sure we have a compiler compiler
            if (this.m_companyCompiler is null) {
                throw new ArgumentNullException(nameof(this.m_companyCompiler), "Cannot compile a session without a Company Compiler instance.");
            }

            // Prepare game settings data
            Dictionary<string, object> bg_settings = new() {
                ["playercount"] = session.Participants.Count(x => x.IsHumanParticipant),
                ["session_guid"] = session.SessionID.ToString(),
                ["map"] = Path.GetFileNameWithoutExtension(session.Scenario.RelativeFilename),
                ["tuning_mod"] = new Dictionary<string, object>() {
                    ["mod_name"] = session.TuningMod.Name,
                    ["mod_guid"] = session.TuningMod.Guid.GUID,
                    ["mod_verify_upg"] = session.TuningMod.VerificationUpgrade,
                },
                ["gamemode"] = session.Gamemode?.Name ?? "Victory Points",
                ["gamemodeoptions"] = session.Settings,
                ["team_setup"] = new Dictionary<string, object>() {
                    ["allies"] = this.GetTeam("allies", session.Participants.Where(x => x.ParticipantFaction.IsAllied)),
                    ["axis"] = this.GetTeam("axis", session.Participants.Where(x => x.ParticipantFaction.IsAxis)),
                },
            };

            // Prepare company data
            Dictionary<string, Dictionary<string, object>> bg_companies = session.Participants
                .Select(x => this.GetCompany(x))
                .ToDictionary();

            // Save to lua
            var sourceBuilder = new LuaSourceBuilder()
                .Assignment(nameof(bg_settings), bg_settings)
                .Assignment(nameof(bg_companies), bg_companies)
                .Assignment("bg_db.towing_upgrade", GetBlueprintName(session.TuningMod.Guid, session.TuningMod.TowingUpgrade))
                .Assignment("bg_db.towed_upgrade", GetBlueprintName(session.TuningMod.Guid, session.TuningMod.TowUpgrade));

            // Write the precompiled database
            this.WritePrecompiledDatabase(sourceBuilder, session.Participants.Select(x => x.ParticipantCompany));

            // Return built source code
            return sourceBuilder.GetSourceText();

        }

        private static string GetBlueprintName(ModGuid guid, string blueprint)
            => $"{guid.GUID}:{blueprint}";

        private static string GetCompanyUsername(SessionParticipant participant)
            => participant.IsHumanParticipant ? participant.GetName() : $"AIPlayer#{participant}";

        private KeyValuePair<string, Dictionary<string, object>> GetCompany(SessionParticipant x)
            => new(GetCompanyUsername(x), this.m_companyCompiler.CompileToLua(x.ParticipantCompany, !x.IsHumanParticipant, x.PlayerIndexOnTeam));

        protected virtual List<Dictionary<string, object>> GetTeam(string team, IEnumerable<SessionParticipant> players) {

            List<Dictionary<string, object>> result = new();

            foreach (var player in players) {
                Dictionary<string, object> data = new() {
                    ["display_name"] = player.GetName(),
                    ["ai_value"] = (byte)player.Difficulty,
                    ["id"] = player.PlayerIndexOnTeam,
                };
                if (player.Difficulty is AIDifficulty.Human) {
                    data["steam_index"] = player.GetID();
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

            // List for keeping track of dummy slot items (Slot items granted by an upgrade)
            Dictionary<string, HashSet<string>> upgradeItems = new();

            // Get all potential slot items used in this session
            List<SlotItemBlueprint> slotitems = companies.Aggregate(new List<Squad>(), (a, b) => { a.AddRange(b.Units); return a; })
                .Aggregate(new List<SlotItemBlueprint>(), (a, b) => {
                    a.AddRange(b.SlotItems.Cast<SlotItemBlueprint>());
                    a.AddRange(b.Upgrades.Cast<UpgradeBlueprint>().Aggregate(new List<SlotItemBlueprint>(), (d, e) => {
                        var items = e.SlotItems.Select(x => BlueprintManager.FromBlueprintName(x, BlueprintType.IBP) as SlotItemBlueprint);
                        d.AddRange(items);
                        _ = items.ForEach(x => upgradeItems.IfTrue(y => y.ContainsKey(x.Name))
                            .Then(y => y[x.Name].Add(e.Name))
                            .Else(y => y.Add(x.Name, new HashSet<string>() { e.Name }))
                        );
                        return d;
                    }));
                    return a;
                })
                .Distinct()
                .ToList();

            // Loop through all the items
            foreach (var ibp in slotitems) {

                // Generate data entry
                Dictionary<string, object> ibpData = new() {
                    ["icon"] = ibp.UI.Icon,
                    ["ignore_if"] = upgradeItems.TryGetValue(ibp.Name, out var upgs) ? upgs : Array.Empty<string>()
                };

                // Write DB
                _ = lua.Assignment($"bg_db.slot_items[\"{ibp.GetScarName()}\"]", ibpData);

            }

        }

        public void SetCompanyCompiler(ICompanyCompiler companyCompiler) => this.m_companyCompiler = companyCompiler;

    }

}
