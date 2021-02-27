using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using Battlegrounds.Campaigns.Organisations;

namespace BattlegroundsApp.Views.CampaignViews.Models {
    
    public class CampaignUnitFormationModel {
    
        public Formation Formation { get; }

        public UIElement Element { get; }

        public CampaignUnitFormationModel(UIElement element, Formation formation) {
            this.Element = element;
            this.Formation = formation;
        }

        public static UIElement CreateElement() => null;

    }

}
