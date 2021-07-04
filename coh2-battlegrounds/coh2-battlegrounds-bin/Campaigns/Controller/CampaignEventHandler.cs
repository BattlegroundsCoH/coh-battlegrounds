using Battlegrounds.Campaigns.API;

namespace Battlegrounds.Campaigns.Controller {

    /// <summary>
    /// 
    /// </summary>
    /// <param name="attackingFormations"></param>
    /// <param name="node"></param>
    /// <returns></returns>
    public delegate CampaignEngagementData? CampaignEngagementAttackHandler(ICampaignFormation[] attackingFormations, ICampaignMapNode node);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="engagementData"></param>
    /// <returns></returns>
    public delegate CampaignEngagementData CampaignEngagmeentDefendHandler(CampaignEngagementData engagementData);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    public delegate void CampaignEngagementOverHandler(CampaignEngagementData data);

    /// <summary>
    /// 
    /// </summary>
    public delegate void CampaignTurnOverHandler();

}
