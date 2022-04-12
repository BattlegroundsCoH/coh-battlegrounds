using System.Linq;
using System.Windows.Input;
using System.Windows.Media;

using Battlegrounds.Campaigns.Organisations;

namespace BattlegroundsApp.Views.CampaignViews.Models {

    public class CampaignUnitReserveModel : ICampaignSelectable {

        public Division Reserve { get; }

        public ImageSource ReserveIcon { get; }

        public ICommand ReserveClicked { get; set; }

        public ICommand ReserveDeployed { get; set; }

        public CampaignResourceContext ResourceContext { get; }

        public string Title => this.ResourceContext.GetString(this.Reserve.Name);

        public string Description => "No description available";

        public CampaignUnitReserveModel(Division division, CampaignResourceContext resourceContext) {

            // Set properties
            this.Reserve = division;
            this.ResourceContext = resourceContext;

            // Get display icon
            if (resourceContext.GetResource($"{division.Name.LocaleID}_icon") is ImageSource source) {
                this.ReserveIcon = source;
            } else {
                this.ReserveIcon = null; // TODO: Map to default
            }

        }

        public CampaignSelectableInfoSection[] GetInfoSections()
            => CampaignUnitFormationModel.GetUnitData(this.Reserve.Regiments.Where(x => !x.IsDeployed), this.ResourceContext).ToArray();

    }

}
