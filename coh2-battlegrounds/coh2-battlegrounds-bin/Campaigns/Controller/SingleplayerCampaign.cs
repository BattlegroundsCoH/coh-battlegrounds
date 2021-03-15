using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        private class Player {
            public ulong id;
            public string name;
            public CampaignArmyTeam team;
        }

        private Player m_player;

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

        public void CreatePlayer(ulong player, string playername, CampaignArmyTeam playerTeam) {
            this.m_player = new Player() {
                id = player,
                name = playername,
                team = playerTeam
            };
        }

        public MatchController Engage(CampaignEngagementData data) {

            // Make sure we have a list
            if (data.allParticipatingSquads is null && data.allParticipatingSquads.Count == 0) {
                throw new InvalidDataException();
            }

            // Aggregate
            data.attackingCompanyUnits.ForEach(x => data.allParticipatingSquads.AddRange(x));
            data.defendingCompanyUnits.ForEach(x => data.allParticipatingSquads.AddRange(x));

            // Create strategies
            SingleplayerStartupStrategy singleplayerStartupStrategy = new() {
                LocalCompanyCollector = () => ICampaignController.GetCompanyFromEngagementData(data, true, 0),
                SessionInfoCollector = () => GetSessionFromEngagementData(data),
                PlayStrategyFactory = new OverwatchStrategyFactory(),
            };
            SingleplayerMatchAnalyzer singleplayerMatchAnalyzer = new();
            SingleplayerFinalizer singleplayerFinalizer = new() { 
                NotifyAI = true,
                CompanyHandler = x => {
                    ICampaignController.HandleCompanyChanges(data, x);
                }
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

        public void ZipPlayerData(ref CampaignEngagementData data) {

            // Method for determining company name
            string DetermineCompanyName(List<Squad> squads, List<Formation> formations) {
                Dictionary<Formation, int> counts = new Dictionary<Formation, int>();
                formations.ForEach(x => {
                    counts.Add(x, squads.Count(x => formations.Any(y => y.Regiments.Any(z => z.HasSquad(x)))));
                });
                int maxVal = counts.Max(x => x.Value);
                Formation largestInfluence = counts.FirstOrDefault(x => x.Value == maxVal).Key;
                return this.Campaign.Locale.GetString(largestInfluence.Regiments.FirstOrDefault().Name);
            }

            // Method for zipping player
            string ZipPlayer(CampaignArmyTeam team, int index) {
                if (index == 0 && team == this.m_player.team) {
                    return this.m_player.name;
                } else {
                    return AIDifficulty.AI_Hard.GetIngameDisplayName();
                }
            }

            // Alloc company name arrays
            data.attackingCompanyNames = new string[data.attackingCompanyUnits.Length];
            data.defendingCompanyNames = new string[data.defendingCompanyUnits.Length];

            // Allow company player name arrays
            data.attackingPlayerNames = new string[data.attackingCompanyUnits.Length];
            data.defendingPlayerNames = new string[data.defendingCompanyUnits.Length];

            // Setup attackers
            for (int i = 0; i < data.attackingPlayerNames.Length; i++) {
                data.attackingCompanyNames[i] = DetermineCompanyName(data.attackingCompanyUnits[i], data.attackingFormations);
                data.attackingPlayerNames[i] = ZipPlayer(data.attackers, i);
            }

            // Setup defenders
            for (int i = 0; i < data.defendingPlayerNames.Length; i++) {
                data.defendingCompanyNames[i] = DetermineCompanyName(data.defendingCompanyUnits[i], data.defendingFormations);
                data.defendingPlayerNames[i] = ZipPlayer(data.defenders, i);
            }

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
                        } else if (x.SBP.IsInfantry) {
                            priority += isDefence ? 0.8 : 0.6;
                        } else if (x.SBP.IsVehicle) {
                            priority += isDefence ? 0.3 : 0.5;
                        }
                        if (x.SBP.IsCommandUnit) {
                            priority = 1.0;
                        }
                        squads.Add(x, priority);
                    });
                });

            }

        }

        private static SessionInfo GetSessionFromEngagementData(CampaignEngagementData data) {
            SessionParticipant[] CreateTeam(bool isAttacker, List<Squad>[] companies, SessionParticipantTeam team) {
                SessionParticipant[] participants = new SessionParticipant[companies.Length];
                var diffs = isAttacker ? data.attackingDifficulties : data.defendingDifficulties;
                for (int i = 0; i < companies.Length; i++) {
                    var company = ICampaignController.GetCompanyFromEngagementData(data, isAttacker, i);
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
                SelectedGamemode = WinconditionList.GetWinconditionByName(WinconditionList.VictoryPoints), // TODO: Set according to CampaignEngagementData
                SelectedGamemodeOption = 50, // TODO: Set according to CampaignEngagementData
                SelectedTuningMod = BattlegroundsInstance.BattleGroundsTuningMod,
                DefaultDifficulty = AIDifficulty.AI_Hard,
                FillAI = false,
                IsOptionValue = true, // TODO: Set according to CampaignEngagementData
                Allies = CreateTeam(areAlliesAttacking, areAlliesAttacking ? data.attackingCompanyUnits : data.defendingCompanyUnits, SessionParticipantTeam.TEAM_ALLIES),
                Axis = CreateTeam(!areAlliesAttacking, !areAlliesAttacking ? data.attackingCompanyUnits : data.defendingCompanyUnits, SessionParticipantTeam.TEAM_AXIS)
            };
            return info;
        }

    }

}
