using System;
using System.Collections.Generic;
using System.Linq;
using Battlegrounds.Game.Battlegrounds;
using Battlegrounds.Steam;
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

            lua.AppendLine($"playercount = {session.Companies.Length},");
            lua.AppendLine($"session_guid = \"{session.SessionID}\",");
            lua.AppendLine($"map = \"{session.Scenario}\",");
            lua.AppendLine($"tuning_mod = {{");
            lua.IncreaseIndent();
            lua.AppendLine($"mod_name = \"{string.Empty}\",");
            lua.AppendLine($"mod_guid = \"{string.Empty}\",");
            lua.AppendLine($"mod_verify_upg = \"{string.Empty}\",");
            lua.DecreaseIndent();
            lua.AppendLine("},");
            lua.AppendLine($"gamemode = \"{session.Gamemode.Name}\",");
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
            this.WriteTeam(lua, "allies", session.Companies.Where(x => x.Army.IsAllied).Select(x => x.Owner));
            this.WriteTeam(lua, "axis", session.Companies.Where(x => !x.Army.IsAllied).Select(x => x.Owner));

            lua.DecreaseIndent();
            lua.AppendLine("},");

            lua.DecreaseIndent();
            lua.AppendLine("};");

            lua.AppendLine($"bg_companies = {{");
            lua.IncreaseIndent();

            foreach (Company company in session.Companies) {
                lua.AppendLine(compiler.CompileToLua(company));
            }

            lua.DecreaseIndent();
            lua.AppendLine("};");

            return lua.GetContent();

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
                double d => d.ToString("0.00"),
                float f => f.ToString("0.00"),
                bool b => (b) ? "true" : "false",
                string s => $"\"{s}\"",
                _ => value.ToString(),
            };

            lua.AppendLine($"{setting} = {strval}");

        }

        /// <summary>
        /// Writes the team setup for a specific side (axis or allies)
        /// </summary>
        /// <param name="lua">The lua code to append team data to.</param>
        /// <param name="team">The string name of the team.</param>
        /// <param name="players">The players on the team.</param>
        protected virtual void WriteTeam(TxtBuilder lua, string team, IEnumerable<SteamUser> players) {

            lua.AppendLine($"[\"{team}\"] = {{");
            lua.IncreaseIndent();

            foreach (SteamUser user in players) {
                lua.AppendLine($"\"{user.Name}\",");
            }

            lua.DecreaseIndent();
            lua.AppendLine("},");

        }

    }

}
