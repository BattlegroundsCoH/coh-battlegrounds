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

        public string SquadName { get; }
        public ImageSource SquadIcon { get; }
        public CostExtension SquadCost { get; }
        public ImageSource SquadVeterancy { get; }
        public bool SquadIsTransported { get; }

        public Squad SquadInstance { get; }

        public event Action<SquadSlotLarge> OnClick;

        public SquadSlotLarge(Squad squad) {

            this.DataContext = this;

            // Get basic info
            this.SquadInstance = squad;
            this.SquadName = GameLocale.GetString(this.SquadInstance.SBP.UI.ScreenName);
            this.SquadIcon = App.ResourceHandler.GetIcon("unit_icons", this.SquadInstance.SBP.UI.Icon);
            this.SquadCost = this.SquadInstance.GetCost();

            // Get veterancy
            if (this.SquadInstance.VeterancyRank > 0) {
                this.SquadVeterancy = new BitmapImage(new Uri($"pack://application:,,,/Resources/ingame/vet/vstar{this.SquadInstance.VeterancyRank}.png"));
            }

            // Set transport
            this.SquadIsTransported = this.SquadInstance.SupportBlueprint is not null;

            // Update UI 
            this.InitializeComponent();
            this.CostDisplay.Cost = this.SquadCost;

        }

        private void RemoveUnit(object sender, RoutedEventArgs e) {
            var result = YesNoDialogViewModel.ShowYesNoDialog("Remove Unit", "Are you sure? This action can not be undone.");

            if (result == YesNoDialogResult.Confirm) {
                //Remove unit here
            }
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
            => this.OnClick?.Invoke(this);

    }

}
