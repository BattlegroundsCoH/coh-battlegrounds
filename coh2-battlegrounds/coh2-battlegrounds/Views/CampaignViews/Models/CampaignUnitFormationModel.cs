using System.Windows;
using Battlegrounds.Campaigns.API;

namespace BattlegroundsApp.Views.CampaignViews.Models {
    
    public class CampaignUnitFormationModel : ICampaignMapVisual {
    
        public ICampaignFormation Formation { get; }

        public UIElement VisualElement { get; }

        public CampaignUnitFormationModel(UIElement element, ICampaignFormation formation) {
            this.VisualElement = element;
            this.Formation = formation;
        }

        public static UIElement CreateElement() => null;

    }

}
