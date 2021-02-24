using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Battlegrounds.Game.Database;
using BattlegroundsApp.Dialogs.Service;
using BattlegroundsApp.Utilities;

namespace BattlegroundsApp.Dialogs.AddUnit {
    public enum AddUnitDialogResult {
        Add,
        Cancel
    }
    public class AddUnitDialogViewModel : DialogControlBase<AddUnitDialogResult> {

        public ICommand AddCommand { get; set; }
        public ICommand CancelCommand { get; set; }

        private Blueprint _selectedUnit;
        public Blueprint SelectedUnit { 
            get {
                return this._selectedUnit;
            }

            set {
                this._selectedUnit = value;
                OnPropertyChanged(nameof(SelectedUnit));
            }
        }

        public List<Blueprint> UnitList { get; set; }

        private AddUnitDialogViewModel(string title, List<Blueprint> unitList) {

            Title = title;
            AddCommand = new RelayCommand<DialogWindow>(Add);
            CancelCommand = new RelayCommand<DialogWindow>(Cancel);
            UnitList = unitList;
            DialogCloseDefault = AddUnitDialogResult.Cancel;

        }

        public static AddUnitDialogResult ShowAddUnitDialog(string title, List<Blueprint> unitList, out Blueprint unit) {
            var dialog = new AddUnitDialogViewModel(title, unitList);
            var result = dialog.ShowDialog();
            unit = dialog.SelectedUnit;
            return result;
        }

        private void Add(DialogWindow window) {

            CloseDialogWithResult(window, AddUnitDialogResult.Add);

        }

        private void Cancel(DialogWindow window) {

            CloseDialogWithResult(window, AddUnitDialogResult.Cancel);

        }

        public override void CloseDialogWithResult(DialogWindow dialog, AddUnitDialogResult result) {

            DialogResult = result;

            if (dialog != null) dialog.DialogResult = true;

        }

    }
}
