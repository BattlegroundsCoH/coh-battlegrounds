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
using Battlegrounds.Game.Database;
using Battlegrounds.Game.Database.Management;
using Battlegrounds.Game.DataCompany;
using Battlegrounds.Game.Gameplay;
using BattlegroundsApp.Dialogs.AddUnit;
using BattlegroundsApp.Views;

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

        public UnitSlotType UnitType { get; }
        private uint SlotOccupantID { get; set; }

        private List<Blueprint> SquadList => BlueprintManager.GetAllBlueprintsOfType(BlueprintType.SBP).Values.ToList();

        private CompanyBuilderView _companyBuilderView { get; set; }

        public UnitSlot(CompanyBuilderView view, UnitSlotType type) {
            InitializeComponent();
            _companyBuilderView = view;
            UnitType = type;
        }

        public void SetUnit(Squad squad) {
            if (squad is null) {
                this.StateChangeRequest(UnitSlotStateType.Add);
            } else {
                this.StateChangeRequest(UnitSlotStateType.Occupied);
                if (this.State is OccupiedState occupiedState) {
                    occupiedState.SetUnit(squad);
                    SlotOccupantID = squad.SquadID;
                }
            }
        }

        private void AddUnit_Click(object sender, RoutedEventArgs e) {
            var result = AddUnitDialogViewModel.ShowAddUnitDialog("Add Unit", SquadList, out Blueprint unit);

            if (result == AddUnitDialogResult.Add) {
                UnitBuilder unitBuilder = new UnitBuilder();
                unitBuilder.SetBlueprint(unit as SquadBlueprint);
                this._companyBuilderView.AddUnitToCompany(unitBuilder);
            }

        }

        private void ReplaceUnit_Click(object sender, RoutedEventArgs e) {
            var result = AddUnitDialogViewModel.ShowAddUnitDialog("Add Unit", SquadList, out Blueprint unit);

            if (result == AddUnitDialogResult.Add) {
                UnitBuilder unitBuilder = this._companyBuilderView.Builder.GetUnit(SlotOccupantID);
                unitBuilder.SetBlueprint(unit as SquadBlueprint);
                this._companyBuilderView.ReplaceUnitInCompany(unitBuilder);
            }
        }
    }
}
