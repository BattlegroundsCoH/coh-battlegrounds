using System;
using System.Collections.Immutable;
using Battlegrounds.Game.Battlegrounds;
using Battlegrounds.Game.Database;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Util;

namespace Battlegrounds.Compiler {
    
    /// <summary>
    /// 
    /// </summary>
    public class CompanyCompiler {
    
        /// <summary>
        /// 
        /// </summary>
        public CompanyCompiler() {}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="company"></param>
        /// <param name="indent"></param>
        public virtual string CompileToLua(Company company, bool namedIndex, byte indexOnTeam, int indent = 1) {

            TxtBuilder lua = new TxtBuilder();

            lua.AppendLine($"[\"{company.Owner}{(namedIndex ? ("#" + indexOnTeam.ToString()) : "")}\"] = {{");
            lua.SetIndent(indent);
            lua.IncreaseIndent();

            { // Company data

                lua.AppendLine($"name = \"{company.Name}\",");
                lua.AppendLine($"style = \"{company.Type}\",");
                lua.AppendLine($"army = \"{company.Army.Name}\",");

                lua.AppendLine($"units = {{");
                lua.IncreaseIndent();

                foreach (Squad squad in company.Units) {
                    this.CompileUnit(lua, squad);
                }

                lua.DecreaseIndent();
                lua.AppendLine("}");

            }

            lua.DecreaseIndent();
            lua.AppendLine("},");

            return lua.GetContent();

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="squad"></param>
        public virtual void CompileUnit(TxtBuilder builder, Squad squad) {

            builder.AppendLine("{");
            builder.IncreaseIndent();

            // Write the basics of the units
            builder.AppendLine($"bp_name = {squad.SBP.ToScar()},");
            builder.AppendLine($"company_id = {squad.SquadID},");
            builder.AppendLine($"symbol = \"{squad.SBP.Symbol}\",");
            builder.AppendLine($"category = \"{squad.GetCategory(true)}\",");
            builder.AppendLine($"phase = {squad.DeploymentPhase - 1},");
            builder.AppendLine($"veterancy_rank = {squad.VeterancyRank},");
            builder.AppendLine($"veterancy_progress = {squad.VeterancyProgress:0.00},");

            // If heavy armour or heavy artillery
            if (squad.SBP.IsHeavyArmour || squad.SBP.IsHeavyArtillery) {
                builder.AppendLine("heavy = true,");
            }

            // If command
            if (squad.SBP.IsCommandUnit) {
                builder.AppendLine("command = true,");
            }

            // If specialized infantry
            if (squad.SBP.IsSniper || squad.SBP.IsSpecialInfantry) {
                builder.AppendLine("special = true,");
            }

            // Write the support blueprint - if any
            if (squad.SupportBlueprint != null) {
                builder.AppendLine("transport = {");
                builder.IncreaseIndent();
                builder.AppendLine($"sbp = {squad.SupportBlueprint.ToScar()},");
                builder.AppendLine($"symbol = \"{(squad.SupportBlueprint as SquadBlueprint).Symbol}\",");
                builder.AppendLine($"mode = {(int)squad.DeploymentMethod},");
                if (squad.DeploymentMethod != DeploymentMethod.None && (squad.SBP.IsHeavyArtillery || (!squad.SBP.IsHeavyArtillery && squad.SBP.IsAntiTank))) {
                    builder.AppendLine("tow = true,");
                }
                builder.DecreaseIndent();
                builder.AppendLine("},");
            }

            // Write the crew data - if any
            if (squad.Crew is Squad crew) {
                builder.AppendLine("crew = {");
                builder.IncreaseIndent();
                builder.AppendLine($"company_id = {crew.SquadID},");
                builder.AppendLine($"veterancy_rank = {crew.VeterancyRank},");
                builder.AppendLine($"veterancy_progress = {crew.VeterancyProgress:0.0},");

                CompileList(builder, "upgrades", crew.Upgrades, x => { UpgradeBlueprint y = x as UpgradeBlueprint; return y.ToScar(); });
                CompileList(builder, "slot_items", crew.SlotItems, x => { SlotItemBlueprint y = x as SlotItemBlueprint; return y.ToScar(); });
                CompileList(builder, "modifiers", (new string[0]).ToImmutableHashSet(), x => x);

                builder.DecreaseIndent();
                builder.AppendLine("},");
            }

            // Get the squad cost
            Cost fCost = squad.GetCost();

            builder.AppendLine($"cost = {{");
            builder.IncreaseIndent();
            builder.AppendLine($"manpower = {fCost.Manpower:0.00},");
            if (squad.SBP.Cost.Munitions > 0) {
                builder.AppendLine($"munitions = {fCost.Munitions:0.00},");
            } else if (squad.SBP.Cost.Fuel > 0) {
                builder.AppendLine($"fuel = {fCost.Fuel:0.00},");
            }
            builder.AppendLine($"fieldtime = {fCost.FieldTime:0.00},");
            builder.DecreaseIndent();
            builder.AppendLine("},");

            CompileList(builder, "upgrades", squad.Upgrades, x => { UpgradeBlueprint y = x as UpgradeBlueprint; return $"{{ bp={y.ToScar()}, symbol=\"{y.Symbol}\" }}"; });
            CompileList(builder, "slot_items", squad.SlotItems, x => { SlotItemBlueprint y = x as SlotItemBlueprint; return $"{y.ToScar()}"; });
            CompileList(builder, "modifiers", (new string[0]).ToImmutableHashSet(), x => x);

            builder.AppendLine($"spawned = false,");

            builder.DecreaseIndent();
            builder.AppendLine("},");

        }

        private void CompileList<T>(TxtBuilder builder, string table, ImmutableHashSet<T> source, Func<T, string> func) {
            builder.AppendLine($"{table} = {{");
            builder.IncreaseIndent();
            if (source.Count > 0) {
                builder.Append(""); // Apply indentation here
                foreach (T t in source) {
                    builder.Append($"{func(t)},", false);
                }
                builder.Append("\n", false);
            }
            builder.DecreaseIndent();
            builder.AppendLine("},");
        }

    }

}
