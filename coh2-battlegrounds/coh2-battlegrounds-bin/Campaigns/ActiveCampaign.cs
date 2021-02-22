using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Battlegrounds.Campaigns.Organisation;
using Battlegrounds.Functional;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Json;
using Battlegrounds.Locale;

namespace Battlegrounds.Campaigns {
    
    public class ActiveCampaign : IJsonObject {

        public class Player { }

        public CampaignMap PlayMap { get; init; }

        public Army[] Armies { get; init; }

        public Localize Locale { get; init; }

        private ActiveCampaign() { 
        }

        public void CreateArmy(int index, CampaignPackage.ArmyData army) {
            this.Armies[index] = new Army(army.Army.IsAllied ? CampaignArmyTeam.TEAM_ALLIES : CampaignArmyTeam.TEAM_AXIS) {
                Name = this.Locale.GetString(army.Name),
                Description = this.Locale.GetString(army.Desc),
                Faction = army.Army,
            };
            army.FullArmyData?.Pairs((k, v) => {

            });
        }

        public void NewPlayer() {

        }

        public string ToJsonReference() => throw new NotSupportedException();

        public static ActiveCampaign FromPackage(CampaignPackage package, CampaignMode mode, int difficulty, string playas, object createArgs = null) {

            // Create initial campaign data
            ActiveCampaign campaign = new ActiveCampaign {
                PlayMap = new CampaignMap(package.MapData),
                Armies = new Army[package.CampaignArmies.Length],
                Locale = package.LocaleManager
            };

            // Create campaign nodes
            campaign.PlayMap.BuildNetwork();

            // Create the armies
            for (int i = 0; i < package.CampaignArmies.Length; i++) {
                campaign.CreateArmy(i, package.CampaignArmies[i]);
            }

            // Create mode-specific data
            if (mode == CampaignMode.Singleplayer) {



            } else {
                // TODO: Stuff
            }

            return campaign;

        }

    }

}
