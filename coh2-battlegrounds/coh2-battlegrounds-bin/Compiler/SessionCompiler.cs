using System;
using Battlegrounds.Game.Battlegrounds;
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

            {

                lua.AppendLine($"playercount = {session.Companies.Length},");
                lua.AppendLine($"guid = \"{session.SessionID}\",");
                lua.AppendLine($"map = \"{session.Scenario}\",");
                lua.AppendLine($"gamemode = \"{session.Gamemode.Name}\",");

                lua.AppendLine($"gameoptions = {{");
                lua.IncreaseIndent();

                foreach (var par in session.Settings) {
                    this.WriteSetting(lua, par.Key, par.Value);
                }

                lua.DecreaseIndent();
                lua.AppendLine("}");

            }

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

    }

}
