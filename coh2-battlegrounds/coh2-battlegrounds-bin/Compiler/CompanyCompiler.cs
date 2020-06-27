using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
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
        public virtual string CompileToLua(Company company, int indent = 1) {

            TxtBuilder lua = new TxtBuilder();
            lua.SetIndent(indent);

            lua.AppendLine($"[\"{company.Owner.Name}\"] = {{");
            lua.IncreaseIndent();

            { // Company data

                lua.AppendLine($"name = \"{company.Name}\",");
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

            builder.AppendLine($"bp_name = \"{squad.Blueprint.Name}\",");
            builder.AppendLine($"veterancy_rank = {squad.VeterancyRank},");
            builder.AppendLine($"veterancy_progress = {squad.VeterancyProgress:0.00},");

            CompileList(builder, "upgrades", squad.Upgrades, x => x.Name);
            CompileList(builder, "slot_items", squad.SlotItems, x => x.Name);
            CompileList(builder, "modifiers", (new string[0]).ToImmutableArray(), x => x);

            builder.AppendLine($"spawned = false,");

            builder.DecreaseIndent();
            builder.AppendLine("},");

        }

        private void CompileList<T>(TxtBuilder builder, string table, ImmutableArray<T> source, Func<T, string> func) {
            builder.AppendLine($"{table} = {{");
            builder.IncreaseIndent();
            if (source.Length > 0) {
                builder.Append(""); // Apply indentation here
                foreach (T t in source) {
                    builder.Append($"\"{func(t)}\",", false);
                }
                builder.Append("\n", false);
            }
            builder.DecreaseIndent();
            builder.AppendLine("},");
        }

    }

}
