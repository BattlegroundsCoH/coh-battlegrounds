using System;

using Battlegrounds.Campaigns.API;
using Battlegrounds.Locale;

namespace Battlegrounds.Campaigns.Models {

    /// <summary>
    /// Singleplayer representation of a campaign goal. Implements <see cref="ICampaignGoal"/>.
    /// </summary>
    public class SingleplayerCampaignGoal : ICampaignGoal {

        private CampaignGoalState m_state;
        private string m_scriptOnDone;
        private string m_scriptOnFail;
        private string m_scriptOnTrigger;
        private string m_scriptOnUI;

        public ICampaignGoal Parent { get; }

        public ICampaignGoal[] SubGoals { get; set; }

        public CampaignGoalType Type { get; }

        public CampaignGoalState State => this.m_state;

        public LocaleKey Title { get; }

        public LocaleKey Desc { get; }

        public event OnGoalStateHandler OnState;

        public SingleplayerCampaignGoal(LocaleKey title, LocaleKey desc, CampaignGoalType type, ICampaignGoal parent, CampaignGoalState initialState) {
            this.Title = title;
            this.Desc = desc;
            this.Parent = parent;
            this.Type = type;
            this.SubGoals = Array.Empty<ICampaignGoal>();
            this.m_state = initialState;
            this.m_scriptOnDone = this.m_scriptOnFail = this.m_scriptOnUI = this.m_scriptOnTrigger = string.Empty;
        }

        public void UpdateState(ICampaignScriptHandler scriptHandler) {
            if (!string.IsNullOrEmpty(this.m_scriptOnFail) && scriptHandler.GetGlobalAndInvoke(this.m_scriptOnFail)) {
                this.SetState(CampaignGoalState.Failed);
            }
            if (!string.IsNullOrEmpty(this.m_scriptOnDone) && scriptHandler.GetGlobalAndInvoke(this.m_scriptOnDone)) {
                this.SetState(CampaignGoalState.Completed);
            }
            if (this.m_state == CampaignGoalState.Inactive) {
                if (!string.IsNullOrEmpty(this.m_scriptOnTrigger) && scriptHandler.GetGlobalAndInvoke(this.m_scriptOnTrigger)) {
                    this.SetState(CampaignGoalState.Started);
                }
            } else {
                if (!string.IsNullOrEmpty(this.m_scriptOnUI)) {
                    scriptHandler.GetGlobalAndInvoke(this.m_scriptOnUI);
                }
            }
        }

        public void SetScriptPointers(string onDone, string onFail, string onTrigger, string onUI) {
            this.m_scriptOnDone = onDone ?? this.m_scriptOnDone;
            this.m_scriptOnFail = onFail ?? this.m_scriptOnFail;
            this.m_scriptOnUI = onUI ?? this.m_scriptOnUI;
            this.m_scriptOnTrigger = onTrigger ?? this.m_scriptOnTrigger;
        }

        private void SetState(CampaignGoalState state) {
            this.m_state = state;
            this.OnState?.Invoke(this, state);
        }

    }

}
