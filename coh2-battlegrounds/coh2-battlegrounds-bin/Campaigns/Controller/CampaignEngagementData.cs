using System;
using System.Collections.Generic;

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

        public List<Squad>[] attackingCompanyUnits;
        public List<Squad>[] defendingCompanyUnits;

        public Scenario scenario;

    }

}
