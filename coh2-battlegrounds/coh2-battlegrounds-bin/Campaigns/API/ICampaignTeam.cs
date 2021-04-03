using Battlegrounds.Game.Gameplay;

namespace Battlegrounds.Campaigns.API {

    /// <summary>
    /// 
    /// </summary>
    public enum CampaignArmyTeam {
        TEAM_NEUTRAL,
        TEAM_ALLIES,
        TEAM_AXIS,
    }

    /// <summary>
    /// 
    /// </summary>
    public interface ICampaignTeam {

        /// <summary>
        /// 
        /// </summary>
        CampaignArmyTeam Team { get; }

        /// <summary>
        /// 
        /// </summary>
        ICampaignPlayer[] Players { get; }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="playerIndex"></param>
        /// <param name="name"></param>
        /// <param name="uid"></param>
        void CreatePlayer(int playerIndex, string name, ulong uid);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="faction"></param>
        /// <returns></returns>
        public static CampaignArmyTeam GetArmyTeamFromFaction(string faction)
            => Faction.IsAlliedFaction(faction) ? CampaignArmyTeam.TEAM_ALLIES : CampaignArmyTeam.TEAM_AXIS;

    }

}
