using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Battlegrounds.Campaigns.Organisations;
using Battlegrounds.Lua;

namespace Battlegrounds.Campaigns {
    
    public class CampaignMap {

        public byte[] RawImageData { get; }

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
                var node = new CampaignMapNode(k.Str(), vt["u"] as LuaNumber, vt["v"] as LuaNumber) {
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

        public void EachNode(Action<CampaignMapNode> action) => this.m_nodes.ForEach(action);

        public void EachTransition(Action<CampaignMapTransition> action) => this.m_edges.ForEach(action);

        public void EachFormation(Action<Formation> action) => this.m_nodes.ForEach(x => x.Occupants.ForEach(action));

        public CampaignMapNode FromName(string name) => this.m_nodes.FirstOrDefault(x => x.NodeName == name);

        public bool SpawnFormationAt(string nodeIdentifier, Formation formation) {
            if (this.FromName(nodeIdentifier) is CampaignMapNode node) {
                
                if (node.CanMoveTo(formation)) {
                    formation.SetNodeLocation(node);
                    node.Occupants.Add(formation);
                    return true;
                }

            }
            return false;
        }

        public void MoveTo(Formation formation, CampaignMapNode target) {
            
            // Update occupants
            formation.Node.Occupants.Remove(formation);
            target.Occupants.Add(formation);
            
            // Set location and update destination
            formation.SetNodeLocation(target);
            formation.UpdatePath();

            // Update owner
            if (target.Owner != formation.Team) {
                target.SetOwner(formation.Team);
            }

            // Invoke lua script
            
        }

        public bool SetPath(CampaignMapNode from, CampaignMapNode end, Formation formation) {
            var path = Dijkstra(from, end, formation);
            if (path.Count > 0) {
                Trace.WriteLine(string.Join(" -> ", path.Select(x => x.NodeName)), $"{nameof(CampaignMap)}:PathResult");
                if (path[0] == from && path[^1] == end) {
                    formation.SetNodeDestinations(path);
                    return true;
                }
            }
            return false;
        }

        private float Weight(CampaignMapNode prev, CampaignMapNode to, Formation formation) {
            if (to.CanMoveTo(formation)) {
                float w = 1.0f;
                if (to.Owner != formation.Team) {
                    w += 2.5f;
                }
                // TODO: Check with lua scripts if it's possible to move.
                return w;
            }
            return float.PositiveInfinity;
        }

        private List<CampaignMapNode> Dijkstra(CampaignMapNode from, CampaignMapNode end, Formation formation) {
            
            HashSet<CampaignMapNode> nodes = new HashSet<CampaignMapNode>();
            Dictionary<CampaignMapNode, float> distance = new Dictionary<CampaignMapNode, float>();
            Dictionary<CampaignMapNode, CampaignMapNode> prev = new Dictionary<CampaignMapNode, CampaignMapNode>();
            
            foreach (CampaignMapNode node in this.m_nodes) {
                distance[node] = float.PositiveInfinity;
                prev[node] = null;
                nodes.Add(node);
            }

            distance[from] = 0.0f;

            CampaignMapNode MinDis() {
                float f = float.PositiveInfinity;
                CampaignMapNode q = null;
                foreach (CampaignMapNode n in nodes) {
                    if (distance[n] < f) {
                        q = n;
                    }
                }
                return q;
            }

            while (nodes.Count > 0) {

                CampaignMapNode u = MinDis();
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

            List<CampaignMapNode> path = new List<CampaignMapNode>();
            CampaignMapNode t = end;
            if (prev[t] is not null || t == from) {
                while (t is not null) {
                    path.Insert(0, t);
                    t = prev[t];
                }
            }

            return path;

        }

        private List<CampaignMapNode> GetNeighbours(CampaignMapNode source, IEnumerable<CampaignMapNode> filter) {
            List<CampaignMapNode> neighbours = this.GetNodeNeighbours(source);
            int fcount = filter.Count();
            if (fcount == 0) {
                return neighbours;
            } else {
                return neighbours.Where(x => filter.Contains(x)).ToList();
            }
        }

        public List<CampaignMapNode> GetNodeNeighbours(CampaignMapNode mapNode)
            => this.GetNodeNeighbours(mapNode, x => true);

        public List<CampaignMapNode> GetNodeNeighbours(CampaignMapNode mapNode, Predicate<CampaignMapNode> predicate) {
            List<CampaignMapNode> neighbours = new List<CampaignMapNode>();
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

    }

}
