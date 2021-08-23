using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Battlegrounds.Campaigns.AI;
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
using Battlegrounds.Modding;
using Battlegrounds.Util.Coroutines;
using Battlegrounds.Util.Lists;

namespace Battlegrounds.Campaigns.Controller {

    /// <summary>
    /// Class for representing and controlling the behaviour of a single-player campaign.
    /// </summary>
    public class SingleplayerCampaign : ICampaignController {

        private string m_locSourceID;
        private ushort m_unitCampaignID;

        private HashSet<string> m_allowedSummerAtmospheres;
        private HashSet<string> m_allowedWinterAtmospheres;

        private ICampaignGoalManager m_goalManager;
        private ICampaignEventManager m_eventManager;
        private ICampaignMap m_map;
        private ICampaignScriptHandler m_scriptHandler;
        private ICampaignTurn m_turn;

        private ICampaignTeam[] m_teams;
        private ICampaignPlayer m_player; // The single-player

        private CampaignMapAI m_opponentAI;

        private Localize m_locale;

        public ICampaignGoalManager Goals => this.m_goalManager;

        public ICampaignEventManager Events => this.m_eventManager;

        public ICampaignScriptHandler Script => this.m_scriptHandler;

        public ICampaignMap Map => this.m_map;

        public ICampaignTurn Turn => this.m_turn;

        public event CampaignEngagementAttackHandler OnAttack;
        public event CampaignEngagmeentDefendHandler OnDefend;
        public event CampaignTurnOverHandler OnTurn;

        public Localize Locale => this.m_locale;

        /// <summary>
        /// 
        /// </summary>
        public Army[] Armies { get; init; }

        public List<GfxMap> GfxMaps { get; init; }

        public int DifficultyLevel { get; init; }

        public bool IsSingleplayer => true;

        private SingleplayerCampaign() {
            this.m_unitCampaignID = 0;
        }

        public void Save() {

        }

        public bool EndTurn() {
            bool canCampaignContinue = ICampaignController.GlobalEndTurn(this);
            this.OnTurn?.Invoke();
            if (canCampaignContinue) {
                if (this.Turn.CurrentTurn != this.m_player.Team.Team) {
                    Coroutine.StartCoroutine(this.m_opponentAI.ProcessTurn());
                }
            }
            return canCampaignContinue;
        }

        public void StartCampaign() {

            // Initialise AI
            this.m_opponentAI.Initialise();

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
                while (targetList[i].Count < Company.MAX_SIZE && targetList[i].Count < distribution && options.Count > 0) {

                    // Add random
                    targetList[i].Add(options.PickOut(BattlegroundsInstance.RNG));

                }

            }

        }

        public CampaignEngagementData? HandleAttacker(ICampaignFormation[] attackingFormations, ICampaignMapNode node) {

            // Note: Because it's singleplayer we can treat this as the AI.

            // Get teams
            var attackers = attackingFormations[0].Team;
            var defenders = node.Owner;

            // Create data
            CampaignEngagementData data = new CampaignEngagementData {
                node = node,
                scenario = this.Map.PickScenario(node, this.m_turn),
                allParticipatingSquads = new List<Squad>(),
                attackers = attackers,
                defenders = defenders,
                attackingFaction = attackingFormations[0].Regiments[0].ElementOf.EleemntOf.Faction,
                defendingFaction = node.Occupants[0].Regiments[0].ElementOf.EleemntOf.Faction,
                attackingFormations = attackingFormations.ToList(),
                defendingFormations = node.Occupants
            };

            // Generate AI attack data (TODO: Bring in AI logic)
            this.GenerateAIEngagementSetup(ref data, false, 1, attackingFormations);

            // Return generated data
            return data;

        }

        public void HandleDefender(ref CampaignEngagementData engagementData) =>
            // Note: Because it's singleplayer we can treat this as the AI.
            this.GenerateAIEngagementSetup(ref engagementData, true, engagementData.defendingFormations.Count, engagementData.defendingFormations.ToArray());

        public ICampaignTeam GetTeam(CampaignArmyTeam teamType) => teamType switch {
            CampaignArmyTeam.TEAM_ALLIES => this.m_teams[0],
            CampaignArmyTeam.TEAM_AXIS => this.m_teams[1],
            _ => null
        };

        public Division[] GetReserves(CampaignArmyTeam teamType) {
            var armies = this.Armies.Where(x => x.Team == teamType);
            List<Division> divs = new List<Division>();
            foreach (var army in armies) {
                foreach (var div in army.Divisions) {
                    if (!div.Regiments.Any(x => x.IsDeployed)) {
                        divs.Add(div);
                    }
                }
            }
            return divs.ToArray();
        }

        public ICampaignPlayer GetSelf() => this.m_player;

