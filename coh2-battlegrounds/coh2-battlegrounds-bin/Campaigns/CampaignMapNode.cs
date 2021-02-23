using System.Collections.Generic;
using Battlegrounds.Lua;

namespace Battlegrounds.Campaigns {
    
    /// <summary>
    /// Represents a node on the campaign map where companies/regiments can be located and where armies can fight.
    /// </summary>
    public class CampaignMapNode {

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

        public double U { get; }

        public double V { get; }

        public string NodeName { get; }

        public object VisualNode { get; set; }

        public CampaignArmyTeam Owner { get; set; }

        public List<int> Occupants { get; }

        public int OccupantCapacity { get; init; }

        public double Value { get; set; }

        public double Attrition { get; set; }

        public bool IsLeaf { get; init; }

        public List<NodeMap> Maps { get; }

        public CampaignMapNode(string name, double u, double v) {
            this.NodeName = name;
            this.U = u;
            this.V = v;
            this.VisualNode = null;
            this.Owner = CampaignArmyTeam.TEAM_NEUTRAL;
            this.Occupants = new List<int>();
            this.Maps = new List<NodeMap>();
        }

        public void SetMapData(LuaValue luaValue) {
            if (luaValue is LuaString ls) {
                this.Maps.Add(new NodeMap(ls.Str(), string.Empty, false));
            } else if (luaValue is LuaTable mapOptions) {
                mapOptions.Pairs((k, v) => {
                    if (v is LuaTable vt) {
                        string name = (vt["name"] as LuaString).Str();
                        string func = (vt["script"] as LuaString)?.Str() ?? string.Empty;
                        bool isWinter = (vt["winter"] as LuaBool)?.IsTrue ?? false;
                        this.Maps.Add(new NodeMap(name, func, isWinter));
                    } else if (v is LuaString s) {
                        this.Maps.Add(new NodeMap(s.Str(), string.Empty, false));
                    }
                });
            }
        }

    }

}
