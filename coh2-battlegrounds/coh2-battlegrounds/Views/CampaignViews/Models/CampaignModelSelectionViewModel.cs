using System.Collections.Generic;

namespace BattlegroundsApp.Views.CampaignViews.Models {

    public class CampaignModelSelectionViewModel {

        public string SelectedTitle => this.SelectedObject.Title;

        public string SelectedDesc => this.SelectedObject.Description;

        public List<CampaignSelectableInfoSection> InfoSections { get; }

        public ICampaignSelectable SelectedObject { get; }

        public CampaignModelSelectionViewModel(ICampaignSelectable selectable) {
            this.SelectedObject = selectable;
            this.InfoSections = new List<CampaignSelectableInfoSection>(selectable.GetInfoSections());
        }

    }

}
