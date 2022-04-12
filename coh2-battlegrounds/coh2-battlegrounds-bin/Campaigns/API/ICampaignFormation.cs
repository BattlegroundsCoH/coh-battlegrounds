using System.Collections.Generic;

using Battlegrounds.Campaigns.Organisations;
using Battlegrounds.Lua;

namespace Battlegrounds.Campaigns.API {

    /// <summary>
    /// 
    /// </summary>
    /// <param name="formation"></param>
    /// <param name="origin"></param>
    /// <param name="destination"></param>
    public delegate void FormationPositionEventHandler(ICampaignFormation formation, ICampaignMapNode origin, ICampaignMapNode destination);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="formation"></param>
    /// <param name="killed"></param>
    public delegate void FormationDisbandedEventHandler(ICampaignFormation formation, bool killed);

    /// <summary>
    /// Interface for representing a unit formation in a campaign.
    /// </summary>
    public interface ICampaignFormation {

        /// <summary>
        /// Get the current node occupied by the formation.
        /// </summary>
        /// <remarks>
        /// Lua-Visible
        /// </remarks>
        [LuaUserobjectProperty]
        ICampaignMapNode Node { get; }

        /// <summary>
        /// Get the destination of this formation
        /// </summary>
        ICampaignMapNode Destination { get; }

        /// <summary>
        /// Get the <see cref="CampaignArmyTeam"/> owning the formation.
        /// </summary>
        /// <remarks>
        /// Lua-Visible
        /// </remarks>
        [LuaUserobjectProperty]
        CampaignArmyTeam Team { get; }

        /// <summary>
        /// 
        /// </summary>
        Regiment[] Regiments { get; }

        /// <summary>
        /// Get the name of the dominant army in charge of this formation.
        /// </summary>
        /// <remarks>
        /// Lua-Visible
        /// </remarks>
        [LuaUserobjectProperty]
        string Army { get; }

        /// <summary>
        /// Get if the unit formation can be split.
        /// </summary>
        bool CanSplit { get; }

        /// <summary>
        /// Get if the formation can currently move
        /// </summary>
        bool CanMove { get; }

        /// <summary>
        /// 
        /// </summary>
        event FormationPositionEventHandler FormationMoved;

        /// <summary>
        /// 
        /// </summary>
        event FormationDisbandedEventHandler FormationDisbanded;

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
        void SetNodeDestinations(List<ICampaignMapNode> nodes);

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
        /// Updates destination and decrements available move distance by 1.
        /// </summary>
        void MoveToDestination();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        List<ICampaignMapNode> GetPath();

    }

}
