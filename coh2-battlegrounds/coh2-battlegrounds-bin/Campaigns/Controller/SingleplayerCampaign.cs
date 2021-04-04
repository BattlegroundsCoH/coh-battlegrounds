using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Battlegrounds.Campaigns.API;
using Battlegrounds.Campaigns.Map;
using Battlegrounds.Campaigns.Models;
using Battlegrounds.Campaigns.Organisations;
using Battlegrounds.Campaigns.Scripting;
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
using Battlegrounds.Gfx;
using Battlegrounds.Locale;
using Battlegrounds.Lua;
using Battlegrounds.Util.Lists;

namespace Battlegrounds.Campaigns.Controller {

    /// <summary>
    /// 
    /// </summary>
    public class SingleplayerCampaign : ICampaignController {

        private string m_locSourceID;
        private ushort m_unitCampaignID;

        private HashSet<string> m_allowedSummerAtmospheres;
        private HashSet<string> m_allowedWinterAtmospheres;

        private ICampaignEventManager m_eventManager;
        private ICampaignMap m_map;
        private ICampaignScriptHandler m_scriptHandler;
        private ICampaignTurn m_turn;

        private ICampaignTeam[] m_teams;
        private ICampaignPlayer m_player; // The single-player

        private Localize m_locale;

        /// <summary>
        /// 
        /// </summary>
        public ICampaignEventManager Events => this.m_eventManager;

        /// <summary>
        /// 
        /// </summary>
        public ICampaignScriptHandler Script => this.m_scriptHandler;

        /// <summary>
        /// 
        /// </summary>
        public ICampaignMap Map => this.m_map;

        /// <summary>
        /// 
        /// </summary>
        public ICampaignTurn Turn => this.m_turn;

        /// <summary>
        /// 
        /// </summary>
        public Localize Locale => this.m_locale;

        /// <summary>
        /// 
        /// </summary>
        public Army[] Armies { get; init; }

        /// <summary>
        /// 
        /// </summary>
        public List<GfxMap> GfxMaps { get; init; }

