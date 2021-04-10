using System.Diagnostics;
using System.Diagnostics.Contracts;
using Battlegrounds.Campaigns.API;
using Battlegrounds.Campaigns.Controller;
using Battlegrounds.Lua;
using Battlegrounds.Lua.Runtime;

namespace Battlegrounds.Campaigns.Scripting {

    /// <summary>
    /// Lua library for campaign script functionality.
    /// </summary>
    public static class BattlegroundsCampaignLibrary {

        /// <summary>
        /// Lua name of the in-script reference to the campaign instance.
        /// </summary>
        public const string CampaignInstanceField = "__campaignInstance";

        /// <summary>
        /// Register handler function for evaluating victory conditions for a specific team.
        /// </summary>
        /// <param name="luaState">The state invoking the function.</param>
        /// <param name="stack">The stack of values containing arguments and return values.</param>
        public static void RegisterVictoryHandler(LuaState luaState, LuaStack stack) {
            Contract.Requires(stack.Top >= 2);
            if (stack.Pop() is not LuaClosure func) {
                Trace.WriteLine("Failed to register function as victory handler: 'not a function'.", "Battlegrounds:Lua");
                return;
            }
            var team = stack.Pop() as LuaUserObject;
            var inst = luaState._G[CampaignInstanceField].As<LuaUserObject>().GetObject<ICampaignController>();
            if (!inst.Events.NewLuaVictoryHandler(team.GetObject<CampaignArmyTeam>(), func)) {
                Trace.WriteLine($"Failed to register function as victory handler: 'handler already exists for {team.GetObject<CampaignArmyTeam>()}'.", "Battlegrounds:Lua");
            }
        }

        /// <summary>
        /// Register handler function for when the campaign fires an event.
        /// </summary>
        /// <param name="luaState">The state invoking the function.</param>
        /// <param name="stack">The stack of values containing arguments and return values.</param>
        public static void RegisterEventHandler(LuaState luaState, LuaStack stack) {
            Contract.Requires(stack.Top >= 2);
            var type = stack.Pop() as LuaNumber;
            if (stack.Pop() is not LuaClosure func) {
                Trace.WriteLine("Failed to register function in event handler 'not a function'.", "Battlegrounds:Lua");
                return;
            }
            var inst = luaState._G[CampaignInstanceField].As<LuaUserObject>().GetObject<ICampaignController>();
            inst.Events.NewLuaEventHandler(type.ToInt(), func);
        }

        /// <summary>
        /// Check if a team owns all nodes contained within a <see cref="LuaTable"/>.
        /// </summary>
        /// <param name="luaState">The state invoking the function.</param>
        /// <param name="stack">The stack of values containing arguments and return values.</param>
        /// <returns>The amount of values pushed onto the stack.</returns>
        public static int TeamOwnsAll(LuaState luaState, LuaStack stack) {
            LuaTable nodes = stack.Pop() as LuaTable;
            var team = stack.Pop().As<LuaUserObject>().GetObject<CampaignArmyTeam>();
            var result = nodes.Pairs((k, v) => {
                if ((v as LuaUserObject).GetObject<ICampaignMapNode>().Owner != team) {
                    return new LuaBool(false);
                } else {
                    return LuaNil.Nil;
                }
            });
            if (result is LuaNil) {
                result = new LuaBool(true);
            }
            stack.Push(result);
            return 1;
        }

        /// <summary>
        /// Pushes the amount of points a team has onto the stack.
        /// </summary>
        /// <param name="luaState">The state invoking the function.</param>
        /// <param name="stack">The stack of values containing arguments and return values.</param>
        /// <returns>The amount of values pushed onto the stack.</returns>
        public static int TeamGetPoints(LuaState luaState, LuaStack stack) {
            var teamType = stack.Pop().As<LuaUserObject>().GetObject<CampaignArmyTeam>();
            var inst = luaState._G[CampaignInstanceField].As<LuaUserObject>().GetObject<ICampaignController>();
            var team = inst.GetTeam(teamType);
            stack.Push(team.VictoryPoints);
            return 1;
        }

        /// <summary>
        /// Pushes the amount of reserve divisions a team has onto the stack.
        /// </summary>
        /// <param name="luaState">The state invoking the function.</param>
        /// <param name="stack">The stack of values containing arguments and return values.</param>
        /// <returns>The amount of values pushed onto the stack.</returns>
        public static int TeamGetReservesCount(LuaState luaState, LuaStack stack) {
            var teamType = stack.Pop().As<LuaUserObject>().GetObject<CampaignArmyTeam>();
            var inst = luaState._G[CampaignInstanceField].As<LuaUserObject>().GetObject<ICampaignController>();
            var team = inst.GetTeam(teamType);
            stack.Push(team.GetReserves().Count);
            return 1;
        }

        /// <summary>
        /// Pushes the amount of formations owned by a team onto the stack.
        /// </summary>
        /// <param name="luaState">The state invoking the function.</param>
        /// <param name="stack">The stack of values containing arguments and return values.</param>
        /// <returns>The amount of values pushed onto the stack.</returns>
        public static int TeamGetFormationCount(LuaState luaState, LuaStack stack) {
            var teamType = stack.Pop().As<LuaUserObject>().GetObject<CampaignArmyTeam>();
            var inst = luaState._G[CampaignInstanceField].As<LuaUserObject>().GetObject<ICampaignController>();
            int count = 0;
            inst.Map.EachFormation(x => count += x.Team == teamType ? 1 : 0);
            stack.Push(count);
            return 1;
        }

        /// <summary>
        /// Triggers an announcement event in the GUI.
        /// </summary>
        /// <param name="luaState">The state invoking the function.</param>
        /// <param name="stack">The stack of values containing arguments and return values.</param>
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
