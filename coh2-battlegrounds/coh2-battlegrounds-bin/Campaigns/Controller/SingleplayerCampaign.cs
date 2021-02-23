using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Battlegrounds.Campaigns.Controller {

    public class SingleplayerCampaign : ICampaignController {

        public ActiveCampaign Campaign { get; }

        public bool IsSingleplayer => true;

        public SingleplayerCampaign(ActiveCampaign campaign) {
            this.Campaign = campaign;
        }

        public void Save() {

        }

        public bool EndTurn() => this.Campaign.Turn.EndTurn();

        public void EndCampaign() {

        }

    }

}
