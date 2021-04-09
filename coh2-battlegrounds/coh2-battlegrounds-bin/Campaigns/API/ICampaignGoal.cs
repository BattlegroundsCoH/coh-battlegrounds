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
        Failed
    }

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
        string Title { get; }

        /// <summary>
        /// 
        /// </summary>
        string Desc { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="scriptHandler"></param>
        void UpdateState(ICampaignScriptHandler scriptHandler);

    }

}
