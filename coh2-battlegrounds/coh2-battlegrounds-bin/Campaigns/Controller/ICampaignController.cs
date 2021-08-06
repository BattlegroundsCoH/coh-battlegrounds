using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Battlegrounds.Campaigns.API;
using Battlegrounds.Campaigns.Organisations;
using Battlegrounds.Campaigns.Scripting;
using Battlegrounds.Functional;
using Battlegrounds.Game.DataCompany;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Game.Match;
using Battlegrounds.Gfx;
using Battlegrounds.Locale;
using Battlegrounds.Modding;

namespace Battlegrounds.Campaigns.Controller {

    /// <summary>
    /// 
    /// </summary>
    public enum MoveFormationResult {
        MoveSuccess,
        MoveAttack,
        MoveCap
    }

    /// <summary>
    /// Interface for controlling campaign behaviour.
    /// </summary>
    public interface ICampaignController {

        /// <summary>
        /// Get the event manager of the campaign.
        /// </summary>
        ICampaignEventManager Events { get; }

        /// <summary>
        /// Get the campaign goal manager.
        /// </summary>
        ICampaignGoalManager Goals { get; }

        /// <summary>
        /// Get the script handler for the campaign.
        /// </summary>
        ICampaignScriptHandler Script { get; }

        /// <summary>
        /// Get the map object.
        /// </summary>
        ICampaignMap Map { get; }

        /// <summary>
        /// Get the turn data.
        /// </summary>
        ICampaignTurn Turn { get; }

        /// <summary>
        /// Event fired when one team attacks the other team.
        /// </summary>
        event CampaignEngagementAttackHandler OnAttack;

        /// <summary>
        /// Event fired when one team defends itself from the other team.
        /// </summary>
        event CampaignEngagmeentDefendHandler OnDefend;

        /// <summary>
        /// Event fired when <see cref="EndTurn"/> has been called and finished.
        /// </summary>
        event CampaignTurnOverHandler OnTurn;

        /// <summary>
        /// Get the difficulty level
        /// </summary>
        int DifficultyLevel { get; }

        /// <summary>
        /// Get the GFX mappings of the campaign.
        /// </summary>
        List<GfxMap> GfxMaps { get; }

        /// <summary>
        /// Get the specific locale data associated with the campaign.
        /// </summary>
        public Localize Locale { get; }

        /// <summary>
        /// Get if this is a purely singleplayer campaign instance.
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
        void StartCampaign();

        /// <summary>
        /// Determine if local user is allowed to perform actions on the map.
        /// </summary>
        /// <returns>When local user is in turn, <see langword="true"/> is returned; Otherwise <see langword="false"/>.</returns>
        bool IsSelfTurn();

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
        void GenerateAIEngagementSetup(ref CampaignEngagementData data, bool isDefence, int armyCount, ICampaignFormation[] availableFormations);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="attackingFormations"></param>
        /// <param name="node"></param>
        /// <returns></returns>
        CampaignEngagementData? HandleAttacker(ICampaignFormation[] attackingFormations, ICampaignMapNode node);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="teamType"></param>
        /// <returns></returns>
        Division[] GetReserves(CampaignArmyTeam teamType);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="teamType"></param>
        /// <returns></returns>
        ICampaignTeam GetTeam(CampaignArmyTeam teamType);

        /// <summary>
        /// Get the local player instance.
        /// </summary>
        /// <returns>The local player.</returns>
        ICampaignPlayer GetSelf();

        /// <summary>
        /// Moves <paramref name="formation"/> to destination node <paramref name="to"/>. This performs A -> B moves with no intermediate steps.
        /// </summary>
        /// <param name="formation">The formation to move.</param>
        /// <param name="to">The destination of the formation.</param>
        /// <returns>If move can be performed instantly, <see langword="true"/>; Otherwise <see langword="false"/>.</returns>
        MoveFormationResult MoveFormation(ICampaignFormation formation, ICampaignMapNode to);

