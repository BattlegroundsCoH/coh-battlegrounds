using Battlegrounds.Game.Database.Extensions;
using Battlegrounds.Game.Gameplay;
using BattlegroundsApp.Dialogs.YesNo;
using BattlegroundsApp.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BattlegroundsApp.Controls.CompanyBuilderControls {
    public partial class SquadSlotLarge : UserControl {

        public string SquadName { get; }
        public string SquadIcon { get; }
        public CostExtension SquadCost { get; }
        public byte SquadVeterancy { get; }
        public bool SquadIsTransported { get; }

        private uint SlotOccupantID { get; set; }

        public SquadSlotLarge(Squad squad) {
            SquadName = GameLocale.GetString(uint.Parse(squad.SBP.UI.ScreenName));
            SquadIcon = $"pack://application:,,,/Resources/ingame/unit_icons/{squad.SBP.UI.Icon}.png";
            SquadCost = squad.SBP.Cost;
            SquadVeterancy = squad.VeterancyRank;
            SquadIsTransported = squad.SupportBlueprint is not null;
            SlotOccupantID = squad.SquadID;
            InitializeComponent();
        }

        private void RemoveUnit(object sender, RoutedEventArgs e) {
            var result = YesNoDialogViewModel.ShowYesNoDialog("Remove Unit", "Are you sure? This action can not be undone.");

            if (result == YesNoDialogResult.Confirm) {
                //Remove unit here
            }
        }
    }
}
