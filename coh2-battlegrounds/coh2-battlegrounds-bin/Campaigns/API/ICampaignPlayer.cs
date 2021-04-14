namespace Battlegrounds.Campaigns.API {
    
    /// <summary>
    /// Interface for representing a campaign player.
    /// </summary>
    public interface ICampaignPlayer {

        /// <summary>
        /// Get the <see cref="ICampaignTeam"/> the player is a part of.
        /// </summary>
        ICampaignTeam Team { get; }

        /// <summary>
        /// Get the display name of the player.
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Get the faction name used by the player.
        /// </summary>
        string FactionName { get; }

    }

}
