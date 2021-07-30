using System.Collections.Generic;

using Battlegrounds.Campaigns.Organisations;
using Battlegrounds.Game.Gameplay;

namespace Battlegrounds.Campaigns.API {

    /// <summary>
    /// Represents a team in the campaign system.
    /// </summary>
    public enum CampaignArmyTeam {

        /// <summary>
        /// Neutral team, no allegiance.
        /// </summary>
        TEAM_NEUTRAL,

        /// <summary>
        /// Allied team (wf allies, soviets).
        /// </summary>
        TEAM_ALLIES,

        /// <summary>
        /// Axis team (okw, wehrmacht).
        /// </summary>
        TEAM_AXIS,

    }

    /// <summary>
    /// Interface for representing a team in a campaign.
    /// </summary>
    public interface ICampaignTeam {

        /// <summary>
        /// Get the campaign team type.
        /// </summary>
        CampaignArmyTeam Team { get; }

        /// <summary>
        /// Get the players on the team.
        /// </summary>
        ICampaignPlayer[] Players { get; }

        /// <summary>
        /// Get the amount of victory points the team has.
        /// </summary>
        double VictoryPoints { get; }

        /// <summary>
        /// Create a new <see cref="ICampaignPlayer"/> with specified index, name and index.
        /// </summary>
        /// <param name="playerIndex">The index of the player on the team.</param>
        /// <param name="name">The display name of the player.</param>
        /// <param name="uid">The unique ID of the player.</param>
        /// <param name="faction">The faction the player is using.</param>
        void CreatePlayer(int playerIndex, string name, ulong uid, string faction);

        /// <summary>
        /// Add a whole <see cref="Army"/> to the team reserves.
        /// </summary>
        /// <param name="army">The new reserve army.</param>
        void AddReserveArmy(Army army);

        /// <summary>
        /// Add a whole <see cref="Division"/> to the team reserves.
        /// </summary>
        /// <param name="division">The new reserve division.</param>
        void AddReserveDivision(Division division);

        /// <summary>
        /// Add a <see cref="Regiment"/> to team reserves.
        /// </summary>
        /// <param name="regiment">The reserve regiment.</param>
        void AddReserveRegiment(Regiment regiment);

        /// <summary>
        /// Get the undeployed regiments that can be called in.
        /// </summary>
        /// <returns>List of undeployed regiments.</returns>
        List<Regiment> GetReserves();

        /// <summary>
        /// Award specified amount of points to the team.
        /// </summary>
        /// <param name="value">The amount of points to award.</param>
        void AwardPoints(double value);

        /// <summary>
        /// Get the <see cref="CampaignArmyTeam"/> corresponding to the string name of a faction.
        /// </summary>
        /// <param name="faction">The string faction to find <see cref="CampaignArmyTeam"/> value of.</param>
        /// <returns>
        /// If <paramref name="faction"/> is "aef", "soviet", or "british" then <see cref="CampaignArmyTeam.TEAM_ALLIES"/> is returned;
        /// Otherwise <see cref="CampaignArmyTeam.TEAM_AXIS"/>.
        /// </returns>
        public static CampaignArmyTeam GetArmyTeamFromFaction(string faction)
            => Faction.IsAlliedFaction(faction) ? CampaignArmyTeam.TEAM_ALLIES : CampaignArmyTeam.TEAM_AXIS;

    }

}
