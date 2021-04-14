using Battlegrounds.Locale;

namespace Battlegrounds.Campaigns.API {
    
    /// <summary>
    /// 
    /// </summary>
    public enum CampaignGoalType {
        Primary,
        Secondary,
        Achievement,
    }

    /// <summary>
    /// 
    /// </summary>
    public enum CampaignGoalState {
        Started,
        Completed,
        Failed,
        Inactive,
    }

    public delegate void OnGoalStateHandler(ICampaignGoal goal, CampaignGoalState newState);

    /// <summary>
    /// 
    /// </summary>
    public interface ICampaignGoal {

        /// <summary>
        /// 
        /// </summary>
        ICampaignGoal Parent { get; }

        /// <summary>
        /// 
        /// </summary>
        ICampaignGoal[] SubGoals { get; }

        /// <summary>
        /// 
        /// </summary>
        CampaignGoalType Type { get; }

        /// <summary>
        /// 
        /// </summary>
        CampaignGoalState State { get; }

        /// <summary>
        /// 
        /// </summary>
        LocaleKey Title { get; }

        /// <summary>
        /// 
        /// </summary>
        LocaleKey Desc { get; }

        /// <summary>
        /// 
        /// </summary>
        event OnGoalStateHandler OnState;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="onDone"></param>
        /// <param name="onFail"></param>
        /// <param name="onTrigger"></param>
        /// <param name="onUI"></param>
        void SetScriptPointers(string onDone, string onFail, string onTrigger, string onUI);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="scriptHandler"></param>
        void UpdateState(ICampaignScriptHandler scriptHandler);

    }

}
