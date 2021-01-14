using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using BattlegroundsApp.Dialogs.Service;
using BattlegroundsApp.Utilities;
using BattlegroundsApp.ViewModels;

namespace BattlegroundsApp.Dialogs.RenameCompany {
    public enum RenameCompanyDialogResult {
        Rename,
        Cancel
    }
    class RenameCompanyDialogViewModel : DialogControlBase<RenameCompanyDialogResult> {
        
        public ICommand RenameCommand { get; set; }
        public ICommand CancelCommand { get; set; }

        private string _newCompanyName;
        public string NewCompanyName {
            get {
                return this._newCompanyName;
            }

            set {
                this._newCompanyName = value;
                OnPropertyChanged(nameof(NewCompanyName));
            }
        }

        private RenameCompanyDialogViewModel(string title) {

            Title = title;
            RenameCommand = new RelayCommand<DialogWindow>(Rename);
            CancelCommand = new RelayCommand<DialogWindow>(Cancel);
            DialogCloseDefault = RenameCompanyDialogResult.Cancel;
        }

        public static RenameCompanyDialogResult ShowRenameCompanyDialog(string title) {
            var dialog = new RenameCompanyDialogViewModel(title);
            return dialog.ShowDialog();
        }

        private void Rename(DialogWindow window) {

            CloseDialogWithResult(window, RenameCompanyDialogResult.Rename);

        }

        private void Cancel(DialogWindow window) {

            CloseDialogWithResult(window, RenameCompanyDialogResult.Cancel);

        }
    }
}
