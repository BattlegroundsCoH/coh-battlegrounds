using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Json;

namespace Battlegrounds.Campaigns.Organisation {
    
    public class Army : IJsonObject {

        public string Name { get; init; }

        public string Description { get; init; }

        public Division[] Divisions { get; init; }

        public Faction Faction { get; init; }

        public CampaignArmyTeam Team { get; }

        public Army(CampaignArmyTeam team) {
            this.Team = team;
        }

        public void NewRegimentTemplate() {

        }

        public string ToJsonReference() => throw new NotSupportedException();

    }

}
