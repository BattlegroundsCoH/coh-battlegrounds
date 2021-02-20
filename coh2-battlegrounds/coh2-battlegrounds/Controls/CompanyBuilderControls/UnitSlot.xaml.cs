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
using Battlegrounds.Game.Gameplay;

namespace BattlegroundsApp.Controls.CompanyBuilderControls {

    public enum UnitSlotType {
        Infantry,
        Support,
        Vehicle,
        Ability
    }

    public enum UnitSlotStateType {
        Add,
        Occupied
    }

    /// <summary>
    /// Interaction logic for UnitSlot.xaml
    /// </summary>
    public partial class UnitSlot : UnitSlotControl {

        public UnitSlotType UnitType { get; set; }

        public UnitSlot() {
            InitializeComponent();
        }

        public void SetUnit(Squad squad) {
            if (squad is null) {
                this.StateChangeRequest(UnitSlotStateType.Add);
            } else {
                this.StateChangeRequest(UnitSlotStateType.Occupied);
                if (this.State is OccupiedState occupiedState) {
                    occupiedState.SetUnit(squad);
                }
            }
        }

        private void AddUnit_Click(object sender, RoutedEventArgs e) {

        }
    }
}
