namespace Battlegrounds.Campaigns.API {
    
    /// <summary>
    /// Interface for managing goals in a campaign context.
    /// </summary>
    public interface ICampaignGoalManager {

        /// <summary>
        /// Add a new goal.
        /// </summary>
        /// <param name="goalID">The ID of the goal.</param>
        /// <param name="goal">The actual goal.</param>
        void AddGoal(string goalID, ICampaignGoal goal);

        /// <summary>
        /// Get goal by goal ID.
        /// </summary>
        /// <param name="goalID">The ID of the goal to fetch.</param>
        /// <returns>The goal registered with <paramref name="goalID"/>.</returns>
        ICampaignGoal GetGoal(string goalID);

        /// <summary>
        /// Handle the campaign goals state.
        /// </summary>
        /// <param name="scriptHandler">The handler for scripts.</param>
        void Handle(ICampaignScriptHandler scriptHandler);

    }

}
