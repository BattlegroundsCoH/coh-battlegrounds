using System.Collections.Generic;
using System.Windows.Controls;

namespace BattlegroundsApp.Views.CampaignViews {

    public delegate void CampaignDialogViewAction(CampaignDialogView view);

    public abstract class CampaignDialogView : UserControl {

        private Canvas m_canvasOwner;
        private Dictionary<string, CampaignDialogViewAction> m_actions;

        protected abstract void OnOpened();

        protected void CloseDialog(string action) {

            // Remove self
            this.m_canvasOwner.Children.Remove(this);

            // Invoke action (if any)
            if (this.m_actions is not null) {
                if (this.m_actions.ContainsKey(action)) {
                    this.m_actions[action]?.Invoke(this);
                }
            }

        }

        public void ShowDialogView(Canvas owner) {

            // Assign canvas
            this.m_canvasOwner = owner;

            // Add self to dialog
            owner.Children.Add(this);
            this.DataContext = this;

            // Set Z-index
            this.SetValue(Panel.ZIndexProperty, 1000);

            // Invoke events
            this.OnOpened();

        }

        public void SubscribeToDialogEvent(string eventName, CampaignDialogViewAction action) {
            if (this.m_actions is null) {
                this.m_actions = new Dictionary<string, CampaignDialogViewAction>();
            }
            this.m_actions[eventName] = action;
        }

    }

}
