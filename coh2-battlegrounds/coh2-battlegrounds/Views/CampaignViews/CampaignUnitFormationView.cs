using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using Battlegrounds.Campaigns.Organisations;

namespace BattlegroundsApp.Views.CampaignViews {
    
    public class CampaignUnitFormationView {
    
        public Formation Formation { get; }

        public UIElement Element { get; }

        public CampaignUnitFormationView(UIElement element, Formation formation) {
            this.Element = element;
            this.Formation = formation;
        }

    }

}
