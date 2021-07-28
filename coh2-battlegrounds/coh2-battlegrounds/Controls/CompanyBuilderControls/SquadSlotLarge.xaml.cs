using Battlegrounds.Game.Database.Extensions;
using Battlegrounds.Game.Gameplay;

using BattlegroundsApp.Dialogs.YesNo;
using BattlegroundsApp.Resources;

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BattlegroundsApp.Controls.CompanyBuilderControls {

    public partial class SquadSlotLarge : UserControl {

        public string SquadName { get; }
        public string SquadIcon { get; }
        public CostExtension SquadCost { get; }
        public byte SquadVeterancy { get; }
        public bool SquadIsTransported { get; }

        private uint SlotOccupantID { get; }

        public event Action<SquadSlotLarge> OnClick;

        public SquadSlotLarge(Squad squad) {
            this.DataContext = this;
            this.SquadName = GameLocale.GetString(squad.SBP.UI.ScreenName);
            this.SquadIcon = $"pack://application:,,,/Resources/ingame/unit_icons/{squad.SBP.UI.Icon}.png";
            this.SquadCost = squad.SBP.Cost;
            this.SquadVeterancy = squad.VeterancyRank;
            this.SquadIsTransported = squad.SupportBlueprint is not null;
            this.SlotOccupantID = squad.SquadID;
            this.InitializeComponent();
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
