using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Battlegrounds.Lua;

namespace Battlegrounds.Campaigns {
    
    /// <summary>
    /// 
    /// </summary>
    public class ActiveCampaignEventManager {

        /// <summary>
        /// 
        /// </summary>
        public const int ET_Ownership = 1;

        private Dictionary<int, List<LuaClosure>> m_eventHandlers;
        private Dictionary<CampaignArmyTeam, LuaClosure> m_victoryHandlers;

        public LuaState RuntimeState { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public ActiveCampaignEventManager() {
            this.m_eventHandlers = new Dictionary<int, List<LuaClosure>>();
            this.m_victoryHandlers = new Dictionary<CampaignArmyTeam, LuaClosure>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="closure"></param>
        public void NewLuaEventHandler(int eventType, LuaClosure closure) {
            if (this.m_eventHandlers.ContainsKey(eventType)) {
                this.m_eventHandlers[eventType].Add(closure);
            } else {
                this.m_eventHandlers.Add(eventType, new List<LuaClosure>() { closure });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="team"></param>
        /// <param name="closure"></param>
        /// <returns></returns>
        public bool NewLuaVictoryHandler(CampaignArmyTeam team, LuaClosure closure) {
            if (this.m_victoryHandlers.ContainsKey(team)) {
                return false;
            }
            this.m_victoryHandlers.Add(team, closure);
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        public void UpdateEvents() {

        }

        /// <summary>
        /// 
        /// </summary>
        public void UpdateVictory() {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="args"></param>
        public void FireEvent(int eventType, params object[] args) { 
            if (this.m_eventHandlers.TryGetValue(eventType, out List<LuaClosure> closures)) {
                closures.ForEach(x => LuaMarshal.InvokeClosureManaged(x, this.RuntimeState, args));
            }
        }

    }

}
