using Battlegrounds.Campaigns.API;

namespace Battlegrounds.Campaigns.Models {

    /// <summary>
    /// Represents a player in a singleplayer campaign.
    /// </summary>
    public class SingleCampaignPlayer : ICampaignPlayer {
        
        public ICampaignTeam Team { get; }

        /// <summary>
        /// The unique ID identifying the player.
        /// </summary>
        public ulong Uid { get; }

        public string DisplayName { get; }

        public string FactionName { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="display"></param>
        /// <param name="id"></param>
        /// <param name="team"></param>
        /// <param name="faction"></param>
        public SingleCampaignPlayer(string display, ulong id, ICampaignTeam team, string faction) {
            this.Team = team;
            this.Uid = id;
            this.DisplayName = display;
            this.FactionName = faction;
        }

    }

}
