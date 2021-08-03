using Battlegrounds.Game.Database.Extensions;
using Battlegrounds.Game.Gameplay;

using BattlegroundsApp.Dialogs.YesNo;
using BattlegroundsApp.Resources;

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BattlegroundsApp.Controls.CompanyBuilderControls {

    public partial class SquadSlotLarge : UserControl {

        public string SquadName { get; set; }
        public ImageSource SquadIcon { get; }
        public CostExtension SquadCost { get; set; }
        public ImageSource SquadVeterancy { get; set; }
        public bool SquadIsTransported { get; set; }

        public Squad SquadInstance { get; }

        public event Action<SquadSlotLarge> OnClick;

        public event Action<SquadSlotLarge> OnRemove;

        public SquadSlotLarge(Squad squad) {

            // Set squad instance
            this.SquadInstance = squad;

            // Set context
            this.DataContext = this;

            // Set data known not to change
            this.SquadIcon = App.ResourceHandler.GetIcon("unit_icons", this.SquadInstance.SBP.UI.Icon);

            // Update UI 
            this.InitializeComponent();

            // Refresh data
            this.RefreshData();

        }

        private void RefreshData() {

            // Refresh cost
            this.CostDisplay.Cost = this.SquadCost;

            // Get basic info
            this.SquadName = GameLocale.GetString(this.SquadInstance.SBP.UI.ScreenName);
            this.SquadCost = this.SquadInstance.GetCost();

            // Get veterancy
            if (this.SquadInstance.VeterancyRank > 0) {
                this.SquadVeterancy = new BitmapImage(new Uri($"pack://application:,,,/Resources/ingame/vet/vstar{this.SquadInstance.VeterancyRank}.png"));
            }

            // Set transport
            this.SquadIsTransported = this.SquadInstance.SupportBlueprint is not null;

        }

        private void RemoveUnit(object sender, RoutedEventArgs e) {
            if (YesNoDialogViewModel.ShowYesNoDialog("Remove Unit", "Are you sure? This action can not be undone.") is YesNoDialogResult.Confirm) {
                this.OnRemove?.Invoke(this);
            }
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
            => this.OnClick?.Invoke(this);

    }

}
