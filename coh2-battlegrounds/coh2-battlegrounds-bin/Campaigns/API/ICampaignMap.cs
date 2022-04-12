using System;
using System.Collections.Generic;

using Battlegrounds.Campaigns.Organisations;
using Battlegrounds.Game.Database;
using Battlegrounds.Lua;

namespace Battlegrounds.Campaigns.API {

    /// <summary>
    /// Interface for handling map-related behaviour for campaigns.
    /// </summary>
    public interface ICampaignMap {

        /// <summary>
        /// 
        /// </summary>
        byte[] RawImageData { get; }

        /// <summary>
        /// 
        /// </summary>
        ICampaignScriptHandler ScriptHandler { get; set; }

        /// <summary>
        /// 
        /// </summary>
        void BuildNetwork();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        void EachNode(Action<ICampaignMapNode> action);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        void EachTransition(Action<ICampaignMapNodeTransition> action);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        void EachFormation(Action<ICampaignFormation> action);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [LuaUserobjectMethod(UseMarshalling = true)]
        ICampaignMapNode FromName(string name);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <param name="campaignTurn"></param>
        /// <returns></returns>
        Scenario PickScenario(ICampaignMapNode node, ICampaignTurn campaignTurn);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="divisionID"></param>
        /// <param name="army"></param>
        /// <param name="locations"></param>
        void DeployDivision(uint divisionID, Army army, List<string> locations);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mapNode"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        List<ICampaignMapNode> GetNodeNeighbours(ICampaignMapNode mapNode, Predicate<ICampaignMapNode> predicate);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="formation"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        bool FindPath(ICampaignFormation formation, ICampaignMapNode end, out List<ICampaignMapNode> path);

    }

}
