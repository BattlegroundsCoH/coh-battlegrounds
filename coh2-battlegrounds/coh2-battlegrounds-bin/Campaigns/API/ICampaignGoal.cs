using Battlegrounds.Locale;

namespace Battlegrounds.Campaigns.API {

    /// <summary>
    /// Represents the type of goal.
    /// </summary>
    public enum CampaignGoalType {
        Primary,
        Secondary,
        Achievement,
    }

    /// <summary>
    /// Represents the current state a goal is in.
    /// </summary>
    public enum CampaignGoalState {
        Started,
        Completed,
        Failed,
        Inactive,
    }

    /// <summary>
    /// Handler delegate for handling when <paramref name="goal"/> changes state.
    /// </summary>
    /// <param name="goal">The goal that triggered the handler.</param>
    /// <param name="newState">The new state that was changed to.</param>
    public delegate void OnGoalStateHandler(ICampaignGoal goal, CampaignGoalState newState);

    /// <summary>
    /// Interface representation of a goal in a campaign.
    /// </summary>
    public interface ICampaignGoal {

        /// <summary>
        /// Get the parent goal.
        /// </summary>
        ICampaignGoal Parent { get; }

        /// <summary>
        /// Get subgoals
        /// </summary>
        ICampaignGoal[] SubGoals { get; }

        /// <summary>
        /// Get the goal type
        /// </summary>
        CampaignGoalType Type { get; }

        /// <summary>
        /// Get the current goal state.
        /// </summary>
        CampaignGoalState State { get; }

        /// <summary>
        /// Get the title of the goal.
        /// </summary>
        LocaleKey Title { get; }

        /// <summary>
        /// Get the description of the goal.
        /// </summary>
        LocaleKey Desc { get; }

        /// <summary>
        /// Event triggered when the goal changes state.
        /// </summary>
        event OnGoalStateHandler OnState;

        /// <summary>
        /// Set the script function or variable points to read.
        /// </summary>
        /// <param name="onDone">The function to call when checking if goal has been completed.</param>
        /// <param name="onFail">The function to call when checkinf if goal has failed.</param>
        /// <param name="onTrigger">The function to call when checking if the goal should be triggered.</param>
        /// <param name="onUI">The function to call to update potential UI.</param>
        void SetScriptPointers(string onDone, string onFail, string onTrigger, string onUI);

        /// <summary>
        /// Update the state of the goal.
        /// </summary>
        /// <param name="scriptHandler">The scripthandler to use when invoking script functionality.</param>
        void UpdateState(ICampaignScriptHandler scriptHandler);

    }

}
