using Battlegrounds.Campaigns.Organisations;
using Battlegrounds.Game.Match;

namespace Battlegrounds.Campaigns.Controller {
    
    /// <summary>
    /// 
    /// </summary>
    public interface ICampaignController {

        /// <summary>
        /// 
        /// </summary>
        ActiveCampaign Campaign { get; }

        /// <summary>
        /// 
        /// </summary>
        bool IsSingleplayer { get; }

        /// <summary>
        /// 
        /// </summary>
        void Save();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        bool EndTurn();

        /// <summary>
        /// 
        /// </summary>
        void EndCampaign();

        /// <summary>
        /// Initialize a <see cref="MatchController"/> object that is ready to be controlled.
        /// </summary>
        /// <param name="data">The engagement data to use while generating scenario data</param>
        /// <returns>A ready-to-use <see cref="MatchController"/> instance.</returns>
        MatchController Engage(CampaignEngagementData data);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="isDefence"></param>
        /// <param name="armyCount"></param>
        /// <param name="availableFormations"></param>
        void GenerateAIEngagementSetup(ref CampaignEngagementData data, bool isDefence, int armyCount, Formation[] availableFormations);

    }

}
