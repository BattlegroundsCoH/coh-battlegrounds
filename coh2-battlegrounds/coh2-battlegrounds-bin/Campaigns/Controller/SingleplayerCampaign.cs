using System;
using System.Collections.Generic;

using Battlegrounds.Campaigns.Organisations;
using Battlegrounds.Functional;
using Battlegrounds.Game;
using Battlegrounds.Game.Database;
using Battlegrounds.Game.DataCompany;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Game.Match;
using Battlegrounds.Game.Match.Analyze;
using Battlegrounds.Game.Match.Composite;
using Battlegrounds.Game.Match.Finalizer;
using Battlegrounds.Game.Match.Play.Factory;
using Battlegrounds.Game.Match.Startup;
using Battlegrounds.Util.Lists;

namespace Battlegrounds.Campaigns.Controller {

    public class SingleplayerCampaign : ICampaignController {

        public ActiveCampaign Campaign { get; }

        public bool IsSingleplayer => true;

        public SingleplayerCampaign(ActiveCampaign campaign) {
            this.Campaign = campaign;
        }

        public void Save() {

        }

        public bool EndTurn() => this.Campaign.Turn.EndTurn();

        public void EndCampaign() {

        }

        public MatchController Engage(CampaignEngagementData data) {

            // Create strategies
            SingleplayerStartupStrategy singleplayerStartupStrategy = new() {
                LocalCompanyCollector = () => GetCompanyFromEngagementData(data, true, 0),
                SessionInfoCollector = () => GetSessionFromEngagementData(data),
                PlayStrategyFactory = new OverwatchStrategyFactory(),
            };
            SingleplayerMatchAnalyzer singleplayerMatchAnalyzer = new();
            SingleplayerFinalizer singleplayerFinalizer = new() { 
            };

            // Create session handler
            SingleplayerSession session = new SingleplayerSession();

            // Setup controller
            MatchController controller = new MatchController();
            controller.SetStartupObjects(session, singleplayerStartupStrategy);
            controller.SetAnalysisObjects(session, singleplayerMatchAnalyzer);
            controller.SetFinalizerObjects(session, singleplayerFinalizer);

            // Return the controller
            return controller;

        }

        public void GenerateAIEngagementSetup(ref CampaignEngagementData data, bool isDefence, int armyCount, Formation[] availableFormations) {

            // RefPtr to lists that will be affected
            List<Squad>[] targetList;

            // Determine amount and target list
            if (isDefence) {

                // Get count
                int defenderCount = Math.Min(armyCount, data.attackingCompanyUnits.Length);

                // Pair up evenly
                data.defendingCompanyUnits = new List<Squad>[defenderCount];
                data.defendingDifficulties = new AIDifficulty[defenderCount];
                Array.Fill(data.defendingDifficulties, AIDifficulty.AI_Hard);

                targetList = data.defendingCompanyUnits;
                

            } else {

                // Set to amount of armies
                data.attackingCompanyUnits = new List<Squad>[armyCount];
                data.attackingDifficulties = new AIDifficulty[armyCount];
                Array.Fill(data.attackingDifficulties, AIDifficulty.AI_Hard);

                targetList = data.attackingCompanyUnits;

            }

            // Generate weighted list
            WeightedList<Squad> options = new WeightedList<Squad>();
            PopulateListWithFormations(options, isDefence, availableFormations);

            // Define a max distribution (Incase theres less squads than max available for each player)
            int distribution = options.Count / targetList.Length;

            // Loop through each defending company
            for (int i = 0; i < targetList.Length; i++) {

                // Create list
                targetList[i] = new List<Squad>();

                // Pick at random
                while (targetList[i].Count < Company.MAX_SIZE && targetList[i].Count < distribution) {

                    // Add random
                    targetList[i].Add(options.Pick(BattlegroundsInstance.RNG));

                }

            }

        }

