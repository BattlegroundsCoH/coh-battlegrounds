using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Battlegrounds.Game.Database;
using Battlegrounds.Game.Database.Extensions;
using Battlegrounds.Game.DataCompany;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Util;

namespace Battlegrounds.Compiler {

    /// <summary>
    /// Standard implementation for compiling a company into Lua format.
    /// </summary>
    public class CompanyCompiler : ICompanyCompiler {

        /// <summary>
        /// New <see cref="CompanyCompiler"/> instance.
        /// </summary>
        public CompanyCompiler() { }

        /// <summary>
        /// Compile a <see cref="Company"/> into a formatted string representing a lua table. Only in-game values are compiled. (Virtual Method).
        /// </summary>
        /// <param name="company">The company file to compile into lua string.</param>
        /// <param name="indexOnTeam">The index of the player on their team. (Should only be used for AI).</param>
        /// <param name="isAIPlayer">Is this an AI player company</param>
        /// <param name="indent">The indentation level to use when compiling lua.</param>
        /// <returns>A formatted (and correct) lua table string containing all <see cref="Company"/> data for in-game use.</returns>
        public virtual string CompileToLua(Company company, bool isAIPlayer, byte indexOnTeam, int indent = 1) {

            TxtBuilder lua = new TxtBuilder();

            if (isAIPlayer) {
                lua.AppendLine($"[\"AIPlayer#{indexOnTeam}\"] = {{");
            } else {
                lua.AppendLine($"[\"{company.Owner}\"] = {{");
            }
            lua.SetIndent(indent);
            lua.IncreaseIndent();

            { // Company data

                // Write the name and style
                lua.AppendLine($"name = \"{company.Name}\",");
                lua.AppendLine($"style = \"{company.Type}\",");
                lua.AppendLine($"army = \"{company.Army.Name}\",");

                // Write special call-in abilities
                lua.AppendLine($"specials = {{");
                lua.IncreaseIndent();

                //// Compile the list
                //this.CompileList(lua, "artillery", company.Abilities.Where(x => x.Category == AbilityCategory.Artillery).ToImmutableArray(), x => x.ToScar());
                //this.CompileList(lua, "air", company.Abilities.Where(x => x.Category == AbilityCategory.AirSupport).ToImmutableArray(), x => x.ToScar());

                lua.DecreaseIndent();
                lua.AppendLine("},");

                //// Compile upgrades
                //this.CompileList(lua, "upgrades", company.Upgrades, x => x.ToScar());
                //this.CompileList(lua, "modifiers", company.Modifiers, x => x.ToScar());

                // Write units table
                lua.AppendLine($"units = {{");
                lua.IncreaseIndent();

                // Sort the units (important for how they're displayed ingame)
                var units = company.Units;
                Array.Sort(units, (a, b) => this.CompareUnit(a, b));

                foreach (Squad squad in units) {
                    this.CompileUnit(lua, squad);
                }

                lua.DecreaseIndent();
                lua.AppendLine("}");

            }

            lua.DecreaseIndent();
            lua.AppendLine("},");

            return lua.GetContent();

        }

        public Dictionary<string, object> CompileToLua(Company company, bool isAIPlayer, byte indexOnTeam) {

            // Grab units
            var units = company.Units;
            Array.Sort(units, (a, b) => this.CompareUnit(a, b));

            // Get unit abilities
            var uabps = company.GetSpecialUnitAbilities();

            // Get artillery
            var artillery = company.Abilities.Where(x => x.Category is AbilityCategory.Artillery)
                .Union(uabps.Where(x => x.Category is AbilityCategory.Artillery)).ToArray();

            // Get air
            var air = company.Abilities.Where(x => x.Category is AbilityCategory.AirSupport)
                .Union(uabps.Where(x => x.Category is AbilityCategory.AirSupport)).ToArray();

            // Create result
            Dictionary<string, object> result = new() {
                ["name"] = company.Name,
                ["style"] = company.Type,
                ["army"] = company.Army.Name,
                ["specials"] = new Dictionary<string, object>() {
                    ["artillery"] = artillery,
                    ["air"] = air,
                },
                ["upgrades"] = company.Upgrades.Select(x => x.GetScarName()),
                ["modifiers"] = company.Modifiers,
                ["units"] = units
            };

            // Return result
            return result;

        }

