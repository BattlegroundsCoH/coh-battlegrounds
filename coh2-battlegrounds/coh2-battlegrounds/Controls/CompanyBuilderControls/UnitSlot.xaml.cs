using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;

using Battlegrounds.Game.Database;
using Battlegrounds.Game.Database.Management;
using Battlegrounds.Game.DataCompany;
using Battlegrounds.Game.Gameplay;

using BattlegroundsApp.Dialogs.AddUnit;
using BattlegroundsApp.Dialogs.YesNo;
using BattlegroundsApp.Utilities;
using BattlegroundsApp.Views;

namespace BattlegroundsApp.Controls.CompanyBuilderControls {

    public enum UnitSlotType {
        [Description("infantry")]
        Infantry = 0,
        [Description("team_weapon")]
        Support = 1,
        [Description("vehicle")]
        Vehicle = 2,
        [Description("ability")]
        Ability = 3
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

        private List<SquadBlueprint> SquadList
            => BlueprintManager.GetCollection<SquadBlueprint>()
            .FilterByMod(_companyBuilderView.CompanyGUID)
            .Filter(x => x.Army == _companyBuilderView.CompanyFaction.ToString() && CompanyBuilderView.Category[(int)this.UnitType].IsValid(x)).ToList();

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
            var result = AddUnitDialogViewModel.ShowAddUnitDialog("Add Unit", SquadList, out SquadBlueprint unit);

            if (result == AddUnitDialogResult.Add) {
                UnitBuilder unitBuilder = new UnitBuilder();
                unitBuilder.SetBlueprint(unit);
                this._companyBuilderView.AddUnitToCompany(unitBuilder);
            }

        }

        private void ReplaceUnit_Click(object sender, RoutedEventArgs e) {
            var result = AddUnitDialogViewModel.ShowAddUnitDialog("Add Unit", SquadList, out SquadBlueprint unit);

            if (result == AddUnitDialogResult.Add) {
                UnitBuilder unitBuilder = this._companyBuilderView.Builder.GetUnit(SlotOccupantID);
                unitBuilder.SetBlueprint(unit);
                this._companyBuilderView.ReplaceUnitInCompany(unitBuilder);
            }
        }

        private void RemoveUnit_Click(object sender, RoutedEventArgs e) {
            var result = YesNoDialogViewModel.ShowYesNoDialog("Remove Unit", "Are you sure? This action can not be undone.");

            if (result == YesNoDialogResult.Confirm) {
                //this._companyBuilderView.RemoveUnitFromCompany(SlotOccupantID);
            }
        }
    }
}
