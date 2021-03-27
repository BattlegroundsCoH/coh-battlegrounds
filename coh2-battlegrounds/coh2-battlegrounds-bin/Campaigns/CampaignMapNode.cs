using System.Collections.Generic;
using System.Diagnostics;

using Battlegrounds.Campaigns.API;
using Battlegrounds.Campaigns.Organisations;
using Battlegrounds.Lua;
using Battlegrounds.Util.Lists;

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
        /// Get the name of the function to invoke when weighting nodes in the Dijkstra algorithm.
        /// </summary>
        public string NodeFilter { get; }

        /// <summary>
        /// 
        /// </summary>
        public IVisualCampaignNode VisualNode { get; set; }

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
        public DistinctList<Formation> Occupants { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="u"></param>
        /// <param name="v"></param>
        public CampaignMapNode(string name, double u, double v, string filter) {
            this.NodeName = name;
            this.NodeFilter = filter ?? string.Empty;
            this.U = u;
            this.V = v;
            this.VisualNode = null;
            this.Owner = CampaignArmyTeam.TEAM_NEUTRAL;
            this.Maps = new List<NodeMap>();
            this.Occupants = new DistinctList<Formation>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="luaValue"></param>
        public void SetMapData(LuaValue luaValue) {
            if (luaValue is LuaString ls) {
                string lss = ls.Str();
                if (!string.IsNullOrEmpty(lss)) {
                    this.Maps.Add(new NodeMap(lss, string.Empty, false));
                    return;
                }
            } else if (luaValue is LuaTable mapOptions) {
                mapOptions.Pairs((k, v) => {
                    if (v is LuaTable vt) {
                        string name = (vt["scenario"] as LuaString).Str();
                        string func = (vt["script"] as LuaString)?.Str() ?? string.Empty;
                        bool isWinter = (vt["winter"] as LuaBool)?.IsTrue ?? false;
                        this.Maps.Add(new NodeMap(name, func, isWinter));
                    } else if (v is LuaString s) {
                        this.Maps.Add(new NodeMap(s.Str(), string.Empty, false));
                    }
                });
                return;
            }
            Trace.WriteLine($"Node '{this.NodeName}' has no valid map defined.", nameof(CampaignMapNode));
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
            this.VisualNode?.OwnershipChanged(team);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public void SetValue(double value) {
            this.Value = value;
            this.VisualNode?.VictoryValueChanged(value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public void SetAttrition(double value) {
            this.Attrition = value;
            this.VisualNode?.AttritionValueChanged(value);
        }

        /// <summary>
        /// 
        /// </summary>
        public void EndOfRound() {
            
            // Apply attrition
            this.Occupants.ForEach(x => {
                if (x.CanMove) {
                    x.ApplyAttrition(this.Attrition);
                }
            });

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="formation"></param>
        public void AddOccupant(Formation formation) {
            this.Occupants.Add(formation);
            this.VisualNode?.OccupantAdded(formation);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="formation"></param>
        public void RemoveOccupant(Formation formation) {
            this.Occupants.Remove(formation);
            this.VisualNode?.OccupantRemoved(formation);
        }

    }

}
