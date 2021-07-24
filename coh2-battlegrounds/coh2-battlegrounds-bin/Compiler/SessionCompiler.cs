using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Battlegrounds.Game.Database.Management;
using Battlegrounds.Game.DataCompany;
using Battlegrounds.Game.Database;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Game.Match;
using Battlegrounds.Functional;
using Battlegrounds.Modding;
using Battlegrounds.Util;

namespace Battlegrounds.Compiler {

    /// <summary>
    /// Basic <see cref="Session"/> to Lua code compiler. Can be extended to add support for custom features.
    /// </summary>
    public class SessionCompiler : ISessionCompiler {

        private ICompanyCompiler m_companyCompiler;

        /// <summary>
        /// Create a new <see cref="SessionCompiler{T}"/> instance.
        /// </summary>
        public SessionCompiler() {
            this.m_companyCompiler = null;
        }

        public virtual string CompileSession(Session session) {

            // Make sure we have a compiler compiler
            if (this.m_companyCompiler is null) {
                throw new ArgumentNullException(nameof(this.m_companyCompiler), "Cannot compile a session without a Company Compiler instance.");
            }

            // Create txt builder
            TxtBuilder lua = new TxtBuilder();

            lua.AppendLine($"bg_settings = {{");
            lua.IncreaseIndent();

            lua.AppendLine($"playercount = {session.Participants.Count(x => x.IsHumanParticipant)},");
            lua.AppendLine($"session_guid = \"{session.SessionID}\",");
            lua.AppendLine($"map = \"{Path.GetFileNameWithoutExtension(session.Scenario.RelativeFilename)}\",");

            // Write the tuning data
            this.WriteTuningData(lua, session.TuningMod);

            lua.AppendLine($"gamemode = \"{session.Gamemode?.Name ?? "Victory Points"}\",");
            lua.AppendLine($"gameoptions = {{");
            lua.IncreaseIndent();

            foreach (var par in session.Settings) {
                this.WriteSetting(lua, par.Key, par.Value);
            }

            lua.DecreaseIndent();
            lua.AppendLine("},");

            lua.AppendLine($"team_setup = {{");
            lua.IncreaseIndent();

            // Write the teams
            this.WriteTeam(lua, "allies", session.Participants.Where(x => x.ParticipantFaction.IsAllied));
            this.WriteTeam(lua, "axis", session.Participants.Where(x => x.ParticipantFaction.IsAxis));

            lua.DecreaseIndent();
            lua.AppendLine("},");

            lua.DecreaseIndent();
            lua.AppendLine("};\n");

            // If there are tow upgrades - save them
            if (!string.IsNullOrEmpty(session.TuningMod.TowUpgrade) && !string.IsNullOrEmpty(session.TuningMod.TowingUpgrade)) {
                lua.AppendLine($"bg_db.towing_upgrade = \"{session.TuningMod.Guid.ToString().Replace("-", "")}:{session.TuningMod.TowingUpgrade}\"");
                lua.AppendLine($"bg_db.towed_upgrade = \"{session.TuningMod.Guid.ToString().Replace("-", "")}:{session.TuningMod.TowUpgrade}\"");
            }

            // Write the precompiled database
            WritePrecompiledDatabase(lua, session.Participants.Select(x => x.ParticipantCompany));

            lua.AppendLine($"bg_companies = {{");
            lua.IncreaseIndent();

            foreach (SessionParticipant participant in session.Participants) {
                lua.AppendLine(this.m_companyCompiler.CompileToLua(participant.ParticipantCompany, !participant.IsHumanParticipant, participant.PlayerIndexOnTeam, 1));
            }

            lua.DecreaseIndent();
            lua.AppendLine("};");

            return lua.GetContent();

        }

        protected virtual void WriteTuningData(TxtBuilder lua, ITuningMod tuning) {

            lua.AppendLine($"tuning_mod = {{");

            // Only write the tuning data if it exists.
            if (tuning != null) {

                string guid = tuning.Guid.ToString().Replace("-", "");

                lua.IncreaseIndent();
                lua.AppendLine($"mod_name = \"{tuning.Name}\",");
                lua.AppendLine($"mod_guid = \"{guid}\",");
                lua.AppendLine($"mod_verify_upg = \"{guid}:{tuning.VerificationUpgrade}\",");

            }

            lua.DecreaseIndent();
            lua.AppendLine("},");

        }

