using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Battlegrounds.Campaigns.Organisations;
using Battlegrounds.Functional;
using Battlegrounds.Game.DataCompany;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Game.Match;

namespace Battlegrounds.Campaigns.Controller {
    
    /// <summary>
    /// 
    /// </summary>
    public interface ICampaignController {

        /// <summary>
        /// 
        /// </summary>
        ActiveCampaign Campaign { get; }

        /// <summary>
        /// 
        /// </summary>
        bool IsSingleplayer { get; }

        /// <summary>
        /// 
        /// </summary>
        void Save();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        bool EndTurn();

        /// <summary>
        /// 
        /// </summary>
        void EndCampaign();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="player"></param>
        /// <param name="playername"></param>
        /// <param name="playerTeam"></param>
        void CreatePlayer(ulong player, string playername, CampaignArmyTeam playerTeam);

        /// <summary>
        /// Initialize a <see cref="MatchController"/> object that is ready to be controlled.
        /// </summary>
        /// <param name="data">The engagement data to use while generating scenario data</param>
        /// <returns>A ready-to-use <see cref="MatchController"/> instance.</returns>
        MatchController Engage(CampaignEngagementData data);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        void ZipPlayerData(ref CampaignEngagementData data);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="isDefence"></param>
        /// <param name="armyCount"></param>
        /// <param name="availableFormations"></param>
        void GenerateAIEngagementSetup(ref CampaignEngagementData data, bool isDefence, int armyCount, Formation[] availableFormations);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="isAttacker"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static Company GetCompanyFromEngagementData(CampaignEngagementData data, bool isAttacker, int index) {

            // Get squad pool
            List<Squad> units = isAttacker ? data.attackingCompanyUnits[index] : data.defendingCompanyUnits[index];

            // Create company
            CompanyBuilder builder = new CompanyBuilder()
                .NewCompany(isAttacker ? data.attackingFaction : data.defendingFaction)
                .ChangeTuningMod(BattlegroundsInstance.BattleGroundsTuningMod.Guid)
                .ChangeName(isAttacker ? data.attackingCompanyNames[index] : data.defendingCompanyNames[index])
                .ChangeUser(isAttacker ? data.attackingPlayerNames[index] : data.defendingPlayerNames[index]);

            // If no deployment phase is set, auto-generate it
            if (units.TrueForAll(x => x.DeploymentPhase == DeploymentPhase.PhaseNone)) {
                DeploymentPhase phase = DeploymentPhase.PhaseInitial;
                int count = 0;
                for (int i = 0; i < units.Count; i++) {
                    var uBld = new UnitBuilder(units[i], false);
                    uBld.SetDeploymentPhase(phase);
                    count++;
                    if (count > 4 && phase == DeploymentPhase.PhaseInitial) {
                        phase = DeploymentPhase.PhaseA;
                        count = 0;
                    } else if (count > 12 && phase == DeploymentPhase.PhaseA) {
                        phase = DeploymentPhase.PhaseB;
                        count = 0;
                    } else if (count > 12 && phase == DeploymentPhase.PhaseB) {
                        phase = DeploymentPhase.PhaseC;
                    }
                    builder.AddUnit(uBld);
                }
            }

            // Commit and return company
            return builder.Commit().Result;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="company"></param>
        public static void HandleCompanyChanges(CampaignEngagementData data, Company company) { // Note: data is value type, but the fields affected are ref types
            
            // Determine if company was attacking or defending
            bool isAttacker = data.attackingPlayerNames.Contains(company.Owner);
            if (!isAttacker && !data.defendingPlayerNames.Contains(company.Owner)) {
                Trace.WriteLine($"Failed to map company owned by '{company.Owner}' to an attacking or defending player/formation", nameof(ICampaignController));
                return;
            } else {
                Trace.WriteLine($"Handling company '{company.Name}' owned by '{company.Owner}' on {(isAttacker ? "attacking" : "defending")} side.", nameof(ICampaignController));
            }
            
            // Loop through all units and update accordingly
            /*company.Units.ForEach(x => {
                if (isAttacker) {

                } else {

                }
            });*/

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="data"></param>
        /// <param name="node"></param>
        public static void HandleEngagement(MatchController controller, CampaignEngagementData data, Action<CampaignMapNode, bool> node) {

            // Subscribe to completion event
            controller.Complete += res => {

                // Loop through unit statuses and register dead units
                var deadUnits = new List<Squad>();
                res.Units.ForEach(u => {
                    if (u.IsDead) {
                        if (data.allParticipatingSquads.FirstOrDefault(s => s.SquadID == u.UnitID) is Squad unit) {
                            deadUnits.Add(unit);
                            Trace.WriteLine($"Found unit with ID {u.UnitID} in engaged unit list that has been marked as dead.", nameof(ICampaignController));
                        } else {
                            Trace.WriteLine($"Failed to find unit with ID {u.UnitID} in engaged unit list. This may lead to incorrect death counts!", nameof(ICampaignController));
                        }
                    }
                });

                // Formation updater
                void UpdateFormations(Formation f) {
                    f.Regiments.ForEach(x => {
                        x.KillSquads(deadUnits);
                    });
                }

                // Update formations
                data.attackingFormations.ForEach(UpdateFormations);
                data.defendingFormations.ForEach(UpdateFormations);

                // Log
                Trace.WriteLine($"Removed {deadUnits.Count} squads from engaged formations (of {res.Units.Count} detected units).", nameof(ICampaignController));

                var lookatPlayer = res.Players.First();
                bool wasAttackerSuccessful = res.IsWinner(lookatPlayer);
                if (wasAttackerSuccessful && !data.attackingPlayerNames.Contains(lookatPlayer.Name)) {
                    wasAttackerSuccessful = false;
                }

                // Log outcome
                Trace.WriteLine($"The engagement resulted in a {(wasAttackerSuccessful ? "WIN" : "LOSS")} for the attacking side.", nameof(ICampaignController));

                // Tell UI it can now update
                node?.Invoke(data.node, wasAttackerSuccessful);

            };

        }

    }

}
