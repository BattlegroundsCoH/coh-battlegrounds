using System;

using Battlegrounds.Campaigns.Organisation;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Json;
using Battlegrounds.Locale;

namespace Battlegrounds.Campaigns {

    public enum CampaignArmyTeam {
        TEAM_NEUTRAL,
        TEAM_ALLIES,
        TEAM_AXIS,
    }

    public class ActiveCampaign : IJsonObject {

        public class Player { }

        public CampaignMap PlayMap { get; init; }

        public Army[] Armies { get; init; }

        public Localize Locale { get; init; }

        public ActiveCampaignTurnData Turn { get; private set; }

        public int DifficultyLevel { get; init; }

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

        public static ActiveCampaign FromPackage(CampaignPackage package, CampaignMode mode, int difficulty, ActiveCampaignStartData createArgs) {

            // Create initial campaign data
            ActiveCampaign campaign = new ActiveCampaign {
                PlayMap = new CampaignMap(package.MapData),
                Armies = new Army[package.CampaignArmies.Length],
                Locale = package.LocaleManager,
                DifficultyLevel = difficulty,
            };

            // Create campaign nodes
            campaign.PlayMap.BuildNetwork();

            // Create the armies
            for (int i = 0; i < package.CampaignArmies.Length; i++) {
                campaign.CreateArmy(i, package.CampaignArmies[i]);
            }

            // Define start team
            CampaignArmyTeam startTeam = Faction.IsAlliedFaction(package.NormalStartingSide) ? CampaignArmyTeam.TEAM_ALLIES : CampaignArmyTeam.TEAM_AXIS;

            // Create mode-specific data
            if (mode == CampaignMode.Singleplayer) {

                // Determine starting team
                if (createArgs.PlayAs != package.NormalStartingSide) {
                    if (startTeam == CampaignArmyTeam.TEAM_AXIS) {
                        startTeam = CampaignArmyTeam.TEAM_ALLIES;
                    } else {
                        startTeam = CampaignArmyTeam.TEAM_AXIS;
                    }
                }

            } else {
                // TODO: Stuff
            }

            // Create turn data
            campaign.Turn = new ActiveCampaignTurnData(startTeam, new[] {
                package.CampaignTurnData.Start,
                package.CampaignTurnData.End
            }, package.CampaignTurnData.TurnLength);

            return campaign;

        }

    }

}