        /// <summary>
        /// Writes a setting to the <see cref="TxtBuilder"/>.
        /// </summary>
        /// <param name="lua">The lua code to append setting to</param>
        /// <param name="setting">The name of the setting to set</param>
        /// <param name="value">The C# value of the setting to set. The value is automatically converted to its Lua Source Code equivalent</param>
        protected virtual void WriteSetting(TxtBuilder lua, string setting, object value) {

            string strval = value switch
            {
                int i => i.ToString(),
                byte by => by.ToString(),
                double d => d.ToString("0.00"),
                float f => f.ToString("0.00"),
                bool b => b ? "true" : "false",
                string s => $"\"{s}\"",
                _ => value.ToString(),
            };

            lua.AppendLine($"{setting} = {strval},");

        }

        /// <summary>
        /// Writes the team setup for a specific side (axis or allies)
        /// </summary>
        /// <param name="lua">The lua code to append team data to.</param>
        /// <param name="team">The string name of the team.</param>
        /// <param name="players">The players on the team.</param>
        protected virtual void WriteTeam(TxtBuilder lua, string team, IEnumerable<SessionParticipant> players) {

            // Create team table
            lua.AppendLine($"[\"{team}\"] = {{");
            lua.IncreaseIndent();

            // Foreach participant
            foreach (SessionParticipant player in players) {
                StringBuilder blder = new StringBuilder();
                blder.Append($"{{{Environment.NewLine}\t\t\t\t");
                blder.Append($"display_name = \"{player.GetName()}\", ");
                if (player.Difficulty == Game.AIDifficulty.Human) {
                    blder.Append($"steam_index = \"{player.GetID()}\",{Environment.NewLine}\t\t\t\t");
                }
                blder.Append($"ai_value = {(byte)player.Difficulty}, ");
                blder.Append($"id = {player.PlayerIndexOnTeam},{Environment.NewLine}");
                blder.Append($"\t\t\t}},{Environment.NewLine}");
                lua.Append(blder.ToString());
            }

            // End table
            lua.DecreaseIndent();
            lua.AppendLine("},");

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lua"></param>
        /// <param name="companies"></param>
        protected virtual void WritePrecompiledDatabase(TxtBuilder lua, IEnumerable<Company> companies) {

            // List for keeping track of dummy slot items (Slot items granted by an upgrade)
            Dictionary<string, HashSet<string>> upgradeItems = new Dictionary<string, HashSet<string>>();

            // Get all potential slot items used in this session
            List<SlotItemBlueprint> slotitems = companies.Aggregate(new List<Squad>(), (a, b) => { a.AddRange(b.Units); return a; })
                .Aggregate(new List<SlotItemBlueprint>(), (a,b) => { 
                    a.AddRange(b.SlotItems.Cast<SlotItemBlueprint>());
                    a.AddRange(b.Upgrades.Cast<UpgradeBlueprint>().Aggregate(new List<SlotItemBlueprint>(), (d, e) => {
                        var items = e.SlotItems.Select(x => BlueprintManager.FromBlueprintName(x, BlueprintType.IBP) as SlotItemBlueprint);
                        d.AddRange(items);
                        items.ForEach(x => upgradeItems.IfTrue(y => y.ContainsKey(x.Name))
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
            foreach (SlotItemBlueprint ibp in slotitems) {

                string subIfTable;
                if (upgradeItems.TryGetValue(ibp.Name, out HashSet<string> upgs)) {
                    subIfTable = $"{{ {string.Join(", ", upgs.Select(x => $"\"{x}\""))} }}";
                } else {
                    subIfTable = "{}";
                }

                lua.AppendLine($"bg_db.slot_items[\"{ibp.Name}\"] = {{ ignore_if = {subIfTable}, icon = \"{ibp.UI.Icon}\", }};");
                // TODO: Write better format (Just write the IScarSerializable...)
            }

        }

        public void SetCompanyCompiler(ICompanyCompiler companyCompiler) => this.m_companyCompiler = companyCompiler;

    }

}
