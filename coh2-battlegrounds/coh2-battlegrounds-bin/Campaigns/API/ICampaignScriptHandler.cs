using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Battlegrounds.Lua;

namespace Battlegrounds.Campaigns.API {
    
    /// <summary>
    /// 
    /// </summary>
    public interface ICampaignScriptHandler {

        /// <summary>
        /// 
        /// </summary>
        LuaState ScriptState { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="globalName"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        object[] CallGlobal(string globalName, params object[] args);

        /// <summary>
        /// Get a global variable and invoke it if it's a function.
        /// </summary>
        /// <param name="globalName"></param>
        /// <param name="args"></param>
        /// <returns>If global is a function returning a boolean, the value of the boolean is returned; Otherwise <see langword="false"/>.</returns>
        bool GetGlobalAndInvoke(string globalName, params object[] args);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="globalName"></param>
        /// <param name="obj"></param>
        /// <param name="isUserType"></param>
        void SetGlobal(string globalName, object obj, bool isUserType);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="scriptContent"></param>
        void LoadScript(string scriptContent);

    }

}