        private static void PopulateListWithFormations(WeightedList<Squad> squads, bool isDefence, Formation[] formations) {

            // Run through all formations
            for (int i = 0; i < formations.Length; i++) {

                // Loop through all regiments
                formations[i].Regiments.ForEach(x => {
                    x.FirstCompany().Units.ForEach(x => {
                        double priority = 0.05;
                        if (x.SBP.IsAntiTank) {
                            priority += isDefence ? 0.7 : 0.2;
                        } else if (x.SBP.IsCommandUnit) {
                            priority += isDefence ? 0.6 : 0.4;
                        } else if (x.SBP.IsInfantry) {
                            priority += isDefence ? 0.8 : 0.6;
                        } else if (x.SBP.IsVehicle) {
                            priority += isDefence ? 0.3 : 0.5;
                        }
                        squads.Add(x, priority);
                    });
                });

            }

        }

        private static Company GetCompanyFromEngagementData(CampaignEngagementData data, bool isAttacker, int index) {
            List<Squad> units = isAttacker ? data.attackingCompanyUnits[index] : data.defendingCompanyUnits[index];
            CompanyBuilder builder = new CompanyBuilder()
                .NewCompany(isAttacker ? data.attackingFaction : data.defendingFaction)
                .ChangeTuningMod(BattlegroundsInstance.BattleGroundsTuningMod.Guid)
                .ChangeName("UNTITLED COMPANY")
                .ChangeUser(BattlegroundsInstance.Steam.User.Name);
            DeploymentPhase phase = DeploymentPhase.PhaseInitial; // TEMP FIX: Should be set while creating the regiment
            int count = 0;
            for (int i = 0; i < units.Count; i++) {
                var uBld = new UnitBuilder(units[i], false);
                uBld.SetDeploymentPhase(phase);
                count++;
                if (count > 4 && phase == DeploymentPhase.PhaseInitial) {
                    phase = DeploymentPhase.PhaseA;
                    count = 0;
                } else if (count > 6 && phase == DeploymentPhase.PhaseA) {
                    phase = DeploymentPhase.PhaseB;
                    count = 0;
                } else if (count > 8 && phase == DeploymentPhase.PhaseB) {
                    phase = DeploymentPhase.PhaseC;
                }
                builder.AddUnit(uBld);
            }
            return builder.Commit().Result;
        }

        private static SessionInfo GetSessionFromEngagementData(CampaignEngagementData data) {
            SessionParticipant[] CreateTeam(bool isAttacker, List<Squad>[] companies, SessionParticipantTeam team) {
                SessionParticipant[] participants = new SessionParticipant[companies.Length];
                var diffs = isAttacker ? data.attackingDifficulties : data.defendingDifficulties;
                for (int i = 0; i < companies.Length; i++) {
                    var company = GetCompanyFromEngagementData(data, isAttacker, i);
                    if (diffs[i] == AIDifficulty.Human) {
                        var usr = BattlegroundsInstance.Steam.User;
                        participants[i] = new SessionParticipant(usr.Name, usr.ID, company, team, (byte)i);
                    } else {
                        participants[i] = new SessionParticipant(diffs[i], company, team, (byte)i);
                    }
                }
                return participants;
            }
            bool areAlliesAttacking = data.attackers == CampaignArmyTeam.TEAM_ALLIES;
            SessionInfo info = new SessionInfo() {
                SelectedScenario = data.scenario,
                SelectedGamemode = WinconditionList.GetWinconditionByName(WinconditionList.VictoryPoints),
                SelectedGamemodeOption = 0,
                SelectedTuningMod = BattlegroundsInstance.BattleGroundsTuningMod,
                DefaultDifficulty = AIDifficulty.AI_Hard,
                FillAI = false,
                IsOptionValue = false,
                Allies = CreateTeam(areAlliesAttacking, areAlliesAttacking ? data.attackingCompanyUnits : data.defendingCompanyUnits, SessionParticipantTeam.TEAM_ALLIES),
                Axis = CreateTeam(!areAlliesAttacking, !areAlliesAttacking ? data.attackingCompanyUnits : data.defendingCompanyUnits, SessionParticipantTeam.TEAM_AXIS)
            };
            return info;
        }

    }

}