        /// <summary>
        /// Company a specific company unit into a formatted lua table string.
        /// </summary>
        /// <param name="builder">The text builder to write contents to.</param>
        /// <param name="squad">The <see cref="Squad"/> to compile.</param>
        public virtual void CompileUnit(TxtBuilder builder, Squad squad) {

            // Open table entry
            builder.AppendLine("{");
            builder.IncreaseIndent();

            // Write the basics of the units
            //builder.AppendLine($"bp_name = {squad.SBP.ToScar()},");
            builder.AppendLine($"company_id = {squad.SquadID},");
            builder.AppendLine($"symbol = \"{squad.SBP.UI.Symbol}\",");
            builder.AppendLine($"category = \"{squad.GetCategory(true)}\",");
            builder.AppendLine($"phase = {(int)(squad.DeploymentPhase - 1)},");
            builder.AppendLine($"veterancy_rank = {squad.VeterancyRank},");
            builder.AppendLine($"veterancy_progress = {squad.VeterancyProgress:0.00},");

            // If heavy armour or heavy artillery
            if (squad.SBP.Types.IsHeavyArmour || squad.SBP.Types.IsHeavyArtillery) {
                builder.AppendLine("heavy = true,");
            }

            // If command
            if (squad.SBP.Types.IsCommandUnit) {
                builder.AppendLine("command = true,");
            }

            // If specialized infantry
            if (squad.SBP.Types.IsSniper || squad.SBP.Types.IsSpecialInfantry) {
                builder.AppendLine("special = true,");
            }

            // Write the support blueprint - if any
            if (squad.SupportBlueprint != null) {
                builder.AppendLine("transport = {");
                builder.IncreaseIndent();
                //builder.AppendLine($"sbp = {squad.SupportBlueprint.ToScar()},");
                builder.AppendLine($"symbol = \"{(squad.SupportBlueprint as SquadBlueprint).UI.Symbol}\",");
                builder.AppendLine($"mode = {(int)squad.DeploymentMethod},");
                if (squad.DeploymentMethod != DeploymentMethod.None && (squad.SBP.Types.IsHeavyArtillery || (!squad.SBP.Types.IsHeavyArtillery && squad.SBP.Types.IsAntiTank))) {
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

                //CompileList(builder, "upgrades", crew.Upgrades, x => { UpgradeBlueprint y = x as UpgradeBlueprint; return y.ToScar(); });
                //CompileList(builder, "slot_items", crew.SlotItems, x => { SlotItemBlueprint y = x as SlotItemBlueprint; return y.ToScar(); });
                CompileList(builder, "modifiers", (Array.Empty<string>()).ToImmutableHashSet(), x => x);

                builder.DecreaseIndent();
                builder.AppendLine("},");
            }

            // Get the squad cost
            CostExtension fCost = squad.GetCost();

            builder.AppendLine($"cost = {{");
            builder.IncreaseIndent();
            builder.AppendLine($"manpower = {(int)Math.Ceiling(fCost.Manpower)},");
            if (squad.SBP.Cost.Munitions > 0) {
                builder.AppendLine($"munitions = {(int)Math.Ceiling(fCost.Munitions)},");
            } else if (squad.SBP.Cost.Fuel > 0) {
                builder.AppendLine($"fuel = {(int)Math.Ceiling(fCost.Fuel)},");
            }
            builder.AppendLine($"fieldtime = {fCost.FieldTime:0.00},");
            builder.DecreaseIndent();
            builder.AppendLine("},");

            //CompileList(builder, "upgrades", squad.Upgrades, x => { UpgradeBlueprint y = x as UpgradeBlueprint; return $"{{ bp={y.ToScar()}, symbol=\"{y.UI.Icon}\" }}"; });
            //CompileList(builder, "slot_items", squad.SlotItems, x => { SlotItemBlueprint y = x as SlotItemBlueprint; return $"{y.ToScar()}"; });
            //CompileList(builder, "modifiers", squad.Modifiers, x => x.ToScar());

            builder.AppendLine($"spawned = false,");

            builder.DecreaseIndent();
            builder.AppendLine("},");

        }

        protected virtual void CompileList<T>(TxtBuilder builder, string table, IReadOnlyCollection<T> source, Func<T, string> func) {
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

        protected virtual int CompareUnit(Squad lhs, Squad rhs) {
            string catlhs = lhs.GetCategory(true);
            string catrhs = rhs.GetCategory(true);
            int cilhs = catlhs.CompareTo("infantry") == 0 ? 0 : (catlhs.CompareTo("team_weapon") == 0 ? 1 : 2);
            int cirhs = catrhs.CompareTo("infantry") == 0 ? 0 : (catrhs.CompareTo("team_weapon") == 0 ? 1 : 2);
            if (cirhs == cilhs) {
                int order = lhs.Blueprint.Name.CompareTo(rhs.Blueprint.Name);
                if (order == 0) {
                    return lhs.VeterancyRank - rhs.VeterancyRank;
                } else {
                    if (lhs.SBP.Types.IsCommandUnit) {
                        return int.MaxValue;
                    } else {
                        return order;
                    }
                }
            } else {
                return cilhs - cirhs;
            }
        }

    }

}
