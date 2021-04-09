using System.Collections.Generic;
using Battlegrounds.Campaigns.API;

namespace Battlegrounds.Campaigns.Models {

    /// <summary>
    /// Manager class for handling goals in a singleplayer campaign.
    /// </summary>
    public class SingleplayerCampaignGoalManager : ICampaignGoalManager {

        private Dictionary<string, ICampaignGoal> m_goals;

        public SingleplayerCampaignGoalManager() {
            this.m_goals = new Dictionary<string, ICampaignGoal>();
        }

        public void AddGoal(string goalID, ICampaignGoal goal)
            => this.m_goals[goalID] = goal;

        public ICampaignGoal GetGoal(string goalID)
            => this.m_goals[goalID];

        public void Handle(ICampaignScriptHandler scriptHandler) {
            foreach (var goal in this.m_goals) {
                goal.Value.UpdateState(scriptHandler);
            }
        }

    }

}
