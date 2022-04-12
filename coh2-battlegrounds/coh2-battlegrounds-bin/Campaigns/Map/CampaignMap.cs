using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Battlegrounds.Campaigns.API;
using Battlegrounds.Campaigns.Organisations;
using Battlegrounds.Game.Database;
using Battlegrounds.Lua;

namespace Battlegrounds.Campaigns.Map {

    /// <summary>
    /// 
    /// </summary>
    public class CampaignMap : ICampaignMap {

        /// <summary>
        /// 
        /// </summary>
        public byte[] RawImageData { get; }

        /// <summary>
        /// 
        /// </summary>
        public ICampaignScriptHandler ScriptHandler { get; set; }

        private LuaTable MapDef { get; }

        private List<CampaignMapNode> m_nodes;
        private List<CampaignMapTransition> m_edges;

        public CampaignMap(CampaignMapData mapData) {

            // Store raw image data
            this.RawImageData = mapData.RawImageData;

            // Store lua map def table
            this.MapDef = mapData.Data;

            // Create lists for network data
            this.m_nodes = new List<CampaignMapNode>();
            this.m_edges = new List<CampaignMapTransition>();

        }

        public void BuildNetwork() {

            // Create nodes
            var nodes = this.MapDef["nodes"] as LuaTable;
            nodes.Pairs((k, v) => {
                LuaTable vt = v as LuaTable;
                var node = new CampaignMapNode(k.Str(), vt["u"] as LuaNumber, vt["v"] as LuaNumber, vt.ByKey<LuaString>("filter")?.Str()) {
                    IsLeaf = (vt["leaf"] as LuaBool)?.IsTrue ?? false,
                    OccupantCapacity = (int)(vt["capacity"] as LuaNumber),
                };
                node.SetAttrition(vt["attrition"] as LuaNumber);
                node.SetValue(vt["attrition"] as LuaNumber);
                if (vt["owner"] is LuaString ls) {
                    node.SetOwner(ls.Str().ToUpper() == "ALLIES" ? CampaignArmyTeam.TEAM_ALLIES : CampaignArmyTeam.TEAM_AXIS);
                } else {
                    node.SetOwner(CampaignArmyTeam.TEAM_NEUTRAL);
                }
                node.SetMapData(vt["map"]);
                this.m_nodes.Add(node);
            });

            // Create transitions
            var transitions = this.MapDef["transitions"] as LuaTable;
            transitions.Pairs((k, v) => {
                LuaTable vt = v as LuaTable;
                string from = vt[1].Str();
                string to = vt[2].Str();
                string type = vt[3].Str();
                var fromNode = this.m_nodes.FirstOrDefault(x => x.NodeName == from);
                var toNode = this.m_nodes.FirstOrDefault(x => x.NodeName == to);
                this.m_edges.Add(new CampaignMapTransition(fromNode, toNode, type.ToLower() switch {
                    "binary" => CampaignMapTransitionType.Binary,
                    "unary" => CampaignMapTransitionType.Unary,
                    _ => throw new Exception()
                }));
            });

        }

        public void EachNode(Action<ICampaignMapNode> action) => this.m_nodes.ForEach(action);

        public void EachTransition(Action<ICampaignMapNodeTransition> action) => this.m_edges.ForEach(action);

        public void EachFormation(Action<ICampaignFormation> action) => this.m_nodes.ForEach(x => x.Occupants.ForEach(action));

        [LuaUserobjectMethod(UseMarshalling = true)]
        public ICampaignMapNode FromName(string name) => this.m_nodes.FirstOrDefault(x => x.NodeName == name);

        public bool SpawnFormationAt(string nodeIdentifier, ICampaignFormation formation) {
            if (this.FromName(nodeIdentifier) is ICampaignMapNode node) {

                if (node.CanMoveTo(formation)) {
                    formation.SetNodeLocation(node);
                    return true;
                }

            }
            return false;
        }

        public bool FindPath(ICampaignFormation formation, ICampaignMapNode end, out List<ICampaignMapNode> path) {
            var from = formation.Node;
            path = Dijkstra(from, end, formation);
            if (path.Count > 0) {
                Trace.WriteLine(string.Join(" -> ", path.Select(x => x.NodeName)), $"{nameof(CampaignMap)}:PathResult");
                if (path[0] == from && path[^1] == end) {
                    return true;
                }
            }
            return false;
        }

        private float Weight(ICampaignMapNode prev, ICampaignMapNode to, ICampaignFormation formation) {
            if (to.CanMoveTo(formation)) {
                float w = 1.0f;
                if (to.Owner != formation.Team) {
                    w += 2.5f;
                }
                if (!string.IsNullOrEmpty(to.NodeFilter)!) {
                    object[] results = this.ScriptHandler.CallGlobal(to.NodeFilter, formation);
                    if (results.Length == 1 && results[0] is double d) {
                        w += (float)d;
                    } else {
                        Trace.WriteLine($"Lua function '{to.NodeFilter}' returned non-number value and will be ignored.", nameof(ICampaignMapNode));
                    }
                }
                return w;
            }
            return float.PositiveInfinity;
        }

