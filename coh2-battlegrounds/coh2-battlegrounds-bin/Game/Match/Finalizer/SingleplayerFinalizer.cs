using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Battlegrounds.Game.DataCompany;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Game.Match.Analyze;

namespace Battlegrounds.Game.Match.Finalizer;

/// <summary>
/// Finalize strategy for singleplayer matches (but capable of handling multiple non-AI players). Implementation of <see cref="IFinalizeStrategy"/>.
/// </summary>
public class SingleplayerFinalizer : IFinalizeStrategy {

    protected Dictionary<Player, Company> m_companies;
    protected IAnalyzedMatch? m_matchData;

    /// <summary>
    /// Get or set if finalizer should also notify AI company changes. Default value is <see langword="false"/>.
    /// </summary>
    public bool NotifyAI { get; set; }

    public FinalizedCompanyHandler CompanyHandler { get; set; } = x => { };

    public SingleplayerFinalizer() => this.m_companies = new();

    public virtual void Finalize(IAnalyzedMatch analyzedMatch) {

        // Save match data (for potential use in inheriting classes
        this.m_matchData = analyzedMatch;

        // Get the session
        var session = analyzedMatch.Session;

        // Get the units
        var units = analyzedMatch.Units;

        // Get the items
        var items = analyzedMatch.Items;

        // Get the players
        var players = analyzedMatch.Players;

        // Create company dictionary
        this.m_companies = new();

        // Assign player companies
        foreach (var player in players) {
            var company = session.GetPlayerCompany(player.SteamID);
            if (company is not null) {
                if (analyzedMatch.IsWinner(player)) {
                    company.UpdateStatistics(x => { x.TotalMatchWinCount++; x.TotalMatchCount++; return x; });
                } else {
                    company.UpdateStatistics(x => { x.TotalMatchLossCount++; x.TotalMatchCount++; return x; });
                }
                this.m_companies.Add(player, company);
            } else {
                Trace.WriteLine($"Failed to find a company for {player.SteamID} ({player.Name})", nameof(SingleplayerFinalizer));
                // TODO: Handle
            }
        }

        // Run through all units
        foreach (var status in units) {

            // Ignore AI player data
            if (status.PlayerOwner.IsAIPlayer || this.NotifyAI) {
                continue;
            }

            // Get the relevant company
            var company = this.m_companies[status.PlayerOwner];

            // Get the squad
            var squad = company.GetSquadByIndex(status.UnitID);

            // If the unit is dead, remove it.
            if (status.IsDead) {

                // Remove the squad
                if (!company.RemoveSquad(status.UnitID)) {
                    Trace.WriteLine($"Failed to remove company unit '{status.UnitID}' from company '{company.Name}'.", nameof(SingleplayerFinalizer));
                }

                // Update losses
                company.UpdateStatistics(x => UpdateLosses(x, !squad.SBP.Types.IsVehicle, 0)); // TODO: Get proper loss sizes

            } else {

                // Update veterancy
                if (status.VetChange >= 0) {
                    squad.IncreaseVeterancy(status.VetChange, status.VetExperience);
                    Trace.WriteLine($"Unit ID '{status.UnitID}' from '{company.Name}' gained '{status.VetExperience}'.", nameof(SingleplayerFinalizer));
                }

                // Update combat time
                squad.IncreaseCombatTime(status.CombatTime);

                // Picked up any items?
                if (status.CapturedSlotItems.Count > 0) {
                    foreach (var item in status.CapturedSlotItems) {
                        squad.AddSlotItem(item);
                    }
                }

            }

        }

        // Loop over captured items
        foreach (var item in items) {

            // Get the relevant company
            var company = this.m_companies[item.PlayerOwner];

            // Add to captured items
            company.AddInventoryItem(item.Blueprint);

        }

    }

    protected static CompanyStatistics UpdateLosses(CompanyStatistics original, bool isInfantry, uint amount) {
        if (isInfantry) {
            original.TotalInfantryLosses += amount;
        } else {
            original.TotalVehicleLosses += amount;
        }
        return original;
    }

    public virtual void Synchronize(object synchronizeObject) {

        // Make sure we log this unfortunate event
        if (this.CompanyHandler is null) {
            Trace.WriteLine("{Warning} -- The company handler is NULL and changes will therefore not be handled further!", nameof(SingleplayerFinalizer));
            return;
        }

        // Loop through all companies and save
        foreach (var pair in this.m_companies) {
            if (!pair.Key.IsAIPlayer || this.NotifyAI) {
                this.CompanyHandler?.Invoke(pair.Value);
            }
        }

    }

    protected virtual Company? GetLocalPlayerCompany() {
        try {
            ulong selfID = BattlegroundsInstance.Steam.User.ID;
            return this.m_companies.FirstOrDefault(x => x.Key.SteamID == selfID).Value;
        } catch {
            return null;
        }
    }

}
