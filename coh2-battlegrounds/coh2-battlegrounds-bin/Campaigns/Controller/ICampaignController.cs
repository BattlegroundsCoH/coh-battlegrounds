using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Battlegrounds.Campaigns.Controller {
    
    public interface ICampaignController {

        ActiveCampaign Campaign { get; }

        bool IsSingleplayer { get; }

        void Save();

        bool EndTurn();

        void EndCampaign();

    }

}
