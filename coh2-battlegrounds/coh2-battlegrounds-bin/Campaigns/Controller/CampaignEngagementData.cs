using System.Collections.Generic;

using Battlegrounds.Campaigns.Organisations;
using Battlegrounds.Game;
using Battlegrounds.Game.Database;
using Battlegrounds.Game.Gameplay;

namespace Battlegrounds.Campaigns.Controller {
    
    public struct CampaignEngagementData {
        
        public CampaignArmyTeam attackers;
        public CampaignArmyTeam defenders;

        public Faction attackingFaction;
        public Faction defendingFaction;

        public AIDifficulty[] attackingDifficulties;
        public AIDifficulty[] defendingDifficulties;

        public string[] attackingPlayerNames;
        public string[] defendingPlayerNames;

        public string[] attackingCompanyNames;
        public string[] defendingCompanyNames;

        public List<Squad>[] attackingCompanyUnits;
        public List<Squad>[] defendingCompanyUnits;

        public List<Squad> allSquads;
        public List<Squad> initialSquads;

        public List<Formation> attackingFormations;
        public List<Formation> defendingFormations;

        public Scenario scenario;
        public Wincondition gamemode;
        public int gamemodeValue;
        public CampaignMapNode node;

    }

}
