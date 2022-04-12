using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Battlegrounds.Campaigns.API;
using Battlegrounds.Lua;
using Battlegrounds.Lua.Debugging;

namespace Battlegrounds.Campaigns.Scripting {

    public class CampaignScriptHandler : ICampaignScriptHandler {

        private LuaState m_lState;

        public LuaState ScriptState => this.m_lState;

        public CampaignScriptHandler() {

            // Create state
            this.m_lState = new LuaState("base", "math", "table", "string");

            // Register userdata
            this.m_lState.RegisterUserdata(typeof(ICampaignMap));
            this.m_lState.RegisterUserdata(typeof(ICampaignMapNode));
            this.m_lState.RegisterUserdata(typeof(ICampaignFormation));
            this.m_lState.RegisterUserdata(typeof(ICampaignTurn));

            // Register constants
            this.m_lState._G["TEAM_AXIS"] = LuaMarshal.ToLuaValue(CampaignArmyTeam.TEAM_AXIS);
            this.m_lState._G["TEAM_ALLIES"] = LuaMarshal.ToLuaValue(CampaignArmyTeam.TEAM_ALLIES);
            this.m_lState._G["TEAM_NEUTRAL"] = LuaMarshal.ToLuaValue(CampaignArmyTeam.TEAM_NEUTRAL);
            this.m_lState._G["FILTER_NEVER"] = double.MaxValue;
            this.m_lState._G["FILTER_OK"] = 1.0;
            this.m_lState._G["ET_Ownership"] = ICampaignEventManager.ET_Ownership;

        }

        private object[] InvokeGlobal(LuaClosure closure, object[] args) {
            try {
                return LuaMarshal.InvokeClosureManaged(closure, this.m_lState, args);
            } catch (LuaRuntimeError lre) {
                Trace.WriteLine($"Fatal lua error :: {lre}", nameof(CampaignScriptHandler));
            }
            return Array.Empty<object>();
        }

        public object[] CallGlobal(string globalName, params object[] args) {
            if (this.m_lState._G[globalName] is LuaClosure closure) {
                return this.InvokeGlobal(closure, args);
            }
            return Array.Empty<object>();
        }

        public bool GetGlobalAndInvoke(string globalName, params object[] args) {
            if (this.m_lState._G[globalName] is LuaValue v) {
                if (v is LuaClosure closure) {
                    object[] result = this.InvokeGlobal(closure, args);
                    if (result.Length > 0) {
                        if (result[0] is bool br) {
                            return br;
                        }
                    }
                } else if (v is LuaBool b) {
                    return b.IsTrue;
                }
            }
            return false;
        }

        public void LoadScript(string scriptContent) {
            if (this.ScriptState.DoString(scriptContent) is not LuaNil) {
                Trace.WriteLine($"Fatal lua error :: {this.ScriptState.GetError()}", nameof(CampaignScriptHandler));
            }
        }

        public void SetGlobal(string globalName, object obj, bool isUserType) {
            LuaValue v = LuaMarshal.ToLuaValue(obj);
            if (isUserType && v is LuaUserObject luo) {
                var t = this.ScriptState.GetUsertype(obj.GetType());
                luo.SetMetatable(t.InstanceMetatable);
            }
            this.ScriptState._G[globalName] = v;
        }

    }

}
