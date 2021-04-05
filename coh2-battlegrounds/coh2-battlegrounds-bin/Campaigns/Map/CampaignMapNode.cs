using System.Collections.Generic;
using System.Diagnostics;

using Battlegrounds.Campaigns.API;
using Battlegrounds.Lua;
using Battlegrounds.Util.Lists;

namespace Battlegrounds.Campaigns.Map {

    /// <summary>
    /// Represents a node on the campaign map where companies/regiments can be located and where armies can fight.
    /// </summary>
    public class CampaignMapNode : ICampaignMapNode {

        public double U { get; }

        public double V { get; }

        public string NodeName { get; }

        public string NodeFilter { get; }

        public CampaignArmyTeam Owner { get; private set; }

        public int OccupantCapacity { get; init; }

        public double Value { get; private set; }

        public double Attrition { get; private set; }

        public bool IsLeaf { get; init; }

        public List<NodeMap> Maps { get; }

        public DistinctList<ICampaignFormation> Occupants { get; }

        public event NodeEventHandler<CampaignArmyTeam> OnOwnershipChange;
        public event NodeEventHandler<ICampaignFormation> OnOccupantEnter;
        public event NodeEventHandler<ICampaignFormation> OnOccupantLeave;

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
            this.Owner = CampaignArmyTeam.TEAM_NEUTRAL;
            this.Maps = new List<NodeMap>();
            this.Occupants = new DistinctList<ICampaignFormation>();
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

        public bool CanMoveTo(ICampaignFormation formation) {
            if (this.Occupants.Count + 1 <= this.OccupantCapacity) {
                return true;
            } else {
                return this.Owner != formation.Team;
            }
        }

        public void SetOwner(CampaignArmyTeam team) {
            this.Owner = team;
            this.OnOwnershipChange?.Invoke(this, team);
        }

        public void SetValue(double value) => this.Value = value;

        public void SetAttrition(double value) => this.Attrition = value;

        public void EndOfRound() {
            
            // Apply attrition
            this.Occupants.ForEach(x => {
                if (x.CanMove) {
                    x.ApplyAttrition(this.Attrition);
                }
            });

        }

        public void AddOccupant(ICampaignFormation formation) {
            this.Occupants.Add(formation);
            this.OnOccupantEnter?.Invoke(this, formation);
        }

        public void RemoveOccupant(ICampaignFormation formation) {
            this.Occupants.Remove(formation);
            this.OnOccupantLeave?.Invoke(this, formation);
        }

    }

}
