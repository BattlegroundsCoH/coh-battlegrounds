using System.Collections.Generic;
using Battlegrounds.Game.DataCompany;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Game.Match.Analyze;

namespace Battlegrounds.Game.Match.Finalizer {

    /// <summary>
    /// Finalize strategy for singleplayer matches (but capable of handling multiple non-AI players). Implementation of <see cref="IFinalizeStrategy"/>.
    /// </summary>
    public class SingleplayerFinalizer : IFinalizeStrategy {

        protected Dictionary<Player, Company> m_companies;

        public FinalizedCompanyHandler CompanyHandler { get; set; }

        public SingleplayerFinalizer() {
            this.m_companies = null;
        }

        public virtual void Finalize(IAnalyzedMatch analyzedMatch) {

            // Get the session
            var session = analyzedMatch.Session;

            // Get the units
            var units = analyzedMatch.Units;

            // Get the players
            var players = analyzedMatch.Players;

            // Create company dictionary
            this.m_companies = new();

            // Assign player companies
            foreach (Player player in players) {
                var company = session.GetPlayerCompany(player.ID);
                if (company is not null) {
                    this.m_companies.Add(player, company);
                } else {
                    // TODO: Handle
                }
            }

            // Run through all units
            foreach (UnitStatus status in units) {

                // Ignore AI player data
                if (status.PlayerOwner.IsAIPlayer) {
                    continue;
                }

                // Get the relevant company
                var company = this.m_companies[status.PlayerOwner];

                // Get the squad
                Squad squad = company.GetSquadByIndex(status.UnitID);

                // If the unit is dead, remove it.
                if (status.IsDead) {

                    // Remove the squad
                    company.RemoveSquad(status.UnitID);

                    // Update losses
                    company.UpdateStatistics(x => UpdateLosses(x, !squad.SBP.IsVehicle, 0)); // TODO: Get proper loss sizes

                } else {

                    // Update veterancy
                    squad.IncreaseVeterancy(status.VetChange, status.VetExperience);

                    // Update combat time
                    squad.IncreaseCombatTime(status.CombatTime);

                    // TODO: Apply pickups

                }

            }

            // TODO: Save captured items etc.

        }

        protected static CompanyStatistics UpdateLosses(CompanyStatistics original, bool isInfantry, uint amount) {
            if (isInfantry) {
                original.TotalInfantryLosses += amount;
            } else {
                original.TotalVehicleLosses += amount;
            }
            return original;
        }

        public virtual void Synchronize(object syncronizerObject) {

            foreach (var pair in this.m_companies) {
                if (!pair.Key.IsAIPlayer) {
                    this.CompanyHandler?.Invoke(pair.Value);
                }
            }

        }

    }

}
