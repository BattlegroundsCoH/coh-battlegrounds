namespace Battlegrounds.Campaigns.API {
    
    /// <summary>
    /// Interface for managing goals in a campaign context.
    /// </summary>
    public interface ICampaignGoalManager {

        /// <summary>
        /// Get all goals in manager.
        /// </summary>
        public const string GET_ALL_GOALS = "all";

        /// <summary>
        /// Add a new goal.
        /// </summary>
        /// <param name="goalFaction">The faction/army the goal belongs to.</param>
        /// <param name="goalID">The ID of the goal.</param>
        /// <param name="goal">The actual goal.</param>
        void AddGoal(string goalFaction, string goalID, ICampaignGoal goal);

        /// <summary>
        /// Get goal by goal ID.
        /// </summary>
        /// <param name="goalID">The ID of the goal to fetch.</param>
        /// <returns>The goal registered with <paramref name="goalID"/>.</returns>
        ICampaignGoal GetGoal(string goalID);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="faction"></param>
        /// <returns></returns>
        ICampaignGoal[] GetGoals(string faction);

        /// <summary>
        /// Handle the campaign goals state.
        /// </summary>
        /// <param name="scriptHandler">The handler for scripts.</param>
        void Handle(ICampaignScriptHandler scriptHandler);

    }

}
