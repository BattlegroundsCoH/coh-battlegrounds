using System.Collections.Generic;
using Battlegrounds.Campaigns.API;

namespace Battlegrounds.Campaigns {
    
    /// <summary>
    /// 
    /// </summary>
    public class CampaignStartData {

        /// <summary>
        /// 
        /// </summary>
        public readonly struct HumanData {
            public string Name { get; init; }
            public ulong SteamID { get; init; }
            public CampaignArmyTeam Team { get; init; }
            public string Faction { get; init; }
        }

        private List<HumanData> m_humans;

        /// <summary>
        /// 
        /// </summary>
        public int HumanAxisPlayers { get; init; }

        /// <summary>
        /// 
        /// </summary>
        public int HumanAlliesPlayers { get; init; }

        /// <summary>
        /// 
        /// </summary>
        public List<HumanData> HumanDataList => this.m_humans;

        /// <summary>
        /// 
        /// </summary>
        public CampaignStartData() {
            this.m_humans = new List<HumanData>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="id"></param>
        /// <param name="faction"></param>
        /// <param name="team"></param>
        public void AddHuman(string name, ulong id, string faction, CampaignArmyTeam team) 
            => this.m_humans.Add(new HumanData() { Name = name, SteamID = id, Faction = faction, Team = team });

    }

}
