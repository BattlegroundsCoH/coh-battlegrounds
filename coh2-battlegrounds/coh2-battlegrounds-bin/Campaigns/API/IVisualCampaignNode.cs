using Battlegrounds.Campaigns.Organisations;

namespace Battlegrounds.Campaigns.API {
    
    public interface IVisualCampaignNode {

        void OwnershipChanged(CampaignArmyTeam newOwner);

        void VictoryValueChanged(double newValue);

        void AttritionValueChanged(double newValue);

        void OccupantAdded(ICampaignFormation formation);

        void OccupantRemoved(ICampaignFormation formation);

    }

}
