using System.Collections.Generic;
using Battlegrounds.Campaigns.API;
using Battlegrounds.Campaigns.Organisations;
using Battlegrounds.Util.Lists;

namespace Battlegrounds.Campaigns.Models {
    
    /// <summary>
    /// Represents a team in a singleplayer campaign context.
    /// </summary>
    public class SingleCampaignTeam : ICampaignTeam {

        private int m_victoryValue;
        private DistinctList<Regiment> m_reserves;

        public CampaignArmyTeam Team { get; }

        public ICampaignPlayer[] Players { get; }

        public int VictoryPoints => this.m_victoryValue;

        /// <summary>
        /// Initialise a new <see cref="SingleCampaignTeam"/> class with a <see cref="CampaignArmyTeam"/> and space for specified amount of players.
        /// </summary>
        /// <param name="team">The team to be represented.</param>
        /// <param name="players">The amount of players that can be on the team.</param>
        public SingleCampaignTeam(CampaignArmyTeam team, int players) {
            this.Team = team;
            this.Players = new SingleCampaignPlayer[players];
            this.m_reserves = new DistinctList<Regiment>();
            this.m_victoryValue = 0;
        }

        public void CreatePlayer(int playerIndex, string name, ulong uid)
            => this.Players[playerIndex] = new SingleCampaignPlayer(name, uid, this);

        public void AddReserveArmy(Army army)
            => army.Divisions.ForEach(AddReserveDivision);

        public void AddReserveDivision(Division division)
            => division.Regiments.ForEach(AddReserveRegiment);

        public void AddReserveRegiment(Regiment regiment)
            => this.m_reserves.Add(regiment);

        public List<Regiment> GetReserves()
            => this.m_reserves;

    }

}