        /// <summary>
        /// 
        /// </summary>
        public int DifficultyLevel { get; init; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsSingleplayer => true;

        private SingleplayerCampaign() {
            this.m_unitCampaignID = 0;
        }

        public void Save() {

        }

        public bool EndTurn() {
            bool lastTurn = ICampaignController.GlobalEndTurn(this);
            // More stuff here?
            return lastTurn;
        }

        public void StartCampaign() {

            // Invoke setup function if any
            this.Script.CallGlobal("CampaignSetup");

        }

        public void EndCampaign() {

        }

        public bool IsSelfTurn() => this.m_player.Team.Team == this.m_turn.CurrentTurn;

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
            string DetermineCompanyName(List<Squad> squads, List<ICampaignFormation> formations) {
                Dictionary<ICampaignFormation, int> counts = new Dictionary<ICampaignFormation, int>();
                formations.ForEach(x => {
                    counts.Add(x, squads.Count(x => formations.Any(y => y.Regiments.Any(z => z.HasSquad(x)))));
                });
                int maxVal = counts.Max(x => x.Value);
                ICampaignFormation largestInfluence = counts.FirstOrDefault(x => x.Value == maxVal).Key;
                return this.Locale.GetString(largestInfluence.Regiments.FirstOrDefault().Name);
            }

            // Method for zipping player
            string ZipPlayer(CampaignArmyTeam team, int index) {
                if (index == 0 && team == this.m_player.Team.Team) {
                    return this.m_player.DisplayName;
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

        public void GenerateAIEngagementSetup(ref CampaignEngagementData data, bool isDefence, int armyCount, ICampaignFormation[] availableFormations) {

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

        public ICampaignTeam GetTeam(CampaignArmyTeam teamType) => teamType switch {
            CampaignArmyTeam.TEAM_ALLIES => this.m_teams[0],
            CampaignArmyTeam.TEAM_AXIS => this.m_teams[1],
            _ => null
        };

        public ICampaignPlayer GetSelf() => this.m_player;

        public void CreateArmy(int index, ref uint divCount, CampaignPackage.ArmyData army) {
            var team = army.Army.IsAllied ? CampaignArmyTeam.TEAM_ALLIES : CampaignArmyTeam.TEAM_AXIS;
            this.Armies[index] = new Army(team) {
                Name = this.Locale.GetString(army.Name),
                Description = this.Locale.GetString(army.Desc),
                Faction = army.Army,
            };
            uint tmp = divCount;
            army.FullArmyData?.Pairs((k, v) => {
                switch (k.Str()) {
                    case "templates":
                        (v as LuaTable).Pairs((k, v) => {
                            this.Armies[index].NewRegimentTemplate(k.Str(), v as LuaTable);
                        });
                        break;
                    case "army":
                        var vt = v as LuaTable;
                        this.Armies[index].ArmyName = new LocaleKey(vt["name"].Str(), this.m_locSourceID);
                        (vt["divisions"] as LuaTable).Pairs((k, v) => {
                            var vt = v as LuaTable;
                            uint divID = tmp;

                            this.Armies[index].NewDivision(divID,
                                new LocaleKey(k.Str(), this.m_locSourceID),
                                vt["tmpl"].Str(),
                                vt["max_move"],
                                vt["regiments"] as LuaTable,
                                ref this.m_unitCampaignID);

                            tmp++;
                            if (vt["deploy"] is not LuaNil) {
                                if (vt["deploy"] is LuaTable tableAt) {
                                    this.Map.DeployDivision(divID, this.Armies[index], new List<string>(tableAt.ToArray().Select(x => x.Str())));
                                    return;
                                } else if (vt["deploy"] is LuaString strAt) {
                                    this.Map.DeployDivision(divID, this.Armies[index], new List<string>() { strAt.Str() });
                                    return;
                                } else {
                                    Trace.WriteLine($"Attempted to spawn '{k.Str()}' using unsupported datatype!", nameof(SingleplayerCampaign));
                                }
                            }

                            // If not deployed, add to campaign reserves.
                            this.m_teams[team == CampaignArmyTeam.TEAM_ALLIES ? 0 : 1].AddReserveDivision(this.Armies[index].GetDivision(divID));

                        });
                        break;
                    default:
                        Trace.WriteLine($"Unrecognized army entry '{k.Str()}'", nameof(SingleplayerCampaign));
                        break;
                }
            });
            divCount = tmp;
        }

        public static SingleplayerCampaign FromPackage(CampaignPackage package, int difficulty, CampaignStartData createArgs) {

            // Create initial campaign data
            SingleplayerCampaign campaign = new SingleplayerCampaign {
                m_map = new CampaignMap(package.MapData),
                m_locale = package.LocaleManager,
                m_locSourceID = package.LocaleSourceID,
                m_scriptHandler = new CampaignScriptHandler(),
                m_eventManager = new SingleplayerCampaignEventManager(),
                Armies = new Army[package.CampaignArmies.Length],
                DifficultyLevel = difficulty,
                GfxMaps = package.GfxResources,
            };

            // Create campaign nodes
            campaign.Map.BuildNetwork();

            // Define start team
            CampaignArmyTeam startTeam = ICampaignTeam.GetArmyTeamFromFaction(package.NormalStartingSide);
            var human = createArgs.HumanDataList[0];

            // Determine starting team
            if (human.Team != startTeam) {
                if (startTeam == CampaignArmyTeam.TEAM_AXIS) {
                    startTeam = CampaignArmyTeam.TEAM_ALLIES;
                } else {
                    startTeam = CampaignArmyTeam.TEAM_AXIS;
                }
            }

            // Create team and players
            campaign.m_teams = new ICampaignTeam[] {
                new SingleCampaignTeam(CampaignArmyTeam.TEAM_ALLIES, 1),
                new SingleCampaignTeam(CampaignArmyTeam.TEAM_AXIS, 1)
            };
            campaign.m_teams[0].CreatePlayer(0, human.Team == CampaignArmyTeam.TEAM_ALLIES ? human.Name : string.Empty, human.Team == CampaignArmyTeam.TEAM_ALLIES ? human.SteamID : 0);
            campaign.m_teams[1].CreatePlayer(0, human.Team == CampaignArmyTeam.TEAM_AXIS ? human.Name : string.Empty, human.Team == CampaignArmyTeam.TEAM_AXIS ? human.SteamID : 0);

            // Set default player
            campaign.m_player = campaign.m_teams[human.Team == CampaignArmyTeam.TEAM_ALLIES ? 0 : 1].Players[0];

            // Counter to keep track of diviions
            uint divisionCount = 0;

            // Create the armies
            for (int i = 0; i < package.CampaignArmies.Length; i++) {
                campaign.CreateArmy(i, ref divisionCount, package.CampaignArmies[i]);
            }

            // Create turn data
            campaign.m_turn = new SingleplayerCampaignTurn(startTeam, new[] {
                package.CampaignTurnData.Start,
                package.CampaignTurnData.End
            }, package.CampaignTurnData.TurnLength);
            campaign.Turn.SetWinterDates(new[] { package.CampaignWeatherData.WinterStart, package.CampaignWeatherData.WinterEnd });

            // Set atmospheres
            campaign.m_allowedSummerAtmospheres = package.CampaignWeatherData.SummerAtmospheres;
            campaign.m_allowedWinterAtmospheres = package.CampaignWeatherData.WinterAtmospheres;

            // Set lua stuff
            campaign.Script.SetGlobal("Map", campaign.Map, true);
            campaign.Script.SetGlobal("Turn", campaign.Turn, true);
            campaign.Script.SetGlobal(BattlegroundsCampaignLibrary.CampaignInstanceField, campaign, false);

            // Register library
            BattlegroundsCampaignLibrary.LoadLibrary(campaign.Script.ScriptState);

            // Loop over campaign scripts and init them
            package.CampaignScripts.ForEach(x => campaign.Script.LoadScript(x));

            // Assign state ptrs
            campaign.Map.ScriptHandler = campaign.Script;
            campaign.Events.ScriptHandler = campaign.Script;

            // Return campaign
            return campaign;

        }

        private static void PopulateListWithFormations(WeightedList<Squad> squads, bool isDefence, ICampaignFormation[] formations) {

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
