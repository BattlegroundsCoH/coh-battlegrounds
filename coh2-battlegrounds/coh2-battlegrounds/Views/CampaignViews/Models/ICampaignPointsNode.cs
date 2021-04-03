using System;
using Battlegrounds.Campaigns.API;

namespace BattlegroundsApp.Views.CampaignViews.Models {
    
    public interface ICampaignPointsNode : ICampaignMapVisual {

        ICampaignMapNode Node { get; }

        event Action<ICampaignMapNode, bool> NodeClicked;

        void ResetOffset();

        (double x, double y) GetNextOffset(bool increment);

        (double x, double y) GetRelative(double x, double y);

    }

}
