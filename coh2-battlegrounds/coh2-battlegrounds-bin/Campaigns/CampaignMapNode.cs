using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Battlegrounds.Campaigns {
    
    public class CampaignMapNode {

        public double U { get; }

        public double V { get; }

        public string NodeName { get; }

        public object VisualNode { get; set; }

        public CampaignMapNode(string name, double u, double v) {
            this.NodeName = name;
            this.U = u;
            this.V = v;
            this.VisualNode = null;
        }

    }

}
