using Battlegrounds.Lua;

namespace Battlegrounds.Campaigns.API {
    
    /// <summary>
    /// 
    /// </summary>
    public interface ICampaignEventManager {

        /// <summary>
        /// Event Type - Node ownership changed.
        /// </summary>
        public const int ET_Ownership = 1;

        /// <summary>
        /// 
        /// </summary>
        ICampaignScriptHandler ScriptHandler { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="args"></param>
        void FireEvent(int eventType, params object[] args);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        bool UpdateVictory();
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="closure"></param>
        void NewLuaEventHandler(int eventType, LuaClosure closure);
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="team"></param>
        /// <param name="closure"></param>
        /// <returns></returns>
        bool NewLuaVictoryHandler(CampaignArmyTeam team, LuaClosure closure);

    }

}