        /// <summary>
        /// Calculate a path for <paramref name="formation"/> to destination node <paramref name="to"/>. This will use a pathfiding algorithm and may be used on non-neighbouring nodes.
        /// </summary>
        /// <param name="formation">The formation to move.</param>
        /// <param name="to">The destination node to find path of.</param>
        /// <returns>If a path was found <see langword="true"/>; Otherwise <see langword="false"/>.</returns>
        bool FindPath(ICampaignFormation formation, ICampaignMapNode to);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="attackedNode"></param>
        /// <param name="formations"></param>
        void HandleAttack(ICampaignMapNode attackedNode, ICampaignFormation[] formations);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="engagementData"></param>
        void HandleDefender(ref CampaignEngagementData engagementData);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reserve"></param>
        /// <param name="node"></param>
        ICampaignFormation Deploy(Division reserve, ICampaignMapNode node);

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
                .ChangeTuningMod(ModManager.GetPackage("mod_bg").TuningGUID)
                .ChangeName(isAttacker ? data.attackingCompanyNames[index] : data.defendingCompanyNames[index])
                .ChangeUser(isAttacker ? data.attackingPlayerNames[index] : data.defendingPlayerNames[index]);

            // If no deployment phase is set, auto-generate it
            if (units.TrueForAll(x => x.DeploymentPhase == DeploymentPhase.PhaseNone)) {
                DeploymentPhase phase = DeploymentPhase.PhaseInitial;
                int count = 0;
                for (int i = 0; i < units.Count; i++) {
                    var uBld = new UnitBuilder(units[i], true);
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
                    builder.AddAndCommitUnit(uBld);
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

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="data"></param>
        /// <param name="node"></param>
        public static void HandleEngagement(MatchController controller, CampaignEngagementData data, Action<ICampaignMapNode, bool> node) {

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
                void UpdateFormations(ICampaignFormation f) {
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

        protected static void SetupScriptEnvironment(ICampaignController controller, CampaignPackage package) {

            // Set lua stuff
            controller.Script.SetGlobal("Map", controller.Map, true);
            controller.Script.SetGlobal("Turn", controller.Turn, true);
            controller.Script.SetGlobal(BattlegroundsCampaignLibrary.CampaignInstanceField, controller, false);

            // Register library
            BattlegroundsCampaignLibrary.LoadLibrary(controller.Script.ScriptState);

            // Loop over campaign scripts and init them
            package.CampaignScripts.ForEach(x => controller.Script.LoadScript(x));

            // Assign state ptrs
            controller.Map.ScriptHandler = controller.Script;
            controller.Events.ScriptHandler = controller.Script;

        }

        protected static bool GlobalEndTurn(ICampaignController controller) {
            if (controller.Turn.EndTurn(out bool wasEndOfRound)) {
                if (wasEndOfRound) {
                    controller.Map.EachNode(n => {
                        n.EndOfRound();
                        controller.GetTeam(n.Owner).AwardPoints(n.Value);
                    });
                    controller.Map.EachFormation(x => x.EndOfRound());
                    return controller.Events.UpdateVictory();
                }
                return true;
            }
            return false;
        }

        protected static void ScatterLostEngagementDefenders(ICampaignController controller, ICampaignMapNode lostNode) {

            // Get neighbouring nodes of current ally
            var nodes = controller.Map.GetNodeNeighbours(lostNode, x => x.Occupants.Count < x.OccupantCapacity && x.Owner == lostNode.Owner);
            int nodeIndex = 0;

            if (nodes.Count == 0) { // Destroy all formations, nowhere to go to.
                var itter = lostNode.Occupants.GetSafeEnumerator();
                while (itter.MoveNext()) {
                    itter.Current.Disband(true);
                }
            } else {

                var itter = lostNode.Occupants.OrderByDescending(x => x.CalculateStrength()).GetSafeEnumerator();
                while (itter.MoveNext()) {
                    if (controller.MoveFormation(itter.Current, nodes[nodeIndex]) == MoveFormationResult.MoveCap) {
                        itter.Current.Disband(true);
                    }
                    nodeIndex++;
                    if (nodeIndex > nodes.Count) {
                        nodeIndex = 0;
                    }
                }

            }

        }

    }

}
