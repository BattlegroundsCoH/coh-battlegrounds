namespace Battlegrounds.Campaigns.API {

    /// <summary>
    /// 
    /// </summary>
    public enum CampaignMapTransitionType {
        Binary,
        Unary,
    }

    public interface ICampaignMapNodeTransition {

        /// <summary>
        /// 
        /// </summary>
        ICampaignMapNode From { get; }

        /// <summary>
        /// 
        /// </summary>
        ICampaignMapNode To { get; }

        /// <summary>
        /// 
        /// </summary>
        CampaignMapTransitionType TransitionType { get; }

    }

}