        public async void HandleAttack(ICampaignMapNode attackedNode, ICampaignFormation[] formations) {

            // Get attackers
            var attackerData = await Task.Run(() => this.OnAttack?.Invoke(formations, attackedNode) ?? null);

            // If no engagement data, return
            if (attackerData is CampaignEngagementData engagementData) {

                // Get defender data
                var engagement = (await Task.Run(() => this.OnDefend?.Invoke(engagementData) ?? null)).Value;
                this.ZipPlayerData(ref engagement);

                // Create engagement data
                var match = this.Engage(engagement);
                match.Control();

                // Hand-off control
                ICampaignController.HandleEngagement(match, engagement, (node, success) => {

                    // Destroy low length formations
                    static bool DestroyLowStrengthFormations(ICampaignFormation formation) {
                        if (formation.CalculateStrength() <= 0.025f) {
                            formation.Disband(true);
                            return true;
                        } else {
                            return false;
                        }
                    }

                    // Remove all dead attackers and defenders
                    int lostAttackers = engagement.attackingFormations.RemoveAll(DestroyLowStrengthFormations);
                    int lostDefenders = node.Occupants.RemoveAll(DestroyLowStrengthFormations);

                    if (success) {

                        // Move out defenders
                        ICampaignController.ScatterLostEngagementDefenders(this, node);

                        // Move in formations
                        formations.ForEach(x => this.MoveFormation(x, attackedNode));

                    }

                });

            }

        }

        public MoveFormationResult MoveFormation(ICampaignFormation formation, ICampaignMapNode to) {

            // If we're attacking
            if (to.Occupants.Count > 0 && to.Owner != formation.Team) {

                // Cannot move, should initiate attack.
                return MoveFormationResult.MoveAttack;

            } else {

                // Make sure we can actually move
                if (!to.CanMoveTo(formation)) {
                    return MoveFormationResult.MoveCap;
                }

                // Update occupants
                formation.Node.RemoveOccupant(formation);

                // Set location and update destination
                formation.SetNodeLocation(to);

                // Update owner
                if (to.Owner != formation.Team) {
                    to.SetOwner(formation.Team);
                }

                // Return true
                return MoveFormationResult.MoveSuccess;

            }

        }

        public bool FindPath(ICampaignFormation formation, ICampaignMapNode to) {
            if (this.Map.FindPath(formation, to, out List<ICampaignMapNode> path)) {
                formation.SetNodeDestinations(path);
                return true;
            }
            return false;
        }

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

        public ICampaignFormation Deploy(Division reserve, ICampaignMapNode node) {

            // Create formation
            Formation formation = new Formation();
            formation.FromDivision(reserve);
            formation.SetNodeLocation(node);

            // Return formation
            return formation;

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

            // Init 1v1 data
            string[] playerName = {
                human.Team == CampaignArmyTeam.TEAM_ALLIES ? human.Name : string.Empty,
                human.Team == CampaignArmyTeam.TEAM_AXIS ? human.Name : string.Empty,
            };
            ulong[] playerID = {
                human.Team == CampaignArmyTeam.TEAM_ALLIES ? human.SteamID : 0,
                human.Team == CampaignArmyTeam.TEAM_AXIS ? human.SteamID : 0
            };
            string[] playerFaction = {
                human.Team == CampaignArmyTeam.TEAM_ALLIES ? human.Faction : package.CampaignArmies.FirstOrDefault(x => x.Army.IsAllied).Army.Name,
                human.Team == CampaignArmyTeam.TEAM_AXIS ? human.Faction : package.CampaignArmies.FirstOrDefault(x => x.Army.IsAxis).Army.Name,
            };

            // Create players
            campaign.m_teams[0].CreatePlayer(0, playerName[0], playerID[0], playerFaction[0]);
            campaign.m_teams[1].CreatePlayer(0, playerName[1], playerID[1], playerFaction[1]);

            // Counter to keep track of diviions
            uint divisionCount = 0;

            // Create the armies
            for (int i = 0; i < package.CampaignArmies.Length; i++) {
                campaign.CreateArmy(i, ref divisionCount, package.CampaignArmies[i]);
            }

            // Set default player and create AI
            campaign.m_player = campaign.m_teams[human.Team == CampaignArmyTeam.TEAM_ALLIES ? 0 : 1].Players[0];
            campaign.m_opponentAI = new CampaignMapAI(campaign.m_teams[human.Team == CampaignArmyTeam.TEAM_ALLIES ? 1 : 0], campaign);

            // Create turn data
            campaign.m_turn = new SingleplayerCampaignTurn(startTeam, new[] {
                package.CampaignTurnData.Start,
                package.CampaignTurnData.End
            }, package.CampaignTurnData.TurnLength);
            campaign.Turn.SetWinterDates(new[] { package.CampaignWeatherData.WinterStart, package.CampaignWeatherData.WinterEnd });

            // Create goals
            campaign.m_goalManager = new SingleplayerCampaignGoalManager();
            for (int i = 0; i < package.CampaignArmies.Length; i++) {
                RegisterGoalInManager(campaign.m_goalManager as SingleplayerCampaignGoalManager, null, package.CampaignArmies[i], package.CampaignArmies[i].Goals);
            }

            // Set atmospheres
            campaign.m_allowedSummerAtmospheres = package.CampaignWeatherData.SummerAtmospheres;
            campaign.m_allowedWinterAtmospheres = package.CampaignWeatherData.WinterAtmospheres;

            // Setup lua environment
            ICampaignController.SetupScriptEnvironment(campaign, package);

            // Return campaign
            return campaign;

        }

