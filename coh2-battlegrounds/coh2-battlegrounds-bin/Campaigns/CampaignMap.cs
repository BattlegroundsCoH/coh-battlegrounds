using System;
using System.Collections.Generic;
using System.Linq;

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
                var node = new CampaignMapNode(k.Str(), vt["u"] as LuaNumber, vt["v"] as LuaNumber);

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

    }

}
