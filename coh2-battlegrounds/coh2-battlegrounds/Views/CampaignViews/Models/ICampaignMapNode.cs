using System;
using System.Collections.Generic;
using Battlegrounds.Campaigns;

namespace BattlegroundsApp.Views.CampaignViews.Models {
    
    public interface ICampaignMapNode : ICampaignMapVisual {

        CampaignMapNode Node { get; }

        event Action<CampaignMapNode, bool> NodeClicked;

        void ResetOffset();

        (double x, double y) GetNextOffset(bool increment);

        (double x, double y) GetRelative(double x, double y);

    }

}
