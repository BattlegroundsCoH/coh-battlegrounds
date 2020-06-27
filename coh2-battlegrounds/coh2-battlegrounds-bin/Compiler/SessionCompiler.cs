using System;
using System.Collections.Generic;
using System.Text;
using Battlegrounds.Game.Battlegrounds;
using Battlegrounds.Util;

namespace Battlegrounds.Compiler {
    
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SessionCompiler<T> where T : CompanyCompiler {

        /// <summary>
        /// 
        /// </summary>
        public SessionCompiler() {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
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
        /// 
        /// </summary>
        /// <param name="lua"></param>
        /// <param name="setting"></param>
        /// <param name="value"></param>
        protected virtual void WriteSetting(TxtBuilder lua, string setting, object value) {

            string strval = value switch
            {
                bool b => (b) ? "true" : "false",
                string s => $"\"{s}\"",
                _ => value.ToString(),
            };

            lua.AppendLine($"{setting} = {strval}");

        }

    }

}