        private List<ICampaignMapNode> Dijkstra(ICampaignMapNode from, ICampaignMapNode end, ICampaignFormation formation) {

            HashSet<ICampaignMapNode> nodes = new HashSet<ICampaignMapNode>();
            Dictionary<ICampaignMapNode, float> distance = new Dictionary<ICampaignMapNode, float>();
            Dictionary<ICampaignMapNode, ICampaignMapNode> prev = new Dictionary<ICampaignMapNode, ICampaignMapNode>();

            foreach (ICampaignMapNode node in this.m_nodes) {
                distance[node] = float.PositiveInfinity;
                prev[node] = null;
                nodes.Add(node);
            }

            distance[from] = 0.0f;

            ICampaignMapNode MinDis() {
                float f = float.PositiveInfinity;
                ICampaignMapNode q = null;
                foreach (ICampaignMapNode n in nodes) {
                    if (distance[n] < f) {
                        q = n;
                    }
                }
                return q;
            }

            while (nodes.Count > 0) {

                ICampaignMapNode u = MinDis();
                nodes.Remove(u);

                if (u == end || u is null)
                    break;

                var neighbours = this.GetNeighbours(u, nodes);

                foreach (var neighbour in neighbours) {
                    float d = distance[u] + this.Weight(u, neighbour, formation);
                    if (d < distance[neighbour]) {
                        distance[neighbour] = d;
                        prev[neighbour] = u;
                    }
                }

            }

            List<ICampaignMapNode> path = new List<ICampaignMapNode>();
            ICampaignMapNode t = end;
            if (prev[t] is not null || t == from) {
                while (t is not null) {
                    path.Insert(0, t);
                    t = prev[t];
                }
            }

            return path;

        }

        private List<ICampaignMapNode> GetNeighbours(ICampaignMapNode source, IEnumerable<ICampaignMapNode> filter) {
            List<ICampaignMapNode> neighbours = this.GetNodeNeighbours(source);
            int fcount = filter.Count();
            if (fcount == 0) {
                return neighbours;
            } else {
                return neighbours.Where(x => filter.Contains(x)).ToList();
            }
        }

        public List<ICampaignMapNode> GetNodeNeighbours(ICampaignMapNode mapNode)
            => this.GetNodeNeighbours(mapNode, x => true);

        public List<ICampaignMapNode> GetNodeNeighbours(ICampaignMapNode mapNode, Predicate<ICampaignMapNode> predicate) {
            List<ICampaignMapNode> neighbours = new List<ICampaignMapNode>();
            foreach (var transition in this.m_edges) {
                bool a = transition.From == mapNode;
                bool b = transition.To == mapNode;
                bool binary = transition.TransitionType == CampaignMapTransitionType.Binary && (a || b);
                bool unary = transition.TransitionType == CampaignMapTransitionType.Unary && a;
                if (binary || unary) {
                    var node = a ? transition.To : transition.From;
                    if (predicate(node)) {
                        neighbours.Add(node);
                    }
                }
            }
            return neighbours;
        }

        public Scenario PickScenario(ICampaignMapNode node, ICampaignTurn campaignTurn) {
            Scenario result = null;
            if (node.Maps.Count == 1) {
                if (ScenarioList.TryFindScenario(node.Maps[0].MapName, out result)) {
                    return result;
                }
            } else {
                for (int i = 0; i < node.Maps.Count; i++) {
                    if (node.Maps[i].IsWinterVariant == campaignTurn.IsWinter) {
                        if (string.IsNullOrEmpty(node.Maps[i].ScriptDeterminant)) {
                            if (ScenarioList.TryFindScenario(node.Maps[i].MapName, out result)) {
                                return result;
                            }
                        } else {
                            object[] scriptDetermination = this.ScriptHandler.CallGlobal(node.Maps[i].ScriptDeterminant);
                            if (scriptDetermination.Length == 1 && scriptDetermination[0] is true) {
                                if (ScenarioList.TryFindScenario(node.Maps[i].MapName, out result)) {
                                    return result;
                                }
                            }
                        }
                    }
                }
            }
            return result;
        }

        public void DeployDivision(uint divisionID, Army army, List<string> deployLocations) {
            if (army.Divisions.FirstOrDefault(x => x.DivisionUid == divisionID) is Division div) {

                // Create formation from division
                Formation form = new Formation();
                form.FromDivision(div);

                // Determine method to spawn (just spawn or split to fit)
                if (deployLocations.Count == 1) {
                    this.SpawnFormationAt(deployLocations[0], form);
                } else {
                    if (form.CanSplit) { // Make sure we can split
                        Formation[] split = form.Split(deployLocations.Count);
                        for (int i = 0; i < deployLocations.Count; i++) {
                            this.SpawnFormationAt(deployLocations[i], split[i]);
                        }
                    } else {
                        Trace.WriteLine($"Army '{army.ArmyName.LocaleID}' contains deploy division with ID {div.Name.LocaleID}, as there are too few regiments to distribute.", $"{nameof(CampaignMap)}::{nameof(DeployDivision)}");
                    }
                }

                // Log that we're deploying some unit
                Trace.WriteLine($"Army '{army.ArmyName.LocaleID}' deploying {div.Name.LocaleID} at {string.Join(", ", deployLocations)}", $"{nameof(CampaignMap)}::{nameof(DeployDivision)}");

            } else {
                Trace.WriteLine($"Army '{army.ArmyName.LocaleID}' contains no division with ID {divisionID}", $"{nameof(CampaignMap)}::{nameof(DeployDivision)}");
            }
        }

    }

}
