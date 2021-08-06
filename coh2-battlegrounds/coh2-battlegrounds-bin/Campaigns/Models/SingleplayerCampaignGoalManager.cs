using System.Collections.Generic;
using System.Linq;

using Battlegrounds.Campaigns.API;

namespace Battlegrounds.Campaigns.Models {

    /// <summary>
    /// Manager class for handling goals in a singleplayer campaign.
    /// </summary>
    public class SingleplayerCampaignGoalManager : ICampaignGoalManager {

        private Dictionary<string, ICampaignGoal> m_goals;
        private Dictionary<string, string> m_goalOwners;

        /// <summary>
        /// 
        /// </summary>
        public SingleplayerCampaignGoalManager() {
            this.m_goals = new Dictionary<string, ICampaignGoal>();
            this.m_goalOwners = new Dictionary<string, string>();
        }

        public void AddGoal(string army, string goalID, ICampaignGoal goal) {
            this.m_goals[goalID] = goal;
            this.m_goalOwners[goalID] = army;
        }

        public ICampaignGoal GetGoal(string goalID)
            => this.m_goals[goalID];

        public ICampaignGoal[] GetGoals(string faction) => this.m_goalOwners.Where(x => x.Value == faction).Select(x => this.m_goals[x.Key]).ToArray();

        public void Handle(ICampaignScriptHandler scriptHandler) {
            foreach (var goal in this.m_goals) {
                goal.Value.UpdateState(scriptHandler);
            }
        }

    }

}
