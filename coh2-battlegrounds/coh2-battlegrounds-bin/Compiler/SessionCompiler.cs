using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Battlegrounds.Game.Battlegrounds;
using Battlegrounds.Game.Database;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Functional;
using Battlegrounds.Modding;
using Battlegrounds.Util;

namespace Battlegrounds.Compiler {

    /// <summary>
    /// Basic <see cref="Session"/> to Lua code compiler. Can be inherited to add custom features.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="CompanyCompiler"/> to use to compile a <see cref="Company"/> to Lua code.</typeparam>
    public class SessionCompiler<T> where T : CompanyCompiler {

        /// <summary>
        /// Create a new <see cref="SessionCompiler{T}"/> instance.
        /// </summary>
        public SessionCompiler() {}

        /// <summary>
        /// Compile a <see cref="Session"/> into Lua Source Code.
        /// </summary>
        /// <param name="session">The <see cref="Session"/> instance to compile.</param>
        /// <returns>A formatted string containing Lua Source Code.</returns>
        public virtual string CompileSession(Session session) {

            // Create the compiler instance
            CompanyCompiler compiler = Activator.CreateInstance<T>();

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

            // Write the precompiled database
            WritePrecompiledDatabase(lua, session.Participants.Select(x => x.ParticipantCompany));

            lua.AppendLine($"bg_companies = {{");
            lua.IncreaseIndent();

            foreach (SessionParticipant participant in session.Participants) {
                lua.AppendLine(compiler.CompileToLua(participant.ParticipantCompany, !participant.IsHumanParticipant, participant.PlayerIndexOnTeam));
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

            lua.AppendLine($"[\"{team}\"] = {{");
            lua.IncreaseIndent();

            foreach (SessionParticipant player in players) {
                lua.AppendLine($"{{ display_name = \"{player.GetName()}\", ai_value = {(byte)player.Difficulty}, id = {player.PlayerIndexOnTeam}, }},");
            }

            lua.DecreaseIndent();
            lua.AppendLine("},");

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lua"></param>
        /// <param name="companies"></param>
        protected virtual void WritePrecompiledDatabase(TxtBuilder lua, IEnumerable<Company> companies) {

            // Write database table
            lua.AppendLine("bg_db = {");
            lua.IncreaseIndent();

            // Write slot items table
            lua.AppendLine("slot_items = {");
            lua.IncreaseIndent();

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

                lua.AppendLine($"{{ ibp = \"{ibp.Name}\", ignore_if = {subIfTable}, icon = \"{ibp.Icon}\", }},");

            }

            lua.DecreaseIndent();
            lua.AppendLine("},");

            lua.DecreaseIndent();
            lua.AppendLine("};\n");

        }

    }

}
