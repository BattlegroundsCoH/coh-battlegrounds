using System.Collections.Generic;
using Battlegrounds.Campaigns.Organisations;
using Battlegrounds.Lua;

namespace Battlegrounds.Campaigns {

    /// <summary>
    /// 
    /// </summary>
    public enum CampaignMapNodeProperty {
        Owner,
        Value,
        Attrition,
        Occupants,
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="self"></param>
    /// <param name="property"></param>
    /// <param name="value"></param>
    public delegate void CampaignNodePropertyChangeHandler(CampaignMapNode self, CampaignMapNodeProperty property, object value);

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

        /// <summary>
        /// 
        /// </summary>
        public double U { get; }

        /// <summary>
        /// 
        /// </summary>
        public double V { get; }

        /// <summary>
        /// 
        /// </summary>
        public string NodeName { get; }

        /// <summary>
        /// 
        /// </summary>
        public object VisualNode { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public CampaignArmyTeam Owner { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public int OccupantCapacity { get; init; }

        /// <summary>
        /// 
        /// </summary>
        public double Value { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public double Attrition { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsLeaf { get; init; }

        /// <summary>
        /// 
        /// </summary>
        public List<NodeMap> Maps { get; }

        /// <summary>
        /// 
        /// </summary>
        public List<Formation> Occupants { get; }

        /// <summary>
        /// 
        /// </summary>
        public event CampaignNodePropertyChangeHandler NodeChange;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="u"></param>
        /// <param name="v"></param>
        public CampaignMapNode(string name, double u, double v) {
            this.NodeName = name;
            this.U = u;
            this.V = v;
            this.VisualNode = null;
            this.Owner = CampaignArmyTeam.TEAM_NEUTRAL;
            this.Maps = new List<NodeMap>();
            this.Occupants = new List<Formation>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="luaValue"></param>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="formation"></param>
        /// <returns></returns>
        public bool CanMoveTo(Formation formation) {
            if (this.Occupants.Count + 1 <= this.OccupantCapacity) {
                // TODO: Check if script allows for it
                return true;
            } else {
                return this.Owner != formation.Team;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="team"></param>
        public void SetOwner(CampaignArmyTeam team) {
            this.Owner = team;
            this.NodeChange?.Invoke(this, CampaignMapNodeProperty.Owner, team);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public void SetValue(double value) {
            this.Value = value;
            this.NodeChange?.Invoke(this, CampaignMapNodeProperty.Value, value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public void SetAttrition(double value) {
            this.Attrition = value;
            this.NodeChange?.Invoke(this, CampaignMapNodeProperty.Attrition, value);
        }

    }

}
