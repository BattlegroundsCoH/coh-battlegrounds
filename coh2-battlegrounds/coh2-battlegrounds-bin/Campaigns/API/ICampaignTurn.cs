using Battlegrounds.Lua;

namespace Battlegrounds.Campaigns.API {

    /// <summary>
    /// Interface for turn handling in a campaign.
    /// </summary>
    public interface ICampaignTurn {

        /// <summary>
        /// Get the current army in turn.
        /// </summary>
        [LuaUserobjectProperty]
        CampaignArmyTeam CurrentTurn { get; }

        /// <summary>
        /// Get the current turn date.
        /// </summary>
        [LuaUserobjectProperty]
        string Date { get; }

        /// <summary>
        /// Get if the current turn is taking place during the winter.
        /// </summary>
        [LuaUserobjectProperty]
        bool IsWinter { get; }

        /// <summary>
        /// Set the start and end dates of the winter.
        /// </summary>
        /// <param name="dates">A two-element array of tuples containing the dates of when to start and end winter.</param>
        void SetWinterDates((int year, int month, int day)[] dates);

        /// <summary>
        /// End the current turn. If last turn in round, the round is also ended.
        /// </summary>
        /// <param name="wasRound">flag determining if the turn also ended the round.</param>
        /// <returns>If no winner was found, <see langword="true"/> is returned; Otherwise <see langword="false"/>, marking if a winner was found.</returns>
        bool EndTurn(out bool wasRound);

        /// <summary>
        /// Get if the current date is the end date.
        /// </summary>
        /// <returns><see langword="true"/> if end date; Otherwise <see langword="false"/>.</returns>
        [LuaUserobjectMethod(UseMarshalling = true)]
        bool IsEndDate();

    }

}
