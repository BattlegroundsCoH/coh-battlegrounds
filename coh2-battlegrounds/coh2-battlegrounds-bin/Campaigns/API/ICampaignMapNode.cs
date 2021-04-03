using System.Collections.Generic;
using Battlegrounds.Util.Lists;

namespace Battlegrounds.Campaigns.API {

    /// <summary>
    /// Readonly struct representing a map that can be played in node.
    /// </summary>
    public readonly struct NodeMap {

        /// <summary>
        /// Get the name of the map file to play.
        /// </summary>
        public readonly string MapName { get; }

        /// <summary>
        /// Get if this is the winter variant of the map.
        /// </summary>
        public readonly bool IsWinterVariant { get; }

        /// <summary>
        /// Get the name of the function to invoke when checking if map should be used. (Leave empty if not intended for use
        /// </summary>
        public readonly string ScriptDeterminant { get; }

        public NodeMap(string name, string func, bool winter) {
            this.MapName = name;
            this.ScriptDeterminant = func;
            this.IsWinterVariant = winter;
        }

    }

    /// <summary>
    /// 
    /// </summary>
    public interface ICampaignMapNode {

        /// <summary>
        /// Get the U-index for the node.
        /// </summary>
        public double U { get; }

        /// <summary>
        /// Get the V-index for the node.
        /// </summary>
        public double V { get; }

        /// <summary>
        /// Get the name of the node.
        /// </summary>
        public string NodeName { get; }

        /// <summary>
        /// Get the name of the function to invoke when weighting nodes in the Dijkstra algorithm.
        /// </summary>
        public string NodeFilter { get; }

        /// <summary>
        /// Get the visual representation of the node.
        /// </summary>
        public IVisualCampaignNode VisualNode { get; set; }

        /// <summary>
        /// Get the current owner of the node.
        /// </summary>
        public CampaignArmyTeam Owner { get; }

        /// <summary>
        /// Get the max capacity of the node.
        /// </summary>
        public int OccupantCapacity { get; }

        /// <summary>
        /// Get the victory value of the node.
        /// </summary>
        public double Value { get; }

        /// <summary>
        /// Get the attrition value of the node.
        /// </summary>
        public double Attrition { get; }

        /// <summary>
        /// Get if the node is a leaf node.
        /// </summary>
        public bool IsLeaf { get; }

        /// <summary>
        /// Get the maps that can be played on this node.
        /// </summary>
        public List<NodeMap> Maps { get; }

        /// <summary>
        /// Get the occupants of the node.
        /// </summary>
        public DistinctList<ICampaignFormation> Occupants { get; }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="formation"></param>
        /// <returns></returns>
        bool CanMoveTo(ICampaignFormation formation);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="team"></param>
        void SetOwner(CampaignArmyTeam team);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        void SetValue(double value);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        void SetAttrition(double value);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="formation"></param>
        void AddOccupant(ICampaignFormation formation);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="formation"></param>
        void RemoveOccupant(ICampaignFormation formation);

        /// <summary>
        /// 
        /// </summary>
        void EndOfRound();

    }

}
