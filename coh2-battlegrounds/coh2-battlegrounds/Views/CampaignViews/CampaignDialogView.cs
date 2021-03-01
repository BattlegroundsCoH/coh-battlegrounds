using System.Windows.Controls;

namespace BattlegroundsApp.Views.CampaignViews {
    
    public abstract class CampaignDialogView : UserControl {

        protected abstract void OnOpened();

        public void ShowDialogView(Canvas owner) {

            // Add self to dialog
            owner.Children.Add(this);
            this.DataContext = this;

            // Set Z-index
            this.SetValue(Panel.ZIndexProperty, 1000);

            // Invoke events
            this.OnOpened();

        }

    }

}
