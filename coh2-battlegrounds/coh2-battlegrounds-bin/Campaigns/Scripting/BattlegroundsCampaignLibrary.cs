using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Battlegrounds.Lua;
using Battlegrounds.Lua.Runtime;

namespace Battlegrounds.Campaigns.Scripting {

    /// <summary>
    /// Lua library for campaign script functionality.
    /// </summary>
    public static class BattlegroundsCampaignLibrary {

        /// <summary>
        /// 
        /// </summary>
        public const string CampaignInstanceField = "__campaignInstance";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="luaState"></param>
        /// <param name="stack"></param>
        public static void RegisterVictoryHandler(LuaState luaState, LuaStack stack) {
            Contract.Requires(stack.Top >= 2);
            if (stack.Pop() is not LuaClosure func) {
                Trace.WriteLine("Failed to register function as victory handler: 'not a function'.", "Battlegrounds:Lua");
                return;
            }
            var team = stack.Pop() as LuaUserObject;
            var inst = (luaState._G[CampaignInstanceField] as LuaUserObject).GetObject<ActiveCampaign>();
            if (!inst.Events.NewLuaVictoryHandler(team.GetObject<CampaignArmyTeam>(), func)) {
                Trace.WriteLine($"Failed to register function as victory handler: 'handler already exists for {team.GetObject<CampaignArmyTeam>()}'.", "Battlegrounds:Lua");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="luaState"></param>
        /// <param name="stack"></param>
        public static void RegisterEventHandler(LuaState luaState, LuaStack stack) {
            Contract.Requires(stack.Top >= 2);
            var type = stack.Pop() as LuaNumber;
            if (stack.Pop() is not LuaClosure func) {
                Trace.WriteLine("Failed to register function in event handler 'not a function'.", "Battlegrounds:Lua");
                return;
            }
            var inst = (luaState._G[CampaignInstanceField] as LuaUserObject).GetObject<ActiveCampaign>();
            inst.Events.NewLuaEventHandler(type.ToInt(), func);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="luaState"></param>
        /// <param name="stack"></param>
        /// <returns></returns>
        public static int TeamOwnsAll(LuaState luaState, LuaStack stack) {
            LuaTable nodes = stack.Pop() as LuaTable;
            var team = (stack.Pop() as LuaUserObject).GetObject<CampaignArmyTeam>();
            var result = nodes.Pairs((k, v) => {
                if ((v as LuaUserObject).GetObject<CampaignMapNode>().Owner != team) {
                    return new LuaBool(false);
                } else {
                    return new LuaNil();
                }
            });
            if (result is LuaNil) {
                result = new LuaBool(true);
            }
            stack.Push(result);
            return 1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="luaState"></param>
        /// <param name="stack"></param>
        /// <returns></returns>
        public static int TeamGetPoints(LuaState luaState, LuaStack stack) {
            return 1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="luaState"></param>
        /// <param name="stack"></param>
        /// <returns></returns>
        public static int TeamGetReservesCount(LuaState luaState, LuaStack stack) {
            return 1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="luaState"></param>
        /// <param name="stack"></param>
        /// <returns></returns>
        public static int TeamGetFormationCount(LuaState luaState, LuaStack stack) {
            return 1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="luaState"></param>
        /// <param name="stack"></param>
        public static void Announce(LuaState luaState, LuaStack stack) {

        }

        /// <summary>
        /// Load library functionality.
        /// </summary>
        /// <param name="luaState">The state to load library functionality.</param>
        public static void LoadLibrary(LuaState luaState) {

            // Register functionality
            luaState.RegisterFunction("Battlegrounds_RegisterWincondition", RegisterVictoryHandler);
            luaState.RegisterFunction("Battlegrounds_RegisterCampaignEventHandler", RegisterEventHandler);

            // Register event functionality
            luaState.RegisterFunction("Battlegrounds_Announce", Announce);

            // Register team functionality
            luaState.RegisterFunction("Team_OwnsAll", TeamOwnsAll);
            luaState.RegisterFunction("Team_GetPoints", TeamGetPoints);
            luaState.RegisterFunction("Team_GetReservesCount", TeamGetReservesCount);
            luaState.RegisterFunction("Team_GetFormationCount", TeamGetFormationCount);

        }

    }

}
