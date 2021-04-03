using Battlegrounds.Campaigns.API;

namespace Battlegrounds.Campaigns.Models {
    
    /// <summary>
    /// 
    /// </summary>
    public class SingleCampaignTeam : ICampaignTeam {

        /// <summary>
        /// 
        /// </summary>
        public CampaignArmyTeam Team { get; }

        /// <summary>
        /// 
        /// </summary>
        public ICampaignPlayer[] Players { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="team"></param>
        /// <param name="players"></param>
        public SingleCampaignTeam(CampaignArmyTeam team, int players) {
            this.Team = team;
            this.Players = new SingleCampaignPlayer[players];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="playerIndex"></param>
        /// <param name="name"></param>
        /// <param name="uid"></param>
        public void CreatePlayer(int playerIndex, string name, ulong uid)
            => this.Players[playerIndex] = new SingleCampaignPlayer(name, uid, this);

    }

}
