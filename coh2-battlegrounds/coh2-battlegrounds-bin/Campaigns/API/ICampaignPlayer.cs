using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Battlegrounds.Campaigns.API {
    
    public interface ICampaignPlayer {

        /// <summary>
        /// 
        /// </summary>
        ICampaignTeam Team { get; }

        /// <summary>
        /// 
        /// </summary>
        string DisplayName { get; }

    }

}
