using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Battlegrounds.Campaigns.API;

namespace Battlegrounds.Campaigns.Models {

    /// <summary>
    /// 
    /// </summary>
    public class SingleCampaignPlayer : ICampaignPlayer {
        
        /// <summary>
        /// 
        /// </summary>
        public ICampaignTeam Team { get; }

        /// <summary>
        /// 
        /// </summary>
        public ulong Uid { get; }

        /// <summary>
        /// 
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="display"></param>
        /// <param name="id"></param>
        /// <param name="team"></param>
        public SingleCampaignPlayer(string display, ulong id, ICampaignTeam team) {
            this.Team = team;
            this.Uid = id;
            this.DisplayName = display;
        }

    }

}
