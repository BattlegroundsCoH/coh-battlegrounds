using System.Collections.Generic;
using System.Linq;
using Battlegrounds.Campaigns.API;
using Battlegrounds.Lua;

namespace Battlegrounds.Campaigns.Models {
    
    /// <summary>
    /// Campaign event handler for handling events.
    /// </summary>
    public class SingleplayerCampaignEventManager : ICampaignEventManager {

        private Dictionary<int, List<LuaClosure>> m_eventHandlers;
        private Dictionary<CampaignArmyTeam, LuaClosure> m_victoryHandlers;

        /// <summary>
        /// Get or set the active script handler
        /// </summary>
        public ICampaignScriptHandler ScriptHandler{ get; set; }

        /// <summary>
        /// Initialize a new <see cref="SingleplayerCampaignEventManager"/> class with no events.
        /// </summary>
        public SingleplayerCampaignEventManager() {
            this.m_eventHandlers = new Dictionary<int, List<LuaClosure>>();
            this.m_victoryHandlers = new Dictionary<CampaignArmyTeam, LuaClosure>();
        }

        /// <summary>
        /// Add a new lua event handler of specified event type.
        /// </summary>
        /// <param name="eventType">The event type.</param>
        /// <param name="closure">The closure to invoke.</param>
        public void NewLuaEventHandler(int eventType, LuaClosure closure) {
            if (this.m_eventHandlers.ContainsKey(eventType)) {
                this.m_eventHandlers[eventType].Add(closure);
            } else {
                this.m_eventHandlers.Add(eventType, new List<LuaClosure>() { closure });
            }
        }

        /// <summary>
        /// Add a lua victory handler for specified team.
        /// </summary>
        /// <param name="team">The team to set victory handler for.</param>
        /// <param name="closure">The closure to invoke.</param>
        /// <returns>If no event exists <see langword="true"/> is returned; Otherwise <see langword="false"/>.</returns>
        public bool NewLuaVictoryHandler(CampaignArmyTeam team, LuaClosure closure) {
            if (this.m_victoryHandlers.ContainsKey(team)) {
                return false;
            }
            this.m_victoryHandlers.Add(team, closure);
            return true;
        }

        /// <summary>
        /// Invoke the scripted victory check functions.
        /// </summary>
        public bool UpdateVictory() {
            return this.m_victoryHandlers.Any(x => {
                object[] result = LuaMarshal.InvokeClosureManaged(x.Value, this.ScriptHandler.ScriptState);
                if (result.Length == 1 && result[0] is bool b) {
                    return b;
                } else {
                    return false;
                }
            });
        }

        /// <summary>
        /// Invoke all events of designed <paramref name="eventType"/> with specified <paramref name="args"/>.
        /// </summary>
        /// <param name="eventType">The event type to invoke.</param>
        /// <param name="args">The arguments to call each function with.</param>
        public void FireEvent(int eventType, params object[] args) { 
            if (this.m_eventHandlers.TryGetValue(eventType, out List<LuaClosure> closures)) {
                closures.ForEach(x => LuaMarshal.InvokeClosureManaged(x, this.ScriptHandler.ScriptState, args));
            }
        }

    }

}
