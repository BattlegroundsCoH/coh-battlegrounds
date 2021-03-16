using System.Windows;
using Battlegrounds.Campaigns.Organisations;

namespace BattlegroundsApp.Views.CampaignViews.Models {
    
    public class CampaignUnitFormationModel : ICampaignMapVisual {
    
        public Formation Formation { get; }

        public UIElement VisualElement { get; }

        public CampaignUnitFormationModel(UIElement element, Formation formation) {
            this.VisualElement = element;
            this.Formation = formation;
        }

        public static UIElement CreateElement() => null;

    }

}
