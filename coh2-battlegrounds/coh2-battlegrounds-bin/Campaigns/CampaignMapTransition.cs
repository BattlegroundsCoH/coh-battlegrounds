namespace Battlegrounds.Campaigns {
    
    public enum CampaignMapTransitionType {
        Binary,
        Unary,
    }

    public class CampaignMapTransition {
        
        public CampaignMapNode From { get; }
        
        public CampaignMapNode To { get; }
        
        public CampaignMapTransitionType TransitionType { get; }
        
        public CampaignMapTransition(CampaignMapNode from, CampaignMapNode to, CampaignMapTransitionType transitionType) {
            this.From = from;
            this.To = to;
            this.TransitionType = transitionType;
        }

    }

}
