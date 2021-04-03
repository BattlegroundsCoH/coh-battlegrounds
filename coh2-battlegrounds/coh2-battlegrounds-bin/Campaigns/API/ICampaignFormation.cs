using System.Collections.Generic;
using Battlegrounds.Campaigns.Organisations;
using Battlegrounds.Lua;

namespace Battlegrounds.Campaigns.API {
    
    /// <summary>
    /// 
    /// </summary>
    public interface ICampaignFormation {
        
        /// <summary>
        /// 
        /// </summary>
        [LuaUserobjectProperty]
        ICampaignMapNode Node { get; }

        /// <summary>
        /// 
        /// </summary>
        ICampaignMapNode Destination { get; }

        /// <summary>
        /// 
        /// </summary>
        [LuaUserobjectProperty]
        CampaignArmyTeam Team { get; }
        
        /// <summary>
        /// 
        /// </summary>
        Regiment[] Regiments { get; }

        /// <summary>
        /// 
        /// </summary>
        [LuaUserobjectProperty]
        string Army { get; }

        /// <summary>
        /// 
        /// </summary>
        bool CanSplit { get; }

        /// <summary>
        /// 
        /// </summary>
        bool CanMove { get; }

        /// <summary>
        /// 
        /// </summary>
        void EndOfRound();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="attrition"></param>
        void ApplyAttrition(double attrition);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        void SetNodeLocation(ICampaignMapNode node);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nodes"></param>
        void SetNodeDestinationsAndMove(List<ICampaignMapNode> nodes);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="killAll"></param>
        [LuaUserobjectMethod(UseMarshalling = true)]
        void Disband(bool killAll);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [LuaUserobjectMethod(UseMarshalling = true)]
        float CalculateStrength();

        /// <summary>
        /// 
        /// </summary>
        void OnMoved(bool actualMove = true);
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        List<ICampaignMapNode> GetPath();

    }

}
