using Battlegrounds.Campaigns.API;

namespace Battlegrounds.Campaigns.Map {

    /// <summary>
    /// 
    /// </summary>
    public class CampaignMapTransition : ICampaignMapNodeTransition {

        /// <summary>
        /// 
        /// </summary>
        public ICampaignMapNode From { get; }

        /// <summary>
        /// 
        /// </summary>
        public ICampaignMapNode To { get; }

        /// <summary>
        /// 
        /// </summary>
        public CampaignMapTransitionType TransitionType { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="transitionType"></param>
        public CampaignMapTransition(CampaignMapNode from, CampaignMapNode to, CampaignMapTransitionType transitionType) {
            this.From = from;
            this.To = to;
            this.TransitionType = transitionType;
        }

    }

}
