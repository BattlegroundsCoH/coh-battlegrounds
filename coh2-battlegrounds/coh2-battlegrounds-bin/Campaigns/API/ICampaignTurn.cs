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
    public interface ICampaignTurn {

        /// <summary>
        /// 
        /// </summary>
        [LuaUserobjectProperty]
        CampaignArmyTeam CurrentTurn { get; }

        /// <summary>
        /// 
        /// </summary>
        [LuaUserobjectProperty]
        string Date { get; }

        /// <summary>
        /// 
        /// </summary>
        [LuaUserobjectProperty]
        bool IsWinter { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dates"></param>
        void SetWinterDates((int year, int month, int day)[] dates);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="wasRound"></param>
        /// <returns></returns>
        bool EndTurn(out bool wasRound);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [LuaUserobjectMethod(UseMarshalling = true)]
        bool IsEndDate();

    }

}