        private static void RegisterGoalInManager(SingleplayerCampaignGoalManager manager, SingleplayerCampaignGoal parent, CampaignPackage.ArmyData army, CampaignPackage.ArmyGoalData[] goals) {
            List<ICampaignGoal> newGoals = new List<ICampaignGoal>();
            for (int i = 0; i < goals.Length; i++) {
                var bp = goals[i];
                var goal = new SingleplayerCampaignGoal(bp.Title, bp.Desc, (CampaignGoalType)bp.Type, parent, bp.Hidden ? CampaignGoalState.Inactive : CampaignGoalState.Started);
                goal.SetScriptPointers(bp.OnDone, bp.OnFail, bp.OnTrigger, bp.OnUI);
                if (bp.SubGoals.Length > 0) {
                    RegisterGoalInManager(manager, goal, army, bp.SubGoals);
                }
                if (parent is null) {
                    manager.AddGoal(army.Army.Name, goal.Title.LocaleID, goal);
                } else {
                    newGoals.Add(goal);
                }
            }
            if (parent is not null) {
                parent.SubGoals = newGoals.ToArray();
            }
        }

        private static void PopulateListWithFormations(WeightedList<Squad> squads, bool isDefence, ICampaignFormation[] formations) {

            // Run through all formations
            for (int i = 0; i < formations.Length; i++) {

                // Loop through all regiments
                formations[i].Regiments.ForEach(x => {
                    x.FirstCompany().Units.ForEach(u => {
                        double priority = 0.05;
                        if (u.SBP.Types.IsAntiTank) {
                            priority += isDefence ? 0.7 : 0.2;
                        } else if (u.SBP.Types.IsInfantry) {
                            priority += isDefence ? 0.8 : 0.6;
                        } else if (u.SBP.Types.IsVehicle) {
                            priority += isDefence ? 0.3 : 0.5;
                        }
                        if (u.SBP.Types.IsCommandUnit) {
                            priority = 1.0;
                        }
                        squads.Add(u, priority);
                    });
                });

            }

        }

        private static SessionInfo GetSessionFromEngagementData(CampaignEngagementData data) {
            byte uidPlayerIndex = 0;
            SessionParticipant[] CreateTeam(bool isAttacker, List<Squad>[] companies, ParticipantTeam team) {
                SessionParticipant[] participants = new SessionParticipant[companies.Length];
                var diffs = isAttacker ? data.attackingDifficulties : data.defendingDifficulties;
                for (int i = 0; i < companies.Length; i++) {
                    var company = ICampaignController.GetCompanyFromEngagementData(data, isAttacker, i);
                    if (diffs[i] == AIDifficulty.Human) {
                        var usr = BattlegroundsInstance.Steam.User;
                        participants[i] = new SessionParticipant(usr.Name, usr.ID, company, team, (byte)i, uidPlayerIndex++);
                    } else {
                        participants[i] = new SessionParticipant(diffs[i], company, team, (byte)i, uidPlayerIndex++);
                    }
                }
                return participants;
            }
            bool areAlliesAttacking = data.attackers == CampaignArmyTeam.TEAM_ALLIES;
            SessionInfo info = new SessionInfo() {
                SelectedScenario = data.scenario,
                SelectedGamemode = WinconditionList.GetGamemodeByName(ModManager.GetPackage("mod_bg").GamemodeGUID, "vp"), // TODO: Set according to CampaignEngagementData
                SelectedGamemodeOption = 50, // TODO: Set according to CampaignEngagementData
                SelectedTuningMod = ModManager.GetMod<ITuningMod>(ModManager.GetPackage("mod_bg").TuningGUID),
                DefaultDifficulty = AIDifficulty.AI_Hard,
                FillAI = false,
                IsOptionValue = true, // TODO: Set according to CampaignEngagementData
                Allies = CreateTeam(areAlliesAttacking, areAlliesAttacking ? data.attackingCompanyUnits : data.defendingCompanyUnits, ParticipantTeam.TEAM_ALLIES),
                Axis = CreateTeam(!areAlliesAttacking, !areAlliesAttacking ? data.attackingCompanyUnits : data.defendingCompanyUnits, ParticipantTeam.TEAM_AXIS)
            };
            return info;
        }

    }

}
